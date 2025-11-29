using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class CommentNotFoundException : Exception
    {
        public CommentNotFoundException(Guid commentId)
            : base($"{nameof(Comment)} with {nameof(Comment.CommentId)}: {commentId} not found.") { }
    }
}
