using System;

namespace TaskManager.Domain.Models.Reporting
{
    public class TaskReport
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
        public string Change { get; set; }
    }
}
