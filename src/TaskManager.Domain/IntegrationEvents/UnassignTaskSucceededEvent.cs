using TaskManager.Domain.Models;

namespace TaskManager.Domain.IntegrationEvents
{
    public class UnassignTaskSucceededEvent : IntegrationEvent
    {
        public UnassignTaskSucceededEvent(Assignment assignment)
        {
            Assignment = assignment;
        }

        public Assignment Assignment { get; }
    }
}
