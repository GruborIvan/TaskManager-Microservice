using System;
using TaskManager.Domain.DomainEvents;

namespace TaskManager.Domain.Models
{
    public class Relation : Entity
    {
        public Guid RelationId { get; }
        public Guid TaskId { get; }
        public string EntityId { get; }
        public string EntityType { get; }
        public bool IsMain { get; }

        public Relation(
            Guid relationId,
            Guid taskId,
            string entityId,
            string entityType)
        {
            RelationId = relationId;
            TaskId = taskId;
            EntityId = entityId;
            EntityType = entityType;
        }

        public Relation(
            Guid relationId,
            Guid taskId,
            string entityId,
            string entityType,
            bool isMain)
        {
            RelationId = relationId;
            TaskId = taskId;
            EntityId = entityId;
            EntityType = entityType;
            IsMain = isMain;
        }

        public Relation(
            Guid taskId,
            string entityId, 
            string entityType) : this(Guid.NewGuid(), taskId, entityId, entityType)
        {
            AddDomainEvent(new TaskRelated(this));
        }

        public Relation(
            Guid taskId,
            string entityId,
            string entityType,
            bool isMain) : this(Guid.NewGuid(), taskId, entityId, entityType, isMain)
        {
            AddDomainEvent(new TaskRelated(this));
        }
    }
}
