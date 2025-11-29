using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class AssignTaskToEntityHandler : ICommandHandler<AssignTaskToEntity, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly AssignTaskToEntityValidator _validator;

        public AssignTaskToEntityHandler(
            ITaskRepository repository,
            AssignTaskToEntityValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(AssignTaskToEntity request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot assign task to entity");

            task.Assign(request.Assignment, request.InitiatedBy);

            var updatedTask = _repository.UpdateAssignment(task);
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
