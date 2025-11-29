namespace TaskManager.Domain.IntegrationEvents
{
    public class FinalizeTaskStatusSucceededEventV2 : IntegrationEvent
    {
        public FinalizeTaskStatusSucceededEventV2(Models.Task task) : base()
        {
            Task = task;
        }

        public Models.Task Task { get; }
    }
}
