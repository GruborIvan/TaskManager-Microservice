using System;
using System.IO;
using System.Reflection;
using GraphQL.Server.Ui.Playground;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TaskManager.API.Extensions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Azure.Core;
using TaskManager.API.Modules;
using TaskManager.Infrastructure.Modules;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FiveDegrees.Audit.Http.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Providers;
using TaskManager.Infrastructure.Services;
using Microsoft.ApplicationInsights;
using Serilog.Core;
using Serilog.Sinks.ApplicationInsights;
using Serilog;

namespace TaskManager.API
{
    public class Startup
    {
        private static readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private readonly TokenCredential _tokenCredential;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _tokenCredential = AzureCredentials.GetCredentials();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();
            if (!string.IsNullOrWhiteSpace(Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY")))
            {
                services.AddSingleton<ILogEventSink>(
                    p => new ApplicationInsightsSink(
                        p.GetRequiredService<TelemetryClient>(),
                        TelemetryConverter.Traces));
            }
            var mvcBuilder = services.AddControllers();

            services.AddAuditMiddleware(
               options =>
               {
                   options.ServiceBusConnectionString = Configuration.GetConnectionString("ServiceBusConnectionString");
                   options.TokenProvider = new AzureIdentityServiceBusCredentialAdapter(_tokenCredential);
                   options.Enabled = Configuration.GetValue<bool>("TaskManagerConfiguration:AuditEnabled");
               });

            services.AddHealthChecks()
               .AddCheck(
                   name: "TaskManager Api",
                   () => HealthCheckResult.Healthy("Task Manager Api is alive"),
                   tags: new[] { "liveness", "api" })
               .AddDbContextCheck<TasksDbContext>(tags: new[] { "readiness", "api" });

            services.AddGraphQLEntityFramework(mvcBuilder);
            services.AddHttpClient();

            services.TryAddScoped<IContextAccessor, Helpers.HttpContextAccessor>();

            RegisterDbContext(services);
            RegisterAutomapper(services);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = _assemblyName, Version = "v1" });
                c.OperationFilter<SwaggerHeaderExtension>();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{_assemblyName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            ConfigureServicesAuthorization(services);
        }

        protected virtual void RegisterAutomapper(IServiceCollection services)
        {
            services.AddAutoMapper(configuration =>
            {
                configuration.AddProfile(new AutoMapperProfileApi());
                configuration.AddProfile(new AutoMapperProfile());
            },
            new Assembly[] { typeof(AutoMapperProfileApi).Assembly },
            serviceLifetime: ServiceLifetime.Scoped);
        }

        protected virtual void RegisterDbContext(IServiceCollection services)
        {
            services.AddDbContext<TasksDbContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("TaskDbConnectionString")));
        }

        protected virtual void ConfigureServicesAuthorization(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", _assemblyName);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // GraphQL.Server middleware that adds the GraphQL enpoint which takes the 
            // responsibility of creating the controller away from us
            app.UseGraphQLPlayground(options: new GraphQLPlaygroundOptions
            {
                GraphQLEndPoint = "/api/graphql",
                Headers = new Dictionary<string, object>()
                {
                    { "GraphQlPlayground", true }
                }
            });

            app.UseRouting();

            ConfigureAuditMiddleware(app);

            ConfigureAuthorization(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected virtual void ConfigureAuthorization(IApplicationBuilder app)
        {
            app.UseAuthentication();
        }

        protected virtual void ConfigureAuditMiddleware(IApplicationBuilder app)
        {
            app.UseAuditMiddleware();
        }
    }
}
