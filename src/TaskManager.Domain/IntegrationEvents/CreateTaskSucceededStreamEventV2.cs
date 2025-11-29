namespace TaskManager.Domain.IntegrationEvents
{
    public class CreateTaskSucceededStreamEventV2 : IntegrationEvent
    {
        public CreateTaskSucceededStreamEventV2(Models.Task task) : base()
        {
            Task = task;
        }

        public Models.Task Task { get; }
    }
}
