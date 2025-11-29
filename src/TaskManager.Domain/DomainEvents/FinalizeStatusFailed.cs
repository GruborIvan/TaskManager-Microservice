using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class FinalizeStatusFailed : INotification
    {
        public FinalizeStatusFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
