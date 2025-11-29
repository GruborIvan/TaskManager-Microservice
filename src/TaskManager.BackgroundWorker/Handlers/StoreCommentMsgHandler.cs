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
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.BackgroundWorker.Handlers
{
    public class StoreCommentMsgHandler : IHandleMessages<StoreCommentMsg>, IHandleMessages<StoreCommentMsgV2>,
        IHandleMessages<IFailed<StoreCommentMsg>>, IHandleMessages<IFailed<StoreCommentMsgV2>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBus _bus;

        public StoreCommentMsgHandler(
            IMediator mediator,
            ILogger<StoreCommentMsgHandler> logger,
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

        public async Task Handle(StoreCommentMsg message)
        {
            _logger.LogInformation($"Received {nameof(StoreCommentMsg)} with with correlationId: {message.CorrelationId}.");

            var command = _mapper.Map<Domain.Commands.StoreComment>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<StoreCommentMsg> message)
        {
            _logger.LogError($"{nameof(StoreCommentMsg)} failed with correlationId: {message.Message.CorrelationId} and error description {message.ErrorDescription}");

            await _mediator.Publish(new StoreCommentFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }

        public async Task Handle(StoreCommentMsgV2 message)
        {
            _contextAccessor.CheckIfCommandIdAndRequestIdExists();

            _logger.LogInformation($"Received {nameof(StoreCommentMsgV2)} with with requestId: {_contextAccessor.GetRequestId()}.");

            var command = _mapper.Map<Domain.Commands.StoreComment>(message);
            await _mediator.Send(command);
        }

        public async Task Handle(IFailed<StoreCommentMsgV2> message)
        {
            _logger.LogError(@$"{nameof(StoreCommentMsgV2)} failed with 
                    requestId: {_contextAccessor.GetRequestId()}, commandId: {_contextAccessor.GetCommandId()}
                    and error description {message.ErrorDescription}");

            await _mediator.Publish(new StoreCommentFailed(message.Message.TaskId, new ErrorData(message.Exceptions?.FirstOrDefault()?.Message, "")));

            await _bus.Advanced.TransportMessage.Deadletter(message.ErrorDescription);
        }
    }
}
