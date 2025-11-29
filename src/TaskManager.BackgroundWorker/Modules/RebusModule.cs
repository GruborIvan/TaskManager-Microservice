using Autofac;
using FiveDegrees.Audit.Rebus.Extensions;
using FiveDegrees.Messages.Task;
using Microsoft.Extensions.Configuration;
using Rebus.Config;
using System.Linq;
using Azure.Core;
using Rebus.Retry.Simple;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.Infrastructure.Providers;
using TaskManager.Infrastructure.Services;

namespace TaskManager.BackgroundWorker.Modules
{
    public class RebusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterRebus((configurer, context) =>
            {
                var config = context.Resolve<IConfiguration>();
                var connectionString = config.GetConnectionString("ServiceBusConnectionString");
                var queueName = config.GetSection("TaskManagerConfiguration").GetValue<string>("ServiceBusQueueName");
                TokenCredential tokenCredential = AzureCredentials.GetCredentials();

                return configurer
                        .Logging(l => l.Serilog())
                        .Transport(t => t.UseAzureServiceBus(connectionString, queueName, new AzureIdentityServiceBusCredentialAdapter(tokenCredential)))
                        .Options(o =>
                        {
                            o.SimpleRetryStrategy(secondLevelRetriesEnabled: true);

                            o.AddFdsAudit(config.GetValue<bool>("TaskManagerConfiguration:AuditEnabled"));
                        });
            });

            builder.RegisterHandlersFromAssemblyOf<CreateTaskMsgHandler>();
        }
    }
}
