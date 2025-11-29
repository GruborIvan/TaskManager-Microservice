using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class Finalized : INotification
    {
        public Task Task { get; }

        public Finalized(Task task) => Task = task;
    }
}
