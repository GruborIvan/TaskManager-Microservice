using MediatR;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class CommentAddedHandler : INotificationHandler<CommentAdded>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public CommentAddedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(CommentAdded notification, CancellationToken cancellationToken)
        {
            var @event = new StoreCommentSucceededEvent(notification.Comment);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.Comment.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}
