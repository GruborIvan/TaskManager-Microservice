using TaskManager.Domain.Interfaces;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManager.Infrastructure.Services
{
    public class EventGridService : IEventNotificationService
    {
        private const string _version = "0.1";
        private const string _topic = "tasks";
        private const string _correlationIdKey = "CorrelationId";
        private const string _requestIdKey = "requestId";
        private const string _commandIdKey = "commandId";

        private readonly IEventGridClient _eventGridClient;
        private readonly IContextAccessor _contextAccessor;
        private readonly string _topicHostname;

        public EventGridService(IEventGridClient client, IContextAccessor contextAccessor, string topicEndpoint)
        {
            _topicHostname = new Uri(topicEndpoint).Host;
            _eventGridClient = client;
            _contextAccessor = contextAccessor;
        }

        public async Task SendAsync(object @event, string subject)
        {
            Validate(@event, subject);

            var messages = CreateEventsList(@event, subject, _topic, _version);
            await _eventGridClient.PublishEventsAsync(_topicHostname, messages);
        }

        private void Validate(object message, string subject)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
        }

        private IList<EventGridEvent> CreateEventsList(object message, string subject, string topic, string version)
        {
            return new List<EventGridEvent>()
            {
                new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = message.GetType().Name,
                    Data = CreateData(message),
                    EventTime = DateTime.UtcNow,
                    Subject = subject,
                    DataVersion = version,
                    Topic = topic
                }
            };
        }

        private JObject CreateData(object message)
        {
            var data = JObject.FromObject(message);
            data[_correlationIdKey] = _contextAccessor.GetCorrelationId().ToString();
            data[_requestIdKey] = _contextAccessor.GetRequestId().ToString();
            data[_commandIdKey] = _contextAccessor.GetCommandId().ToString();

            return data;
        }
    }
}
