using MediatR;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class RelateTaskToEntityFailedHandler : INotificationHandler<RelateTaskToEntityFailed>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public RelateTaskToEntityFailedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(RelateTaskToEntityFailed notification, CancellationToken cancellationToken)
        {
            var @event = new RelateTaskToEntityFailedEvent(notification.TaskId, notification.Error);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}
