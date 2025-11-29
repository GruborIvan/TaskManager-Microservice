using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.TestHelpers;
using Rebus.Transport.InMem;
using TaskManager.BackgroundWorker;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.BackgroundWorker.Helpers;
using TaskManager.BackgroundWorker.Modules;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Modules;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public abstract class TestFixture : IDisposable
    {
        protected readonly IHostBuilder _hostBuilder;
        protected Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        protected readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();
        protected readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        protected readonly InMemNetwork _network = new InMemNetwork();
        protected readonly InMemorySubscriberStore _subscriberStore = new InMemorySubscriberStore();
        protected readonly EventWaitHandle _msgHandled = new ManualResetEvent(initialState: false);
        protected readonly BuiltinHandlerActivator _subscriberActivator = new BuiltinHandlerActivator();
        private readonly BuiltinHandlerActivator _publisherActivator = new BuiltinHandlerActivator();
        private IHost _host;
        protected (string blobName, Stream stream) blobData;
        protected string exceptionMessage = "";

        private bool disposedValue;

        public TestFixture()
        {
            // Additional configuration containing fake secrets and configurations
            var json = "{\"ConnectionStrings\": {\"AppInsightsConnectionString\": \"InstrumentationKey=11111111-2222-3333-4444-555555555555\"}}";

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();

            _hostBuilder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new AutoMapperModule(typeof(InfrastructureModule).Assembly));
                    builder.RegisterModule<MediatRModule>();
                    builder.RegisterModule(new InfrastructureModule(config));

                    builder.RegisterHandlersFromAssemblyOf<AssignTaskToEntityMsgHandler>();

                    _mockEventStreamingService.Setup(service => service.SendAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                        .Verifiable();
                    builder.Register(c => _mockEventStreamingService.Object).As<IEventStreamingService>();

                    _mockEventNotificationService.Setup(service => service.SendAsync(It.IsAny<object>(), It.IsAny<string>()))
                        .Verifiable();
                    builder.Register(c => _mockEventNotificationService.Object).As<IEventNotificationService>();

                    builder.RegisterType<FakeBus>().As<Rebus.Bus.IBus>();

                    var mockHandler = new Mock<HttpMessageHandler>();
                    mockHandler.Protected()
                    // Setup the PROTECTED method to mock
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    // prepare the expected response of the mocked http call
                    .ReturnsAsync(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK
                    })
                    .Verifiable();

                    var mockHttpClientFactory = new Mock<IHttpClientFactory>();
                    mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHandler.Object));
                    builder.Register(c => mockHttpClientFactory.Object).As<IHttpClientFactory>();

                    var mockBlobContainerClient = new Mock<BlobContainerClient>();
                    mockBlobContainerClient
                        .Setup(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                        .Callback((string name, Stream content, CancellationToken ct) =>
                        {
                            blobData.blobName = name;
                            blobData.stream = content;
                        });
                    var mockBlobServiceClientService = new Mock<BlobServiceClient>();
                    mockBlobServiceClientService
                        .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                        .Returns(mockBlobContainerClient.Object);
                    builder.Register(c => mockBlobServiceClientService.Object).As<BlobServiceClient>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.TryAddScoped<IContextAccessor, RebusContextAccessor>();
                    services.AddHttpClient();

                    var dbName = Guid.NewGuid().ToString();
                    services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase(dbName));
                })
                .ConfigureWebHost(conf =>
                {
                    conf.UseTestServer();
                    conf.Configure(_ => { });
                    conf.UseConfiguration(config);
                });
        }

        public RebusConfigurer ResolveSubscriber<T>()
        {
            var handler = Resolve<IHandleMessages<T>>();
            var failedHandler = Resolve<IHandleMessages<IFailed<T>>>();
            _subscriberActivator.Register(x => handler);
            _subscriberActivator.Register(x => failedHandler);

            return CreateSubscriber(_subscriberActivator);
        }

        public RebusConfigurer CreateSubscriber(BuiltinHandlerActivator handlerActivator)
        {
            var subscriber = Configure.With(handlerActivator)
                .Transport(t => t.UseInMemoryTransport(_network, "bpmu2"))
                .Subscriptions(s => s.StoreInMemory(_subscriberStore))
                .Options(b => b.SimpleRetryStrategy(maxDeliveryAttempts: 2, secondLevelRetriesEnabled: true))
                .Events(e =>
                {
                    e.AfterMessageHandled += (bus, headers, message, context, args) =>
                    {
                        var exceptionOrNull = context.Load<Exception>();
                        if (exceptionOrNull != null)
                        {
                            exceptionMessage = exceptionOrNull.Message;
                        }
                        _msgHandled.Set();
                    };
                });

            return subscriber;
        }

        public Guid InitiatorId { get; private set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public IHost StartHost()
        {
            return _host = _hostBuilder.Start();
        }

        public RebusConfigurer ResolvePublisher()
        {
            var publisher = Configure.With(_publisherActivator)
                .Transport(t => t.UseInMemoryTransportAsOneWayClient(_network))
                .Subscriptions(s => s.StoreInMemory(_subscriberStore));

            return publisher;
        }

        public async Task Publish(object message, Dictionary<string, string> headers)
        {
            var bus = ResolvePublisher().Start();
            await bus.Publish(message, headers);
        }

        public async Task Subscribe<T>()
        {
            var bus = ResolveSubscriber<T>().Start();
            await bus.Subscribe<T>();
        }

        public TResult Resolve<TResult>()
        {
            return _host.Services.GetAutofacRoot().BeginLifetimeScope().Resolve<TResult>();
        }

        public void RegisterMockHttpClient()
        {
            _hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
               _mockHttpMessageHandler
                    .Protected()
                    // Setup the PROTECTED method to mock
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    // prepare the expected response of the mocked http call
                    .ReturnsAsync(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK
                    })
                    .Verifiable();
                var mockFactory = new Mock<IHttpClientFactory>();
                mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHttpMessageHandler.Object));
                builder.Register(c => mockFactory.Object).As<IHttpClientFactory>();
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _subscriberActivator.Dispose();
                    _publisherActivator.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
