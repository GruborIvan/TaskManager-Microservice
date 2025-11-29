using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class StatusUpdated : INotification
    {
        public StatusUpdated(Task task) => Task = task;

        public Task Task { get; }
    }
}
