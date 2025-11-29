using System.Threading.Tasks;
using MediatR;
using Rebus.Handlers;
using FiveDegrees.Messages.Task;
using TaskManager.Domain.Commands;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Linq;
using Rebus.Bus;
using Rebus.Retry.Simple;
using Rebus.Exceptions;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class AssignTaskToEntityMsgHandler : IHandleMessages<AssignTaskToEntityMsg>, IHandleMessages<AssignTaskToEntityMsgV2>,
        IHandleMessages<IFailed<AssignTaskToEntityMsg>>, IHandleMessages<IFailed<AssignTaskToEntityMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public AssignTaskToEntityMsgHandler(
            IMediator mediator,
            ILogger<AssignTaskToEntityMsgHandler> logger,
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

        public async Task Handle(AssignTaskToEntityMsg message)
        {
            _logger.LogInformation($"Received {nameof(AssignTaskToEntityMsg)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<AssignTaskToEntity>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<AssignTaskToEntityMsg> message)
        {
            _logger.LogError($"{nameof(AssignTaskToEntityMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new AssignTaskToEntityFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(AssignTaskToEntityMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(AssignTaskToEntityMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<AssignTaskToEntity>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<AssignTaskToEntityMsgV2> message)
        {
            _logger.LogError(@$"{nameof(AssignTaskToEntityMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new AssignTaskToEntityFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
