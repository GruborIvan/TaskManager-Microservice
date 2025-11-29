using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Models;

namespace TaskManager.Infrastructure.Repositories
{
    public class RelationRepository : IRelationRepository
    {
        private readonly TasksDbContext _context;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RelationRepository(
            TasksDbContext context,
            IMediator mediator,
            IMapper mapper)
        {
            _context = context;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<Relation> GetAsync(Guid relationId, CancellationToken cancellationToken = default)
        {
            var relationDbo = await _context.TaskRelations
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    relation => relation.RelationId.Equals(relationId),
                    cancellationToken
                    ) ?? throw new TaskRelationNotFoundException(relationId);

            return _mapper.Map<Relation>(relationDbo);
        }

        public async Task<Relation> AddAsync(Relation relation, CancellationToken cancellationToken = default)
        {
            var relationDbo = (await _context.TaskRelations.AddAsync(
                _mapper.Map<TaskRelationDbo>(relation),
                cancellationToken
                )).Entity;

            return _mapper.Map<Relation>(relationDbo);
        }

        public async System.Threading.Tasks.Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.DispatchDomainEventsAsync(_context);
        }
    }
}
