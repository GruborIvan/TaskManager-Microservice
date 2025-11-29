using System.Threading.Tasks;
using MediatR;
using Rebus.Handlers;
using FiveDegrees.Messages.Task;
using TaskManager.Domain.Commands;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System;
using System.Linq;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Retry.Simple;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class CreateTaskMsgHandler : IHandleMessages<CreateTaskMsg>, IHandleMessages<CreateTaskMsgV2>, IHandleMessages<CreateTaskMsgV3>,
        IHandleMessages<IFailed<CreateTaskMsg>>, IHandleMessages<IFailed<CreateTaskMsgV2>>, IHandleMessages<IFailed<CreateTaskMsgV3>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public CreateTaskMsgHandler(
            IMediator mediator, 
            ILogger<CreateTaskMsgHandler> logger,
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

        public async Task Handle(CreateTaskMsg message)
        {
            _logger.LogInformation($"Received {nameof(CreateTaskMsg)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<SaveTask>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<CreateTaskMsg> message)
        {
            _logger.LogError($"{nameof(CreateTaskMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new CreateTaskFailed(message.Message.TaskId ?? Guid.Empty, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(CreateTaskMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(CreateTaskMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<SaveTask>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<CreateTaskMsgV2> message)
        {
            _logger.LogError(@$"{nameof(CreateTaskMsgV2)} failed with 
                    requestId: { _contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new CreateTaskFailed(message.Message.TaskId ?? Guid.Empty, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(CreateTaskMsgV3 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(CreateTaskMsgV3)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<SaveTask>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<CreateTaskMsgV3> message)
        {
            _logger.LogError(@$"{nameof(CreateTaskMsgV3)} failed with 
                    requestId: { _contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new CreateTaskFailed(message.Message.TaskId ?? Guid.Empty, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
