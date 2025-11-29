using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;

namespace TaskManager.Domain.Models
{
    public abstract class Entity
    {
        [JsonIgnore]
        public List<INotification> DomainEvents { get; set; }

        public void AddDomainEvent(INotification eventItem)
        {
            DomainEvents ??= new List<INotification>();
            DomainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            DomainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            DomainEvents?.Clear();
        }
    }
}
