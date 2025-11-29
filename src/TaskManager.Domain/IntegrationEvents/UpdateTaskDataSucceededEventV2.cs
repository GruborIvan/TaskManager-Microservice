namespace TaskManager.Domain.IntegrationEvents
{
    public class UpdateTaskDataSucceededEventV2 : IntegrationEvent
    {
        public UpdateTaskDataSucceededEventV2(Models.Task task) : base()
        {
            Task = task;
        }

        public Models.Task Task { get; }
    }
}
