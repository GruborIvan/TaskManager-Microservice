using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class DataUpdated : INotification
    {
        public Task Task { get; }

        public DataUpdated(Task task) => Task = task;
    }
}
