using Autofac;
using Autofac.Extensions.DependencyInjection;
using Matrix.HealthCheckers.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using Azure.Core;
using TaskManager.BackgroundWorker.HealthChecks;
using TaskManager.BackgroundWorker.Helpers;
using TaskManager.BackgroundWorker.Modules;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Modules;
using TaskManager.Infrastructure.Services;
using Microsoft.ApplicationInsights;
using Serilog.Core;
using Serilog.Sinks.ApplicationInsights;
using System.Linq;
using TaskManager.BackgroundWorker.Utilities;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;

namespace TaskManager.BackgroundWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            using var builder = CreateHostBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new Modules.RebusModule());
                    builder.RegisterModule(new InfrastructureModule(Configuration));
                    builder.RegisterModule(new MediatRModule());
                    builder.RegisterModule(new AutoMapperModule(typeof(InfrastructureModule).Assembly));
                })
                .Build();
            builder.Run();
        }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                )
                .ConfigureServices((hostContext, services) =>
                {
                    TokenCredential credentials = AzureCredentials.GetCredentials();

                    var config = hostContext.Configuration;

                    services.AddHostedService<Worker>()
                        .AddApplicationInsightsTelemetryWorkerService(config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"));
                    ServiceDescriptor performanceCounterService = services.FirstOrDefault
                                               (t => t.ImplementationType == typeof(PerformanceCollectorModule));
                    if (performanceCounterService != null)
                    {
                        services.Remove(performanceCounterService);
                    }

                    services.AddSingleton<ITelemetryInitializer, SamplingConfigurationTelemetryInitializer>();
                    services.AddSingleton<Microsoft.ApplicationInsights.WorkerService.ITelemetryProcessorFactory, FilterDependencyTelemetryProcessorFactory>();

                    if (!string.IsNullOrWhiteSpace(Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY")))
                    {
                        services.AddSingleton<ILogEventSink>(
                             p => new ApplicationInsightsSink(p.GetRequiredService<TelemetryClient>(),
                             TelemetryConverter.Traces));
                    }
                    services.AddHealthChecks()
                            .AddDbContextCheck<TasksDbContext>(tags: new[] { "liveness", "api" })
                            .AddAzureServiceBusQueue(
                                uri: Configuration.GetValue<string>("TaskManagerConfiguration:ServiceBusUri"),
                                queueName: Configuration.GetValue<string>("TaskManagerConfiguration:ServiceBusQueueName"),
                                tokenCredential: credentials,
                                tags: new[] { "liveness", "api" });

                    services.Configure<HealthCheckPublisherOptions>(options =>
                    {
                        options.Delay = TimeSpan.FromSeconds(5);
                        options.Predicate = (check) => check.Tags.Contains("liveness");
                    });

                    services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>();
                    services.TryAddScoped<IContextAccessor, RebusContextAccessor>();
                    services.AddHttpClient();

                    services.AddDbContext<TasksDbContext>(opt =>
                        opt.UseSqlServer(config.GetConnectionString("TaskDbConnectionString")));

                   
                });
    }
}
