using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class SaveTaskValidator : Validator<SaveTask>
    {
        public SaveTaskValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.SourceId).NotEmpty();
            RuleFor(x => x.Data).ValidJson();
            RuleFor(x => x.Callback).HttpOrHttpsUrl().When(x => !string.IsNullOrEmpty(x.Callback));
            RuleFor(x => x.TaskType).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.Assignment).NotNull();
            RuleFor(x => x.Relations).NotNull().ForEach(x => x.SetValidator(new RelationValidator()));
        }
    }
}
