using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class UnassignTaskValidator : Validator<UnassignTask>
    {
        public UnassignTaskValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
        }
    }
}
