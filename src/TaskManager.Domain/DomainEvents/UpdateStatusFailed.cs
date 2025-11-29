using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class UpdateStatusFailed : INotification
    {
        public UpdateStatusFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
