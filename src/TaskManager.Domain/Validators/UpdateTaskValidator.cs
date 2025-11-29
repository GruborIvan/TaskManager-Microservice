using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class UpdateTaskValidator : Validator<UpdateTask>
    {
        public UpdateTaskValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Data).ValidJson();
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}
