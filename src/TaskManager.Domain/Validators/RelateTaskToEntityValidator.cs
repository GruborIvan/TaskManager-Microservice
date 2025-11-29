using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class RelateTaskToEntityValidator : Validator<RelateTaskToEntity>
    {
        public RelateTaskToEntityValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.EntityId).NotEmpty();
            RuleFor(x => x.EntityType).NotEmpty();
        }
    }
}
