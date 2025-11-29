using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Handlers
{
    public class UpdateDataHandler : ICommandHandler<UpdateData, Models.Task>
    {
        private readonly ITaskRepository _repository;
        private readonly UpdateDataValidator _validator;

        public UpdateDataHandler(
            ITaskRepository repository,
            UpdateDataValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Models.Task> Handle(UpdateData request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _repository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot update data");

            task.UpdateData(request.Data, request.InitiatedBy);      
            
            var updatedTask = _repository.Update(task);            
            await _repository.SaveAsync(cancellationToken);

            return updatedTask;
        }
    }
}
