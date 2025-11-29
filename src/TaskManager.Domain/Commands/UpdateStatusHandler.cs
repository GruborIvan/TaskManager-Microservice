using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Handlers
{
    public class UpdateStatusHandler : ICommandHandler<UpdateStatus, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly UpdateStatusValidator _validator;

        public UpdateStatusHandler(
            ITaskRepository repository,
            UpdateStatusValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(UpdateStatus request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot update task status");

            task.UpdateStatus(request.Status, request.InitiatedBy);      
            
            var updatedTask = _repository.UpdateTaskStatus(task);
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
