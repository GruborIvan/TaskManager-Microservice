using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.IntegrationEvents
{
    public class CreateTaskFailedEvent : IntegrationEvent
    {
        public CreateTaskFailedEvent(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
