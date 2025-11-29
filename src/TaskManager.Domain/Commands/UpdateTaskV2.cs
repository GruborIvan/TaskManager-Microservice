using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class UpdateTaskV2 : BaseCommand<Task>
    {
        public UpdateTaskV2(
            Guid taskId,
            string data,
            string subject,
            Guid initiatedBy) : base(initiatedBy)
        {
            TaskId = taskId;
            Data = data;
            Subject = subject;
        }

        public Guid TaskId { get; }
        public string Data { get; }
        public string Subject { get; }
    }
}
