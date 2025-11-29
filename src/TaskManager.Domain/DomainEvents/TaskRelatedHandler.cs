using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskRelatedHandler : INotificationHandler<TaskRelated>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public TaskRelatedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(TaskRelated notification, CancellationToken cancellationToken)
        {
            var @event = new RelateTaskToEntitySucceededEvent(notification.Relation);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.Relation.TaskId}");
            await _eventStreamingService.SendAsync(@event, cancellationToken);
        }
    }
}
