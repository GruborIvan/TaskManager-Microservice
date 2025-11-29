using FluentValidation;
using System;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class StoreCommentValidator : Validator<StoreComment>
    {
        private const string DateInFutureErrorMessage = "Comment CreatedDate property must be set in the past";

        public StoreCommentValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.Text).NotEmpty();
            RuleFor(x => x.CreatedDate.Date).LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(DateInFutureErrorMessage);
        }
    }
}
