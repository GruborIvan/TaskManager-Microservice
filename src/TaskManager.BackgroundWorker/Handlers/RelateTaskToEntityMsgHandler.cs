using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Retry.Simple;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class RelateTaskToEntityMsgHandler : IHandleMessages<RelateTaskToEntityMsg>, IHandleMessages<RelateTaskToEntityMsgV2>, IHandleMessages<RelateTaskToEntityMsgV3>,
        IHandleMessages<IFailed<RelateTaskToEntityMsg>>, IHandleMessages<IFailed<RelateTaskToEntityMsgV2>>, IHandleMessages<IFailed<RelateTaskToEntityMsgV3>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public RelateTaskToEntityMsgHandler(
            IMediator mediator,
            ILogger<RelateTaskToEntityMsgHandler> logger,
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

        public async Task Handle(RelateTaskToEntityMsg message)
        {
            _logger.LogInformation($"Received {nameof(RelateTaskToEntityMsg)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<RelateTaskToEntity>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<RelateTaskToEntityMsg> message)
        {
            _logger.LogError($"{nameof(RelateTaskToEntityMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new RelateTaskToEntityFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(RelateTaskToEntityMsgV2 message)
        {
            _logger.LogInformation($"Received {nameof(RelateTaskToEntityMsgV2)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<RelateTaskToEntity>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<RelateTaskToEntityMsgV2> message)
        {
            _logger.LogError($"{nameof(RelateTaskToEntityMsgV2)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new RelateTaskToEntityFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(RelateTaskToEntityMsgV3 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(RelateTaskToEntityMsgV3)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<RelateTaskToEntity>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<RelateTaskToEntityMsgV3> message)
        {
            _logger.LogError(@$"{nameof(RelateTaskToEntityMsgV3)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new RelateTaskToEntityFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
