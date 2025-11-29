using MediatR;
using System;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskCreated : INotification
    {
        public TaskCreated(Task task)
        {
            TaskId = task.TaskId;
            Status = task.Status;
            Task = task;
        }

        public Guid TaskId { get; }
        public string Status { get; }
        public Task Task { get; }
    }
}
