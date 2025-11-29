using System;
using System.Threading;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Interfaces
{
    public interface IRelationRepository
    {
        public System.Threading.Tasks.Task<Relation> AddAsync(Relation relation, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task<Relation> GetAsync(Guid relationId, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
