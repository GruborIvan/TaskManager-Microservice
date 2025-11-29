using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class StoreCommentFailed : INotification
    {
        public StoreCommentFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
