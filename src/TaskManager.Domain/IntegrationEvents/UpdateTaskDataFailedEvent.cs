using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.IntegrationEvents
{
    public class UpdateTaskDataFailedEvent : IntegrationEvent
    {
        public UpdateTaskDataFailedEvent(Guid taskId, ErrorData error)
        {
            TaskId = taskId;
            Error = error;
        }

        public Guid TaskId { get; }
        public ErrorData Error { get; }
    }
}
