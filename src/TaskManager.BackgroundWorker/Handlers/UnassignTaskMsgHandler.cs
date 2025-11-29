using System.Threading.Tasks;
using MediatR;
using Rebus.Handlers;
using FiveDegrees.Messages.Task;
using TaskManager.Domain.Commands;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Linq;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Retry.Simple;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class UnassignTaskMsgHandler : IHandleMessages<UnassignTaskMsg>, IHandleMessages<UnassignTaskMsgV2>,
        IHandleMessages<IFailed<UnassignTaskMsg>>, IHandleMessages<IFailed<UnassignTaskMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public UnassignTaskMsgHandler(
            IMediator mediator,
            ILogger<UnassignTaskMsgHandler> logger,
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

        public async Task Handle(UnassignTaskMsg message)
        {
            _logger.LogInformation($"Received {nameof(UnassignTaskMsg)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<UnassignTask>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UnassignTaskMsg> message)
        {
            _logger.LogError($"{nameof(UnassignTaskMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new UnassignTaskFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(UnassignTaskMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(UnassignTaskMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<UnassignTask>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UnassignTaskMsgV2> message)
        {
            _logger.LogError(@$"{nameof(UnassignTaskMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new UnassignTaskFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
