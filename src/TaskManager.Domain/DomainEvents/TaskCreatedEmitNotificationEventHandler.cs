using MediatR;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskCreatedEmitNotificationEventHandler : INotificationHandler<TaskCreated>
    {
        private readonly IEventNotificationService _eventNotificationService;

        public TaskCreatedEmitNotificationEventHandler(
            IEventNotificationService eventNotificationService)
        {
            _eventNotificationService = eventNotificationService;
        }

        public async System.Threading.Tasks.Task Handle(TaskCreated notification, CancellationToken cancellationToken)
        {
            var @event = new CreateTaskSucceededNotificationEvent(notification.TaskId, notification.Status);

            await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.TaskId}");
        }
    }
}
