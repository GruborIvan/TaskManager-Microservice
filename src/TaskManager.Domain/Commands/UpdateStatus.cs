using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class UpdateStatus : BaseCommand<Task>
    {
        public UpdateStatus(
            Guid taskId,
            string status,
            Guid initiatedBy,
            bool finalState = false) : base(initiatedBy)
        {
            TaskId = taskId;
            FinalState = finalState;
            Status = status;
        }

        public Guid TaskId { get; }
        public bool FinalState { get; }
        public string Status { get; }
    }
}
