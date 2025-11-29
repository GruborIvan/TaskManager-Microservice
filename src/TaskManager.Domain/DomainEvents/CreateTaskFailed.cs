using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class CreateTaskFailed : INotification
    {
        public CreateTaskFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
