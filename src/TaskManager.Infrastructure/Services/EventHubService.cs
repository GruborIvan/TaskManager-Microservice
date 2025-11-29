using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Services
{
    public class EventHubService : IEventStreamingService
    {
        private const string _eventTypeKey = "EventType";
        private const string _taskId = "TaskId";
        private const string _correlationIdKey = "CorrelationId";
        private const string _requestIdKey = "RequestId";
        private const string _commandIdKey = "CommandId";

        private readonly EventHubProducerClient _client;
        private readonly IContextAccessor _contextAccessor;

        public EventHubService(EventHubProducerClient client, IContextAccessor contextAccessor)
        {
            _client = client;
            _contextAccessor = contextAccessor;
        }

        public async Task SendAsync<T>(T @event, CancellationToken ct = default)
        {
            using var batch = await _client.CreateBatchAsync();
            var body = JsonConvert.SerializeObject(CreateEventBody(@event), Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(body)));
            await _client.SendAsync(batch, ct);
        }

        private JObject CreateEventBody<T>(T @event)
        {
            var body = JObject.FromObject(@event);

            var taskId = body
                .Descendants()
                .OfType<JProperty>()
                .FirstOrDefault(f => f.Name == _taskId)
                ?.Value;

            body[_taskId] = taskId;
            body[_correlationIdKey] = _contextAccessor.GetCorrelationId().ToString();
            body[_requestIdKey] = _contextAccessor.GetRequestId().ToString();
            body[_commandIdKey] = _contextAccessor.GetCommandId().ToString();
            body[_eventTypeKey] = @event.GetType().Name;

            return body;
        }
    }
}
