using System;
using System.Threading;
using TaskManager.Domain.Models;

namespace TaskManager.Domain.Interfaces
{
    public interface ICommentRepository
    {
        public System.Threading.Tasks.Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task<Comment> GetAsync(Guid commentId, CancellationToken cancellationToken = default);
        public System.Threading.Tasks.Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
