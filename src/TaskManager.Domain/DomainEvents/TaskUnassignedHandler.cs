using MediatR;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskUnassignedHandler : INotificationHandler<TaskUnassigned>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public TaskUnassignedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(TaskUnassigned notification, CancellationToken cancellationToken)
        {
            var @event = new UnassignTaskSucceededEvent(notification.Assignment);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.Assignment.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}
