using System;
using System.Collections.Generic;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure.Models
{
    public class TaskDbo : Entity
    {
        public Guid TaskId { get; set; }
        public Guid CreatedById { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string TaskType { get; set; }
        public string Subject { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
        public string Callback { get; set; }
        public bool FinalState { get; set; }
        public DateTime? ChangedDate { get; set; }
        public Guid? AssignedToEntityId { get; set; }
        public string AssignmentType { get; set; }
        public Guid FourEyeSubjectId { get; set; }
        public string Change { get; set; } = "Initial";

        public List<TaskRelationDbo> TaskRelations { get; set; }
        public List<CommentDbo> Comments { get; set; }
    }
}
