using System;
using System.Linq;
using System.Threading;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class SaveTaskHandler : ICommandHandler<SaveTask, Task>
    {
        private readonly ITaskRepository _repository;
        private readonly SaveTaskValidator _validator;

        public SaveTaskHandler(
            ITaskRepository repository,
            SaveTaskValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async System.Threading.Tasks.Task<Task> Handle(SaveTask request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.AddAsync(CreateTask(request), cancellationToken);

            await _repository.SaveAsync(cancellationToken);

            return task;
        }

        private Task CreateTask(SaveTask command)
        {
            var task = new Task(
                command.TaskId ?? Guid.NewGuid(),
                command.TaskType,
                new HttpCallback(command.Callback != null ?
                    new Uri(command.Callback) : null),
                command.FourEyeSubjectId,
                command.Subject,
                new Source(
                    command.SourceId,
                    command.SourceName),
                command.Status,
                command.Data,
                command.InitiatedBy,
                DateTime.UtcNow
                );

            task.Assign(
                command.Assignment.AssignedToEntityId,
                command.Assignment.Type,
                task.TaskId,
                command.InitiatedBy
                );

            task.AddRelations(command.Relations.Select((relation, index) =>
                new Relation(
                    relation.RelationId, 
                    task.TaskId,
                    relation.EntityId,
                    relation.EntityType,
                    index == 0)
            ));

            if(!string.IsNullOrEmpty(command.Comment))
            {
                task.AddComment(
                    command.Comment,
                    command.InitiatedBy,
                    false
                    );
            }

            return task;
        }
    }
}