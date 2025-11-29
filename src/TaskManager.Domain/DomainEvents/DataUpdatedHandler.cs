using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class DataUpdatedHandler : INotificationHandler<DataUpdated>
    {
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IEventStreamingService _eventStreamingService;

        public DataUpdatedHandler(
            IEventNotificationService eventNotificationService,
            IEventStreamingService eventStreamingService)
        {
            _eventNotificationService = eventNotificationService;
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(DataUpdated notification, CancellationToken cancellationToken)
        {
            List<object> events = notification.Task.Relations.All(relation =>
                Guid.TryParse(relation.EntityId, out _))
                ? new List<object>()
                    {
                        new UpdateTaskDataSucceededEvent(notification.Task),
                        new UpdateTaskDataSucceededEventV2(notification.Task)
                    }
                : new List<object>()
                    {
                        new UpdateTaskDataSucceededEventV2(notification.Task)
                    };

            foreach(var @event in events)
            {
                await _eventNotificationService.SendAsync(@event, $"api/tasks/{notification.Task.TaskId}");
                await _eventStreamingService.SendAsync(@event, cancellationToken);
            }
        }
    }
}
