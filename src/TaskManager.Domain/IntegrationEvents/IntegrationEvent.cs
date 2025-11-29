using System;

namespace TaskManager.Domain.IntegrationEvents
{
    public abstract class IntegrationEvent
    {
        protected IntegrationEvent()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public DateTime CreatedDate { get; }
    }
}
