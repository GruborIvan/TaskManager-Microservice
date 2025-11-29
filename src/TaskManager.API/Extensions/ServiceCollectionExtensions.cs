using GraphQL.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System.Collections.Generic;
using TaskManager.Infrastructure.Models;

namespace TaskManager.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQLEntityFramework(this IServiceCollection services,
            IMvcBuilder mvcBuilder)
        {
            // Newtonsoft supports handling reference loops, while System.Text.Json doesn't
            // See https://github.com/dotnet/runtime/issues/29900
            mvcBuilder.AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            // Allowed synchronous IO because Newtonsoft library in GraphQL requires it to work
            // See https://github.com/graphql-dotnet/graphql-dotnet/issues/1116
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // Necessary for the complex registration of EfGraphQlService
            EfGraphQLConventions.RegisterInContainer<TasksDbContext>(services);
            // Necessary in order to use GraphQL connection types
            EfGraphQLConventions.RegisterConnectionTypesInContainer(services);

            return services;
        }

        public static IHealthChecksBuilder AddServiceHealthCheck<T>(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default) where T : class, IHealthCheck
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                sp => sp.GetRequiredService(typeof(T)) as T,
                failureStatus,
                tags));
        }
    }
}
