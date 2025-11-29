using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Handlers
{
    public class UpdateTaskHandler : ICommandHandler<UpdateTask, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly ICallbackService _service;
        private readonly UpdateTaskValidator _validator;

        public UpdateTaskHandler(
            ITaskRepository repository, 
            ICallbackService service,
            UpdateTaskValidator validator)
        {
            _repository = repository;
            _service = service;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(UpdateTask request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot update task");

            task.UpdateData(request.Data, request.InitiatedBy);

            if (request.FinalState) 
                task.FinalizeTask(request.Status, request.InitiatedBy);
            else 
                task.UpdateStatus(request.Status, request.InitiatedBy);

            var updatedTask = _repository.UpdateTaskData(task);

            if (request.FinalState)
            {
                await _service.Callback(task.Callback, task);
            }

            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
