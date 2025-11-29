using Newtonsoft.Json;
using System;

namespace TaskManager.Domain.IntegrationEvents
{
    [JsonObject(Title = "CreateTaskSucceededEvent")]
    public class CreateTaskSucceededNotificationEvent : IntegrationEvent
    {
        public CreateTaskSucceededNotificationEvent(Guid taskId, string status) : base()
        {
            TaskId = taskId;
            Status = status;
        }

        public Guid TaskId { get; }
        public string Status { get; }
    }
}
