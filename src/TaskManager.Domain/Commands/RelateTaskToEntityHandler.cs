using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class RelateTaskToEntityHandler : ICommandHandler<RelateTaskToEntity, Relation>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IRelationRepository _relationRepository;
        private readonly RelateTaskToEntityValidator _validator;

        public RelateTaskToEntityHandler(
            ITaskRepository taskRepository,
            IRelationRepository relationRepository,
            RelateTaskToEntityValidator validator)
        {
            _taskRepository = taskRepository;
            _relationRepository = relationRepository;
            _validator = validator;
        }

        public async Task<Relation> Handle(RelateTaskToEntity request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var task = await _taskRepository.GetAsync(request.TaskId, cancellationToken);

            if (task.IsFinal)
                throw new CannotModifyFinalizedTaskException(task.TaskId, "Cannot relate task to entity");

            var addedRelation = await _relationRepository.AddAsync(CreateRelation(request), cancellationToken);
            await _relationRepository.SaveAsync(cancellationToken);

            return addedRelation;
        }

        private Relation CreateRelation(RelateTaskToEntity request)
            => new Relation(request.TaskId, request.EntityId, request.EntityType);
    }
}
