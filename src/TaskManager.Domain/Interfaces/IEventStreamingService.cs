using System.Threading;
using System.Threading.Tasks;

namespace TaskManager.Domain.Interfaces
{
    public interface IEventStreamingService
    {
        Task SendAsync<T>(T @event, CancellationToken ct = default);
    }
}
