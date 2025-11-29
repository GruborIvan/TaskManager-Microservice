using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskUpdated : INotification
    {
        public TaskUpdated(Task task)
        {
            Task = task;
        }

        public Task Task { get; }
    }
}
