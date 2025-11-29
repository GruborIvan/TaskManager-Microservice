using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(Guid taskId)
            : base($"{nameof(Task)} with {nameof(Task.TaskId)}: {taskId} not found.") { }
    }
}
