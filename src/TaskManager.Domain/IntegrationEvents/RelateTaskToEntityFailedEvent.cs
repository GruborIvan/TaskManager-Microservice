using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.IntegrationEvents
{
    public class RelateTaskToEntityFailedEvent : IntegrationEvent
    {
        public RelateTaskToEntityFailedEvent(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
