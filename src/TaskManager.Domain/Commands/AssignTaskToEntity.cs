using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class AssignTaskToEntity : BaseCommand<Task>
    {
        public AssignTaskToEntity(
            Guid taskId, 
            Assignment assignment,
            Guid initiatedBy) : base(initiatedBy)
        {
            TaskId = taskId;
            Assignment = assignment;
        }

        public Guid TaskId { get; }
        public Assignment Assignment { get; }
    }
}
