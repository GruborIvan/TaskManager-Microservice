using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class UpdateTaskV2Validator : Validator<UpdateTaskV2>
    {
        public UpdateTaskV2Validator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Data).ValidJson();
        }
    }
}
