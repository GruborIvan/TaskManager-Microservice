using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Exceptions
{
    public class FourEyeRequirementNotMetException : Exception
    {
        public FourEyeRequirementNotMetException(Guid taskId)
            : base($"{nameof(Task)} with {nameof(Task.TaskId)}: {taskId}, four eyes not met.") { }
    }
}
