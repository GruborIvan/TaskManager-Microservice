namespace TaskManager.Domain.IntegrationEvents
{
    public class UpdateTaskStatusSucceededEventV2 : IntegrationEvent
    {
        public UpdateTaskStatusSucceededEventV2(Models.Task task) : base()
        {
            Task = task;
        }

        public Models.Task Task { get; }
    }
}
