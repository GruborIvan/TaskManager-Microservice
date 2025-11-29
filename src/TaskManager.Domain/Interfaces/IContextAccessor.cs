using System;

namespace TaskManager.Domain.Interfaces
{
    public interface IContextAccessor
    {
        public Guid GetCorrelationId();
        public Guid GetCommandId();
        public Guid GetRequestId();
        public void CheckIfCommandIdAndRequestIdExists();
    }
}
