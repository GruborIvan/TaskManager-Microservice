using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.IntegrationEvents
{
    public class FinalizeTaskStatusFailedEvent : IntegrationEvent
    {
        public FinalizeTaskStatusFailedEvent(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
