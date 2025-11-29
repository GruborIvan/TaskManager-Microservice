using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class UpdateTask : BaseCommand<Task>
    {
        public UpdateTask(
            Guid taskId, 
            string data, 
            string status,
            Guid initiatedBy, 
            bool finalState = false) : base(initiatedBy)
        {
            TaskId = taskId;
            Data = data;
            FinalState = finalState;
            Status = status;
        }

        public Guid TaskId { get; }
        public string Data { get; }
        public bool FinalState { get; }
        public string Status { get; }
    }
}
