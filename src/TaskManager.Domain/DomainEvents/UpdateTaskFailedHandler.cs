using System.Threading;
using MediatR;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class UpdateTaskFailedHandler : INotificationHandler<UpdateTaskFailed>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public UpdateTaskFailedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(UpdateTaskFailed notification, CancellationToken cancellationToken)
        {
            var @event = new UpdateTaskFailedEvent(notification.TaskId, notification.Error);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}