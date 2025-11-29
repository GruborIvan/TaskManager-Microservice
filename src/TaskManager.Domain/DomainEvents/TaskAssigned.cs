using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskAssigned : INotification
    {
        public TaskAssigned(Assignment assignment) => Assignment = assignment;

        public Assignment Assignment { get; }
    }
}
