using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class AssignTaskToEntityValidator : Validator<AssignTaskToEntity>
    {
        public AssignTaskToEntityValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Assignment).NotNull().SetValidator(new AssignmentValidator());
        }
    }
}
