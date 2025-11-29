using FluentValidation;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Validators
{
    public class AssignmentValidator : Validator<Assignment>
    {
        public AssignmentValidator()
        {
            RuleFor(x => x.AssignedToEntityId.GetValueOrDefault()).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
        }
    }
}
