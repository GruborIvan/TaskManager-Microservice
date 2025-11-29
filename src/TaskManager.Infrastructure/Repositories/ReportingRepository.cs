using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models.Reporting;
using TaskManager.Infrastructure.Models;

namespace TaskManager.Infrastructure.Repositories
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly TasksDbContext _dbContext;
        private readonly IMapper _mapper;

        public ReportingRepository(TasksDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskReport>> GetTasksAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default)
        {
            var tasks = await _dbContext
                .Tasks
                .AsNoTracking()
                .Where(c => (c.ChangedDate ?? c.CreatedDate) >= (fromDate ?? DateTime.MinValue) && (c.ChangedDate ?? c.CreatedDate) <= (toDatetime ?? DateTime.MaxValue))
                .ToListAsync(ct);

            return _mapper.Map<IEnumerable<TaskReport>>(tasks);
        }

        public async Task<IEnumerable<TaskReport>> GetTaskHistoryAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default)
        {
            var historicTasks = await _dbContext
                .Tasks
                .FromSqlRaw("SELECT * FROM dbo.Tasks FOR SYSTEM_TIME ALL")
                .OrderByDescending(t => t.ChangedDate)
                .Where(c => (c.ChangedDate ?? c.CreatedDate) >= (fromDate ?? DateTime.MinValue) && (c.ChangedDate ?? c.CreatedDate) <= (toDatetime ?? DateTime.MaxValue))
                .AsNoTracking()
                .ToListAsync(ct);

            return _mapper.Map<IEnumerable<TaskReport>>(historicTasks);
        }

        public async Task<IEnumerable<TaskRelationReport>> GetTaskRelationsAsync(CancellationToken ct = default)
        {
            var taskRelations = await _dbContext
                .TaskRelations
                .AsNoTracking()
                .ToListAsync(ct);

            return _mapper.Map<IEnumerable<TaskRelationReport>>(taskRelations);
        }

        public async Task<IEnumerable<CommentReport>> GetCommentsAsync(DateTime? fromDate, DateTime? toDatetime, CancellationToken ct = default)
        {
            var comments = await _dbContext
                .Comments
                .Where(c => c.CreatedDate >= (fromDate ?? DateTime.MinValue) && c.CreatedDate <= (toDatetime ?? DateTime.MaxValue))
                .AsNoTracking()
                .ToListAsync(ct);

            return _mapper.Map<IEnumerable<CommentReport>>(comments);
        }
    }
}
