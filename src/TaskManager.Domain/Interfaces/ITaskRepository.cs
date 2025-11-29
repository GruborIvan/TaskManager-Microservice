using System;
using System.Collections.Generic;
using System.Threading;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Interfaces
{
    public interface ITaskRepository
    {
        public System.Threading.Tasks.Task<Task> AddAsync(Task task, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task<Task> GetAsync(Guid taskId, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task<IEnumerable<Task>> GetTaskHistoryAsync(Guid taskId, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task SaveAsync(CancellationToken cancellationToken = default);
        public Task Update(Task task);
        public Task UpdateAssignment(Task task);
        public Task UpdateTaskData(Task task);
        public Task UpdateTaskStatus(Task task);
        public Task FinalizeTask(Task task);
    }
}
