using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Domain.DomainEvents
{
    public class TaskCreatedEmitStreamEventHandler : INotificationHandler<TaskCreated>
    {
        private readonly IEventStreamingService _eventStreamingService;

        public TaskCreatedEmitStreamEventHandler(IEventStreamingService eventStreamingService)
        {
            _eventStreamingService = eventStreamingService;
        }

        public async System.Threading.Tasks.Task Handle(TaskCreated notification, CancellationToken cancellationToken)
        {
            List<object> events = notification.Task.Relations.All(relation =>
                Guid.TryParse(relation.EntityId, out _))
                ? new List<object>()
                    {
                        new CreateTaskSucceededStreamEvent(notification.Task),
                        new CreateTaskSucceededStreamEventV2(notification.Task)
                    }
                : new List<object>()
                    {
                        new CreateTaskSucceededStreamEventV2(notification.Task)
                    };

            foreach (var @event in events)
            {
                await _eventStreamingService.SendAsync(@event, cancellationToken);
            }
        }
    }
}
