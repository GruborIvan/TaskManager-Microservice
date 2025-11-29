using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Models.Reporting;

namespace TaskManager.Domain.Interfaces
{
    public interface IReportingRepository
    {
        Task<IEnumerable<TaskReport>> GetTasksAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default);
        Task<IEnumerable<TaskReport>> GetTaskHistoryAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default);
        Task<IEnumerable<TaskRelationReport>> GetTaskRelationsAsync(CancellationToken ct = default);
        Task<IEnumerable<CommentReport>> GetCommentsAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default);
    }
}
