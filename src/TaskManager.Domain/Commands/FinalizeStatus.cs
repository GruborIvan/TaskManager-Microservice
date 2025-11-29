using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class FinalizeStatus : BaseCommand<Task>
    {
        public Guid TaskId { get; }
        public bool FinalState { get; }
        public string Status { get; }

        public FinalizeStatus(
            Guid taskId,
            string status,
            Guid initiatedBy,
            bool finalState = true) : base(initiatedBy)
        {
            TaskId = taskId;
            FinalState = finalState;
            Status = status;
        }
    }
}
