using System;
using FluentValidation;
using TaskManager.Domain.Commands;

namespace TaskManager.Domain.Validators
{
    public class CreateReportValidator : Validator<CreateReport>
    {
        public CreateReportValidator()
        {
            RuleFor(x => x.CommandId).NotEmpty();
            RuleFor(x => x.CorrelationId).NotEmpty();
            RuleFor(x => x.DboEntities).NotEmpty();
            RuleFor(x => x)
                .Must(createReport =>
                {
                    bool bothDatesEmpty =
                        !createReport.FromDatetime.HasValue && !createReport.ToDatetime.HasValue;
                    bool fromInPastToEmpty =
                        (createReport.FromDatetime.HasValue && createReport.FromDatetime.Value < DateTime.UtcNow) &&
                        !createReport.ToDatetime.HasValue;
                    bool fromInPastToLessThanFrom =
                        (createReport.FromDatetime.HasValue && createReport.FromDatetime.Value < DateTime.UtcNow) &&
                        (createReport.ToDatetime.HasValue &&
                         createReport.ToDatetime.Value > createReport.FromDatetime.Value);

                    return bothDatesEmpty || fromInPastToEmpty || fromInPastToLessThanFrom;
                })
                .WithMessage("Invalid Datetime Range");
        }
    }
}
