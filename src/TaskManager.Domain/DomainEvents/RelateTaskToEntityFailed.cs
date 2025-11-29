using MediatR;
using System;

namespace TaskManager.Domain.DomainEvents
{
    public class RelateTaskToEntityFailed : INotification
    {
        public RelateTaskToEntityFailed(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; set; }
        public ErrorData Error { get; }
    }
}
