using System.Threading.Tasks;

namespace TaskManager.Domain.Interfaces
{
    public interface IEventNotificationService
    {
        Task SendAsync(object @event, string subject);
    }
}
