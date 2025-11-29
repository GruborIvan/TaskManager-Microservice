using System;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure.Models
{
    public class CommentDbo : Entity
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedById { get; set; }

        public TaskDbo Task { get; set; }
    }
}
