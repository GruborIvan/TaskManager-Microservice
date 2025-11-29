using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class UnassignTaskHandler : ICommandHandler<UnassignTask, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly UnassignTaskValidator _validator;

        public UnassignTaskHandler(
            ITaskRepository repository,
            UnassignTaskValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(UnassignTask request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot unassign task");

            task.Unassign(request.InitiatedBy);

            var updatedTask = _repository.Update(task);
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
