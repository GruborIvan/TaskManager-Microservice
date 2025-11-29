using System.Threading.Tasks;
using MediatR;
using Rebus.Handlers;
using TaskManager.Domain.Commands;
using FiveDegrees.Messages.Task;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Retry.Simple;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class FinalizeTaskStatusMsgHandler : IHandleMessages<FinalizeTaskStatusMsg>, IHandleMessages<FinalizeTaskStatusMsgV2>,
        IHandleMessages<IFailed<FinalizeTaskStatusMsg>>, IHandleMessages<IFailed<FinalizeTaskStatusMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public FinalizeTaskStatusMsgHandler(
            IMediator mediator,
            ILogger<FinalizeTaskStatusMsgHandler> logger,
            IMapper mapper,
            IContextAccessor contextAccessor,
            IBus bus)
        {
            _mediator = mediator;
            _logger = logger;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _bus = bus;
        }

        public async Task Handle(FinalizeTaskStatusMsg message)
        {
            _logger.LogInformation($"Received {nameof(FinalizeTaskStatusMsg)} with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<FinalizeStatus>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<FinalizeTaskStatusMsg> message)
        {
            _logger.LogError($"{nameof(FinalizeTaskStatusMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new FinalizeStatusFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(FinalizeTaskStatusMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(FinalizeTaskStatusMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<FinalizeStatus>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<FinalizeTaskStatusMsgV2> message)
        {
            _logger.LogError(@$"{nameof(FinalizeTaskStatusMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new FinalizeStatusFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
