using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class StoreComment : BaseCommand<Comment>
    {
        public StoreComment(
            Guid taskId,
            string text,
            Guid initiatedBy,
            DateTime createdDate) : base(initiatedBy)
        {
            TaskId = taskId;
            Text = text;
            CreatedDate = createdDate;
        }

        public Guid TaskId { get; }
        public string Text { get; }
        public DateTime CreatedDate { get; }
    }
}
