using TaskManager.Domain.Models;

namespace TaskManager.Domain.IntegrationEvents
{
    public class AssignTaskToEntitySucceededEvent : IntegrationEvent
    {
        public AssignTaskToEntitySucceededEvent(
            Assignment assignment
            ) : base()
        {
            Assignment = assignment;
        }

        public Assignment Assignment { get; }
    }
}
