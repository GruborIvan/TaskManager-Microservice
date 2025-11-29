using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class CannotModifyFinalizedTaskException : Exception
    {
        public CannotModifyFinalizedTaskException(Guid taskId, string message)
            : base($"{nameof(Task)} with {nameof(Task.TaskId)}: {taskId} is finalized and cannot be modified. {message}") { }
    }
}
