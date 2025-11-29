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
    public class CommentRepository : ICommentRepository
    {
        private readonly TasksDbContext _context;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CommentRepository(
            TasksDbContext context, 
            IMediator mediator,
            IMapper mapper)
        {
            _context = context;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<Comment> GetAsync(Guid commentId, CancellationToken cancellationToken = default)
        {
            var commentDbo = await _context.Comments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    comment => comment.CommentId.Equals(commentId),
                    cancellationToken
                    ) ?? throw new CommentNotFoundException(commentId);

            return _mapper.Map<Comment>(commentDbo);
        }

        public async Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            var commentDbo = (await _context.Comments.AddAsync(
                _mapper.Map<CommentDbo>(comment),
                cancellationToken
                )).Entity;

            return _mapper.Map<Comment>(commentDbo);
        }

        public async System.Threading.Tasks.Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.DispatchDomainEventsAsync(_context);
        }
    }
}
