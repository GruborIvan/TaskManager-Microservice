using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Models
{
    public class Task : Entity
    {
        public Guid TaskId { get; }
        public string TaskType { get; }
        public Callback Callback { get; }
        public Guid FourEyeSubjectId { get; }
        public string Subject { get; private set; }
        public Source Source { get; }
        public IList<Comment> Comments { get; }
        public string Status { get; private set; }
        public string Change { get; private set; }
        public bool IsFinal { get; set; }
        public string Data { get; private set; }
        public Assignment Assignment { get; private set; }
        public IList<Relation> Relations { get; }
        public Guid CreatedBy { get; }
        public Guid? ChangedBy { get; private set; }
        public DateTime CreatedDate { get; }
        public DateTime? ChangedDate { get; private set; }

        public Task()
        {
            Comments = new List<Comment>();
            Relations = new List<Relation>();
        }

        public Task(
            Guid taskId,
            string taskType, 
            Callback callback, 
            Guid fourEyeSubjectId, 
            string subject, 
            Source source, 
            IEnumerable<Comment> comments,
            string status,
            string data,
            Assignment assignment,
            IEnumerable<Relation> relations,
            string change = "Initial",
            bool isFinal = false)
        {
            TaskId = taskId;
            TaskType = taskType;
            Callback = callback;
            FourEyeSubjectId = fourEyeSubjectId;
            Subject = subject;
            Source = source;
            Comments = comments?.ToList() ?? new List<Comment>();
            Status = status;
            Data = data;
            Assignment = assignment;
            Relations = relations?.ToList() ?? new List<Relation>();
            Change = change;
            IsFinal = isFinal;
        }

        public Task(
            Guid taskId,
            string taskType,
            Callback callback,
            Guid fourEyeSubjectId,
            string subject,
            Source source,
            IEnumerable<Comment> comments,
            string status,
            string data,
            Assignment assignment,
            IEnumerable<Relation> relations,
            Guid createdBy,
            DateTime createdDate,
            string change = "Initial",
            bool isFinal = false,
            Guid? changedBy = null,
            DateTime? changedDate = null) : this(taskId, taskType, callback, fourEyeSubjectId, subject, source, comments, status, data, assignment, relations, change, isFinal)
        {
            CreatedBy = createdBy;
            ChangedBy = changedBy;
            CreatedDate = createdDate;
            ChangedDate = changedDate;
        }

        public Task(
            string taskType,
            Callback callback,
            Guid fourEyeSubjectId,
            string subject,
            Source source,
            string status,
            string data,
            Guid createdBy,
            DateTime createdDate,
            string change = "Initial",
            bool isFinal = false,
            Guid? changedBy = null,
            DateTime? changedDate = null) : this(Guid.NewGuid(), taskType, callback, fourEyeSubjectId, subject, source, null, status, data, null, null, createdBy, createdDate, change, isFinal, changedBy, changedDate)
        {
            AddDomainEvent(new TaskCreated(this));
        }

        public Task(
            Guid taskId,
            string taskType,
            Callback callback,
            Guid fourEyeSubjectId,
            string subject,
            Source source,
            string status,
            string data,
            Guid createdBy,
            DateTime createdDate,
            string change = "Initial",
            bool isFinal = false,
            Guid? changedBy = null,
            DateTime? changedDate = null) : this(taskId, taskType, callback, fourEyeSubjectId, subject, source, null, status, data, null, null, createdBy, createdDate, change, isFinal, changedBy, changedDate)
        {
            AddDomainEvent(new TaskCreated(this));
        }

        public void UpdateStatus(string newStatus, Guid initiatedBy)
        {
            Status = newStatus;
            Change = "Status";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
            AddDomainEvent(new StatusUpdated(this));
        }

        public void FinalizeTask(string newStatus, Guid initiatedBy)
        {
            if (initiatedBy == FourEyeSubjectId)
                throw new FourEyeRequirementNotMetException(TaskId);

            UpdateStatus(newStatus, initiatedBy);
            Change = "Final";
            IsFinal = true;
            AddDomainEvent(new Finalized(this));
        }

        public void UpdateData(string newData, Guid initiatedBy)
        {
            Data = newData;
            Change = "Data";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
            AddDomainEvent(new DataUpdated(this));
        }

        public void Unassign(Guid initiatedBy)
        {
            if (Assignment.AssignedToEntityId == null)
                throw new TaskNotAssignedException(TaskId);

            Assignment = new Assignment(null, "Unassigned", TaskId);
            Change = "Assignment";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
            AddDomainEvent(new TaskUnassigned(Assignment));
        }

        public void Assign(Assignment assignment, Guid initiatedBy)
        {
            Assignment = assignment;
            Change = "Assignment";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
            AddDomainEvent(new TaskAssigned(Assignment));
        }

        public void Assign(Guid? assignedToEntityId, string type, Guid taskId, Guid initiatedBy)
        {
            Assignment = new Assignment(assignedToEntityId, type, taskId);
            Change = "Assignment";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
        }

        public void RelateTo(Relation relation)
        {
            Relations.Add(relation);
            AddDomainEvent(new TaskRelated(relation));
        }

        public void RelateTo(string entityId, string entityType, Guid taskId, bool isMain)
        {
            Relations.Add(new Relation(Guid.NewGuid(), taskId, entityId, entityType, isMain));
        }

        public void AddRelations(IEnumerable<Relation> relations)
        {
            foreach(var relation in relations)
            {
                RelateTo(relation.EntityId, relation.EntityType, relation.TaskId, relation.IsMain);
            }
        }

        public void AddComment(string text, Guid initiatedBy, bool sendEvent = true)
        {
            var comment =
                new Comment(
                    Guid.Empty,
                    TaskId,
                    text,
                    initiatedBy,
                    DateTime.UtcNow);
            Comments.Add(comment);
            if (sendEvent)
            {
                AddDomainEvent(new CommentAdded(comment));
            }
        }

        public void UpdateTask(string data, string subject, Guid initiatedBy)
        {
            Data = data;
            Subject = subject;
            Change = "Update";
            ChangedBy = initiatedBy;
            ChangedDate = DateTime.UtcNow;
            AddDomainEvent(new TaskUpdated(this));
        }
    }
}
