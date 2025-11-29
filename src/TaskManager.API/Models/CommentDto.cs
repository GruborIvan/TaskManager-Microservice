using System;

namespace TaskManager.API.Models
{
    public class CommentDto
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }
    }
}