using System;
using System.Collections.Generic;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Commands
{
    public class SaveTask : BaseCommand<Task>
    {
        internal readonly IEnumerable<Comment> Comments;

        public SaveTask(
            string sourceId,
            Guid? taskId,
            string data,
            string callback,
            string taskType,
            string status,
            Assignment assignment,
            Guid fourEyeSubjectId,
            Guid initiatedBy,
            IEnumerable<Relation> relations,
            string sourceName,
            string subject,
            string comment) : base(initiatedBy)
        {
            SourceId = sourceId;
            TaskId = taskId == Guid.Empty ? Guid.NewGuid() : taskId;
            Data = data;
            Callback = callback;
            TaskType = taskType;
            Status = status;
            Assignment = assignment;
            FourEyeSubjectId = fourEyeSubjectId;
            Relations = relations;
            SourceName = sourceName;
            Subject = subject;
            Comment = comment;
        }

        public string SourceId { get; }
        public Guid? TaskId { get; }
        public string Data { get; }
        public string Callback { get; }
        public string TaskType { get; }
        public string Status { get; }
        public Assignment Assignment { get; }
        public Guid FourEyeSubjectId { get; }
        public IEnumerable<Relation> Relations { get; }
        public string SourceName { get; }
        public string Subject { get; }
        public string Comment { get; }
    }
}
