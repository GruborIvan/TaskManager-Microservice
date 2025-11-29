using FiveDegrees.Messages.Task;
using System;
using System.Collections.Generic;

namespace TaskManager.API.Models
{
    public class SendCreateTaskMessageRequest
    {
        public Guid CorrelationId { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string Subject { get; set; }
        public string Data { get; set; }
        public string Callback { get; set; }
        public TaskType TaskType { get; set; }
        public string Status { get; set; }
        public Guid? AssignedToEntityId { get; set; }
        public string AssignmentType { get; set; }
        public IEnumerable<RelationDto>? Relations { get; set; }
        public Guid FourEyeSubjectId { get; set; }
        public Guid RequestorId { get; set; }
        public Guid? TaskId { get; set; }
        public string Comment { get; set; }
    }
}
