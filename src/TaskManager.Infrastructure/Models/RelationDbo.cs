using System;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure.Models
{
    public class TaskRelationDbo : Entity
    {
        public Guid RelationId { get; set; }
        public Guid TaskId { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public bool IsMain { get; set; }

        public TaskDbo Task { get; set; }
    }
}
