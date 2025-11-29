using Autofac;
using Microsoft.Extensions.Configuration;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace TaskManager.Infrastructure.Modules
{
    public class RebusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterRebus((configurer, context) =>
            {
                configurer.Logging(l => l.Serilog())
                    .Options(o => o.SimpleRetryStrategy("error"))
                    .Routing(r => r.TypeBased());

                var config = context.Resolve<IConfiguration>();
                var connectionString = config.GetConnectionString("ServiceBusConnectionString");

                if (string.IsNullOrEmpty(connectionString))
                {
                    return configurer.Transport(t => t.UseInMemoryTransportAsOneWayClient(new InMemNetwork()))
                           .Subscriptions(s => s.StoreInMemory());
                }

                return configurer.Transport(t => t.UseAzureServiceBusAsOneWayClient(connectionString));
            });
        }
    }
}
