using System;
using System.Collections.Generic;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.IntegrationEvents
{
    [Obsolete("This is needed for consistent data model for old events.")]
    public class Task
    {
        public Task(
            Guid taskId,
            string taskType,
            Callback callback,
            Guid fourEyeSubjectId,
            string subject,
            Source source,
            IList<Comment> comments,
            string status,
            string change,
            bool isFinal,
            string data,
            Assignment assignment,
            IList<Relation> relations,
            Guid createdBy,
            Guid? changedBy,
            DateTime createdDate,
            DateTime? changedDate
            )
        {
            TaskId = taskId;
            TaskType = taskType;
            Callback = callback;
            FourEyeSubjectId = fourEyeSubjectId;
            Subject = subject;
            Source = source;
            Comments = comments;
            Status = status;
            Change = change;
            IsFinal = isFinal;
            Data = data;
            Assignment = assignment;
            Relations = relations;
            CreatedBy = createdBy;
            ChangedBy = changedBy;
            CreatedDate = createdDate;
            ChangedDate = changedDate;
        }

        public Guid TaskId { get; }
        public string TaskType { get; }
        public Callback Callback { get; }
        public Guid FourEyeSubjectId { get; }
        public string Subject { get; }
        public Source Source { get; }
        public IList<Comment> Comments { get; }
        public string Status { get; }
        public string Change { get; }
        public bool IsFinal { get; }
        public string Data { get; }
        public Assignment Assignment { get; }
        public IList<Relation> Relations { get; }
        public Guid CreatedBy { get; }
        public Guid? ChangedBy { get; }
        public DateTime CreatedDate { get; }
        public DateTime? ChangedDate { get; }
    }

    [Obsolete("This is needed for consistent data model for old events.")]
    public class Relation
    {
        public Relation(
            Guid relationId,
            Guid taskId,
            Guid entityId,
            string entityType
            )
        {
            RelationId = relationId;
            TaskId = taskId;
            EntityId = entityId;
            EntityType = entityType;
        }

        public Guid RelationId { get; }
        public Guid TaskId { get; }
        public Guid EntityId { get; }
        public string EntityType { get; }
    }
}
