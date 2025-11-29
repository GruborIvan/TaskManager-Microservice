using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class UpdateData : BaseCommand<Task>
    {
        public UpdateData(
            Guid taskId,
            string data,
            Guid initiatedBy) : base(initiatedBy)
        {
            TaskId = taskId;
            Data = data;
        }

        public Guid TaskId { get; }
        public string Data { get; }
    }
}
