using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class TaskRelationNotFoundException : Exception
    {
        public TaskRelationNotFoundException(Guid relationId)
            : base($"{nameof(Relation)} with {nameof(Relation.RelationId)}: {relationId} not found.") { }
    }
}
