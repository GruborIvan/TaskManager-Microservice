using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class UpdateDataValidator : Validator<UpdateData>
    {
        public UpdateDataValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Data).ValidJson();
        }
    }
}
