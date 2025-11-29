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
    public class UpdateTaskStatusMsgHandler : IHandleMessages<UpdateTaskStatusMsg>, IHandleMessages<UpdateTaskStatusMsgV2>,
        IHandleMessages<IFailed<UpdateTaskStatusMsg>>, IHandleMessages<IFailed<UpdateTaskStatusMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public UpdateTaskStatusMsgHandler(
            IMediator mediator,
            ILogger<UpdateTaskStatusMsgHandler> logger,
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

        public async Task Handle(UpdateTaskStatusMsg message)
        {
            _logger.LogInformation($"Received {nameof(UpdateTaskStatusMsg)} with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<UpdateStatus>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UpdateTaskStatusMsg> message)
        {
            _logger.LogError($"{nameof(UpdateTaskStatusMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new UpdateStatusFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(UpdateTaskStatusMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(UpdateTaskStatusMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<UpdateStatus>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UpdateTaskStatusMsgV2> message)
        {
            _logger.LogError(@$"{nameof(UpdateTaskStatusMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new UpdateStatusFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
