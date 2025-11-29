using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class UpdateTaskV2Handler : ICommandHandler<UpdateTaskV2, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly UpdateTaskV2Validator _validator;

        public UpdateTaskV2Handler(
            ITaskRepository repository,
            UpdateTaskV2Validator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(UpdateTaskV2 request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot update task status");

            task.UpdateTask(request.Data, request.Subject, request.InitiatedBy);

            var updatedTask = _repository.Update(task);
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
