using FluentValidation;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Validators
{
    public class RelationValidator : Validator<Relation>
    {
        public RelationValidator()
        {
            RuleFor(x => x.EntityId).NotEmpty();
            RuleFor(x => x.EntityType).NotEmpty();
        }
    }
}
