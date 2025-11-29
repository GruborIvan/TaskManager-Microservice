using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Handlers
{
    public class FinalizeStatusHandler : ICommandHandler<FinalizeStatus, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly ICallbackService _callbackService;
        private readonly FinalizeStatusValidator _validator;

        public FinalizeStatusHandler(
            ITaskRepository repository, 
            ICallbackService callbackService,
            FinalizeStatusValidator validator)
        {
            _repository = repository;
            _callbackService = callbackService;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(FinalizeStatus request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Task is already finalized");

            task.FinalizeTask(request.Status, request.InitiatedBy);

            var updatedTask = _repository.FinalizeTask(task);
            if (task.Callback?.Parameters != null)
            {
                await _callbackService.Callback(task.Callback, task);
            }
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
