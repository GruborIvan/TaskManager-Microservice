using System.Threading;
using MediatR;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskUpdatedHandler : INotificationHandler<TaskUpdated>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public TaskUpdatedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(TaskUpdated notification, CancellationToken cancellationToken)
        {
            var @event = new UpdateTaskSucceededEvent(notification.Task);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.Task.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}