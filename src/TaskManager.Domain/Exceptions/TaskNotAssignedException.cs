using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class TaskNotAssignedException : Exception
    {
        public TaskNotAssignedException(Guid taskId)
            : base($"{nameof(Task)} with {nameof(Task.TaskId)}: {taskId} not assigned.") { }
    }
}
