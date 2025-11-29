using MediatR;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class CommentAdded : INotification
    {
        public CommentAdded(Comment comment) => Comment = comment;

        public Comment Comment { get; }
    }
}
