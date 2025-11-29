using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskUnassigned : INotification
    {
        public TaskUnassigned(Assignment assignment) => Assignment = assignment;

        public Assignment Assignment { get; }
    }
}
