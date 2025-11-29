using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskRelated : INotification
    {
        public TaskRelated(Relation relation) => Relation = relation;

        public Relation Relation { get; }
    }
}
