using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Retry.Simple;
using TaskManager.Domain.Commands;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class ReportingTaskHandler : IHandleMessages<ReportingTaskMsg>,
        IHandleMessages<IFailed<ReportingTaskMsg>>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IBus _bus;

        public ReportingTaskHandler(
            ILogger<ReportingTaskHandler> logger,
            IMediator mediator,
            IMapper mapper,
            IBus bus)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _bus = bus;
        }

        public async Task Handle(ReportingTaskMsg message)
        {
            _logger.LogInformation($"Received {nameof(ReportingTaskMsg)} with CorrelationId: {message.CorrelationId}");
            var command = _mapper.Map<CreateReport>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<ReportingTaskMsg> message)
        {
            _logger.LogError($"{nameof(ReportingTaskMsg)} failed with CorrelationId: {message.Message.CorrelationId} and error description: {message.ErrorDescription}.");
            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
