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
    public class UpdateTaskDataMsgHandler : IHandleMessages<UpdateTaskDataMsg>, IHandleMessages<UpdateTaskDataMsgV2>,
        IHandleMessages<IFailed<UpdateTaskDataMsg>>, IHandleMessages<IFailed<UpdateTaskDataMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public UpdateTaskDataMsgHandler(
            IMediator mediator,
            ILogger<UpdateTaskDataMsgHandler> logger,
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

        public async Task Handle(UpdateTaskDataMsg message)
        {
            _logger.LogInformation($"Received {nameof(UpdateTaskDataMsg)} with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<UpdateData>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UpdateTaskDataMsg> message)
        {
            _logger.LogError($"{nameof(UpdateTaskDataMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new UpdateDataFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(UpdateTaskDataMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(UpdateTaskDataMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<UpdateData>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<UpdateTaskDataMsgV2> message)
        {
            _logger.LogError(@$"{nameof(UpdateTaskDataMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new UpdateDataFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
