using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class UpdateStatusValidator : Validator<UpdateStatus>
    {
        public UpdateStatusValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}
