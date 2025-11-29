using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class CreateReportHandler : ICommandHandler<CreateReport>
    {
        private readonly IReportingService _reportingService;
        private readonly CreateReportValidator _validator;

        public CreateReportHandler(IReportingService reportingService, CreateReportValidator validator)
        {
            _reportingService = reportingService;
            _validator = validator;
        }

        public async Task<Unit> Handle(CreateReport request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            var files = await _reportingService.GetReportingDataAsync(request.DboEntities, request.FromDatetime, request.ToDatetime, cancellationToken);
            await _reportingService.StoreReportAsync(request.CorrelationId, files, cancellationToken);
            return Unit.Value;
        }
    }
}
