using System;
using System.Linq;

namespace TaskManager.Domain.IntegrationEvents
{
    [Obsolete("This event is using outdated data model. Please use UpdateTaskStatusSucceededEventV2.")]
    public class UpdateTaskStatusSucceededEvent : IntegrationEvent
    {
        public UpdateTaskStatusSucceededEvent(Models.Task task) : base()
        {
            Task = new Task(
                    task.TaskId,
                    task.TaskType,
                    task.Callback,
                    task.FourEyeSubjectId,
                    task.Subject,
                    task.Source,
                    task.Comments,
                    task.Status,
                    task.Change,
                    task.IsFinal,
                    task.Data,
                    task.Assignment,
                    task.Relations.Select(item =>
                        new Relation(
                            item.RelationId,
                            item.TaskId,
                            Guid.Parse(item.EntityId),
                            item.EntityType)).ToList(),
                    task.CreatedBy,
                    task.ChangedBy,
                    task.CreatedDate,
                    task.ChangedDate
                );
        }

        public Task Task { get; }
    }
}
