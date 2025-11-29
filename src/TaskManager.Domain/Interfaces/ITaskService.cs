using TaskManager.Domain.Models;

namespace TaskManager.Domain.Interfaces
{
    public interface ICallbackService
    {
        public System.Threading.Tasks.Task Callback(Callback callback, Task task);
    }
}
