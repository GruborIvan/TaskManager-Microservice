using System;

namespace TaskManager.Domain.Models.Reporting
{
    public class TaskRelationReport
    {
        public Guid RelationId { get; set; }
        public Guid TaskId { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public bool IsMain { get; set; }
    }
}
