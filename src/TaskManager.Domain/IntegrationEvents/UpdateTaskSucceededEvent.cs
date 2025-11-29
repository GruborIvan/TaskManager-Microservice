namespace TaskManager.Domain.IntegrationEvents
{
    public class UpdateTaskSucceededEvent : IntegrationEvent
    {
        public UpdateTaskSucceededEvent(Models.Task task) : base()
        {
            Task = task;
        }

        public Models.Task Task { get; }
    }
}
