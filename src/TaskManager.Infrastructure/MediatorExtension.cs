using MediatR;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Infrastructure.Models;

namespace TaskManager.Infrastructure
{
    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, TasksDbContext ctx)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Domain.Models.Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent);
        }
    }
}
