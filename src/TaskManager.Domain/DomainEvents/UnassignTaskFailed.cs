using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class UnassignTaskFailed : INotification
    {
        public UnassignTaskFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
