using System;

namespace TaskManager.Domain.Models
{
    public class Assignment
    {
        public Assignment(Guid? assignedToEntityId, string type, Guid taskId)
        {
            AssignedToEntityId = assignedToEntityId;
            Type = type;
            TaskId = taskId;
        }

        public Guid? AssignedToEntityId { get; }
        public string Type { get; }
        public Guid TaskId { get; }
    }
}
