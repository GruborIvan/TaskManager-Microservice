using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class UnassignTask : BaseCommand<Task>
    {
        public UnassignTask(
            Guid taskId,
            Guid initiatedBy) : base(initiatedBy)
        {
            TaskId = taskId;
        }

        public Guid TaskId { get; }
    }
}
