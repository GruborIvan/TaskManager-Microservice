namespace TaskManager.Domain.IntegrationEvents
{
    public class RelateTaskToEntitySucceededEvent : IntegrationEvent
    {
        public RelateTaskToEntitySucceededEvent(Models.Relation relation) : base()
        {
            Relation = relation;
        }

        public Models.Relation Relation { get; }
    }
}
