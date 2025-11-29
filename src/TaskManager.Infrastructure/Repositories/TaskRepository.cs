using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Models;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TasksDbContext _context;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TaskRepository(
            TasksDbContext context, 
            IMediator mediator,
            IMapper mapper)
        {
            _context = context;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<Domain.Models.Task> GetAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var taskDbo = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Comments)
                .Include(t => t.TaskRelations)
                .SingleOrDefaultAsync(t => t.TaskId.Equals(taskId), cancellationToken)
                    ?? throw new TaskNotFoundException(taskId);

            return _mapper.Map<Domain.Models.Task>(taskDbo);
        }

        public async Task<IEnumerable<Domain.Models.Task>> GetTaskHistoryAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var historicTasks = await _context.Tasks
                .FromSqlRaw("SELECT * FROM dbo.Tasks FOR SYSTEM_TIME ALL")
                .OrderByDescending(t => t.ChangedDate)
                .Where(t => t.TaskId.Equals(taskId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

           return historicTasks.Select(taskDbo => _mapper.Map<Domain.Models.Task>(taskDbo));
        }

        public async Task<Domain.Models.Task> AddAsync(Domain.Models.Task task, CancellationToken cancellationToken = default)
        {
            var taskDbo = (await _context.Tasks.AddAsync(
                _mapper.Map<TaskDbo>(task),
                cancellationToken
                )).Entity;

            return _mapper.Map<Domain.Models.Task>(taskDbo);
        }

        public Domain.Models.Task Update(Domain.Models.Task task)
        {
            var taskDbo = _context.Tasks
                .Update(_mapper.Map<TaskDbo>(task)).Entity;

            return _mapper.Map<Domain.Models.Task>(taskDbo);
        }

        public Domain.Models.Task UpdateAssignment(Domain.Models.Task task)
        {
            var taskDbo = _mapper.Map<TaskDbo>(task);

            _context.Attach(taskDbo);
            _context.Entry(taskDbo).Property("AssignedToEntityId").IsModified = true;
            _context.Entry(taskDbo).Property("AssignmentType").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedBy").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedDate").IsModified = true;

            return task;
        }

        public Domain.Models.Task UpdateTaskData(Domain.Models.Task task)
        {
            var taskDbo = _mapper.Map<TaskDbo>(task);

            _context.Attach(taskDbo);
            _context.Entry(taskDbo).Property("Data").IsModified = true;
            _context.Entry(taskDbo).Property("FinalState").IsModified = true;
            _context.Entry(taskDbo).Property("Status").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedBy").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedDate").IsModified = true;

            return task;
        }

        public Domain.Models.Task UpdateTaskStatus(Domain.Models.Task task)
        {
            var taskDbo = _mapper.Map<TaskDbo>(task);

            _context.Attach(taskDbo);
            _context.Entry(taskDbo).Property("Status").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedBy").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedDate").IsModified = true;

            return task;
        }

        public Domain.Models.Task FinalizeTask(Domain.Models.Task task)
        {
            var taskDbo = _mapper.Map<TaskDbo>(task);
            _context.Attach(taskDbo);
            _context.Entry(taskDbo).Property("FinalState").IsModified = true;
            _context.Entry(taskDbo).Property("Status").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedBy").IsModified = true;
            _context.Entry(taskDbo).Property("ChangedDate").IsModified = true;

            return task;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.DispatchDomainEventsAsync(_context);
        }
    }
}
