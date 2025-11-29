using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using TaskManager.API;
using TaskManager.API.Modules;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Modules;

namespace TaskManager.Tests.IntegrationTests.API
{
    public abstract class TestFixture
    {
        protected readonly IHostBuilder hostBuilder;

        protected IHost host;
        protected HttpClient client;

        protected TestFixture()
        {
            // Additional configuration containing fake secrets and configurations
            var json = "{\"ConnectionStrings\": {\"AppInsightsConnectionString\": \"InstrumentationKey=11111111-2222-3333-4444-555555555555\"}}";

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();

            hostBuilder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new InfrastructureModule(config));
                    builder.RegisterModule<MediatRModule>();
                    builder.RegisterModule<RebusModule>();
                    builder.RegisterModule<GraphQlModule>();

                    // Mock the event service so no events are sent to EventHub
                    var mockEventHubService = new Mock<IEventStreamingService>().Object;
                    builder.Register(c => mockEventHubService).As<IEventStreamingService>();

                    var mockEventGridService = new Mock<IEventNotificationService>().Object;
                    builder.Register(c => mockEventGridService).As<IEventNotificationService>();

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
                })
                .ConfigureWebHost(conf =>
                {
                    conf.UseTestServer();
                    conf.UseStartup<TestStartup>();
                    conf.UseConfiguration(config);

                    // Ignore the StartupStaging class assembly as the "entry point" and instead point it to this app
                    conf.UseSetting(WebHostDefaults.ApplicationKey, typeof(Program).Assembly.FullName);
                });
        }

        protected TResult Resolve<TResult>(IHost host)
        {
            return host.Services.GetAutofacRoot().Resolve<TResult>();
        }

        private class TestStartup : Startup
        {
            public TestStartup(IConfiguration configuration)
                : base(configuration)
            {
            }

            protected override void RegisterDbContext(IServiceCollection services)
            {
                var dbName = Guid.NewGuid().ToString();
                services.AddDbContext<TasksDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            }

            protected override void ConfigureAuthorization(IApplicationBuilder app)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            protected override void ConfigureAuditMiddleware(IApplicationBuilder app)
            {
            }

            protected override void ConfigureServicesAuthorization(IServiceCollection services)
            {
               services.AddAuthorization(options =>
               {
                   options.AddPolicy("CanViewTask", policy =>
                       policy.RequireAssertion(context =>
                       context.User.HasClaim(c => c.Type == "CanViewTask")));

                   options.AddPolicy("CanSearchTask", policy =>
                       policy.RequireAssertion(context =>
                       context.User.HasClaim(c => c.Type == "CanSearchTask")));

                   options.AddPolicy("CanCreateTaskMsg", policy =>
                       policy.RequireAssertion(context =>
                       context.User.HasClaim(c => c.Type == "CanCreateTaskMsg")));
               });
            }
        }
    }
}
