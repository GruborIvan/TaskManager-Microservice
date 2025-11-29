using System;
using MediatR;

namespace TaskManager.Domain.DomainEvents
{
    public class UpdateTaskFailed : INotification
    {
        public UpdateTaskFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
