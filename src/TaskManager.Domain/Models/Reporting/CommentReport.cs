using System;

namespace TaskManager.Domain.Models.Reporting
{
    public class CommentReport
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
    }
}
