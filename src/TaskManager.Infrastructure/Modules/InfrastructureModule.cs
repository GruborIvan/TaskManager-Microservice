using System;
using Autofac;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Infrastructure.Modules
{
    public class InfrastructureModule : Module
    {
        private readonly IConfiguration _configuration;

        public InfrastructureModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            builder.RegisterType<CallbackService>().As<ICallbackService>();
            builder.RegisterType<TaskRepository>().As<ITaskRepository>();
            builder.RegisterType<CommentRepository>().As<ICommentRepository>();
            builder.RegisterType<RelationRepository>().As<IRelationRepository>();
            builder.RegisterType<EventHubService>().As<IEventStreamingService>();
            builder.RegisterType<EventGridService>()
                .WithParameter("client", new EventGridClient(new TopicCredentials(_configuration.GetSection("EventGrid")["TopicKey"])))
                .WithParameter("topicEndpoint", _configuration.GetSection("EventGrid")["TopicEndpoint"])
                .As<IEventNotificationService>();
            builder.RegisterType<ReportingRepository>().As<IReportingRepository>();
            builder.RegisterType<ReportingService>()
                .WithParameter("fileSystemName", _configuration.GetSection("DwhReporting")["AzureDataLakeServiceFileSystemName"])
                .As<IReportingService>();

            RegisterEventHubProducerClient(builder);
            RegisterEventGridProducerClient(builder);
            RegisterAzureBlobServiceClient(builder);
        }

        private void RegisterAzureBlobServiceClient(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var blobServiceEndpoint = _configuration.GetSection("DwhReporting")["AzureDataLakeServiceEndpoint"];

                TokenCredential tokenCredential = AzureCredentials.GetCredentials();

                return new BlobServiceClient(
                    new Uri(blobServiceEndpoint),
                    tokenCredential);
            });
        }

        private void RegisterEventHubProducerClient(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var config = c.Resolve<IConfiguration>();

                var connectionString = config.GetConnectionString("EventHubConnectionString");
                var eventHubName = config.GetSection("TaskManagerConfiguration").GetValue<string>("EventHubName");
                TokenCredential tokenCredential = AzureCredentials.GetCredentials();

                return new EventHubProducerClient(connectionString, eventHubName, tokenCredential);

            })
            .OnRelease(instance => instance.DisposeAsync().GetAwaiter().GetResult())
            .SingleInstance();
        }

        private void RegisterEventGridProducerClient(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var config = c.Resolve<IConfiguration>();
                var contextAccessor = c.Resolve<IContextAccessor>();

                var client = new EventGridClient(new TopicCredentials(config.GetSection("EventGrid")["TopicKey"]));

                return new EventGridService(client, contextAccessor, config.GetSection("EventGrid")["TopicEndpoint"]);

            }).InstancePerLifetimeScope();
        }
    }
}
