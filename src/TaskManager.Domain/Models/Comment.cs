using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.Models
{
    public class Comment : Entity
    {
        public Guid CommentId { get; }
        public Guid TaskId { get; }
        public string Text { get; }
        public Guid CreatedBy { get; }
        public DateTime CreatedDate { get; }

        public Comment(
            Guid commentId,
            Guid taskId,
            string text)
        {
            CommentId = commentId;
            TaskId = taskId;
            Text = text;
        }

        public Comment(
            Guid commentId,
            Guid taskId,
            string text,
            Guid createdBy,
            DateTime createdDate) : this(commentId, taskId, text)
        {
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }

        public Comment(
            Guid taskId,
            string text,
            Guid createdBy,
            DateTime createdDate) : this(Guid.NewGuid(), taskId, text, createdBy, createdDate)
        {
            AddDomainEvent(new CommentAdded(this));
        }
    }
}
