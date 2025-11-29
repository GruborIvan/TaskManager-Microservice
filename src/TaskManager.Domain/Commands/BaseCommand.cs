using System;

namespace TaskManager.Domain.Commands
{
    public abstract class BaseCommand<T> : ICommand<T>
    {
        protected BaseCommand(Guid initiatedBy)
        {
            CommandId = Guid.NewGuid();
            InitiatedBy = initiatedBy;
        }

        public Guid CommandId { get; }
        public Guid InitiatedBy { get; }
    }
}