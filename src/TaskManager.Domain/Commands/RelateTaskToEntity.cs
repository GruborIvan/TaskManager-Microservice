using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class RelateTaskToEntity : BaseCommand<Relation>
    {
        public RelateTaskToEntity(
            string entityId, 
            string entityType, 
            Guid taskId, 
            Guid initiatedBy) : base(initiatedBy)
        {
            EntityId = entityId;
            EntityType = entityType;
            TaskId = taskId;
        }

        public string EntityId { get; }
        public string EntityType { get; }
        public Guid TaskId { get; }
    }
}
