using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.UnitTests.Infrastructure
{
    public class CommentRepositoryTests
    {
        private readonly DbContextOptions<TasksDbContext> _options =
            new DbContextOptionsBuilder<TasksDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private readonly Mock<IMediator> _mediatorMock = new Mock<IMediator>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly Comment _initialComment;

        private readonly TaskDbo _initialTaskDbo;
        private readonly CommentDbo _initialCommentDbo;
        private readonly TaskRelationDbo _initialRelationDbo;
        private readonly TasksDbContext context;

        public CommentRepositoryTests()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _initialCommentDbo = new CommentDbo
            {
                CommentId = Guid.NewGuid(),
                TaskId = taskId,
                Text = "test comment",
                CreatedById = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow
            };
            _initialRelationDbo = new TaskRelationDbo
            {
                EntityId = Guid.NewGuid().ToString(),
                TaskId = taskId,
                EntityType = "Person"
            };
            _initialTaskDbo = new TaskDbo
            {
                TaskId = taskId,
                Status = "status",
                Callback = "https://uri.com",
                Comments = new List<CommentDbo> { _initialCommentDbo },
                TaskRelations = new List<TaskRelationDbo> { _initialRelationDbo }
            };

            context = new TasksDbContext(_options);
            context.Tasks.Add(_initialTaskDbo);
            context.Comments.Add(_initialCommentDbo);

            context.SaveChanges();

            _initialComment = new Comment(
                _initialCommentDbo.CommentId, 
                _initialCommentDbo.TaskId, 
                _initialCommentDbo.Text, 
                _initialCommentDbo.CreatedById, 
                _initialCommentDbo.CreatedDate
                );
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Returns_Expected_Comment()
        {
            // Arrange
            _mockMapper.Setup(mapper => mapper.Map<Comment>(It.IsAny<CommentDbo>()))
                .Returns(_initialComment)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var commentRepository = new CommentRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);
            var comment = await commentRepository.GetAsync(_initialCommentDbo.CommentId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(_initialComment.TaskId, comment.TaskId);
            Assert.Equal(_initialComment.CommentId, comment.CommentId);
            Assert.Equal(_initialComment.CreatedBy, comment.CreatedBy);
            Assert.Equal(_initialComment.Text, comment.Text);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Wrong_CommentId_Throws_Exception()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // Arange  
                var nonExistentCommentId = Guid.NewGuid();
                using var dbContext = new TasksDbContext(_options);

                var commentRepository = new CommentRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);

                // Act 
                var error = Record.ExceptionAsync(async () => await commentRepository.GetAsync(nonExistentCommentId));

                // Assert  
                Assert.NotNull(error.Result);
                Assert.IsType<CommentNotFoundException>(error.Result);
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task AddAsync_Adds_New_Comment()
        {
            // Arrange
            var comment = new Comment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test comment",
                Guid.NewGuid(),
                DateTime.UtcNow
                );

            var commentDbo = new CommentDbo
            {
                CommentId = comment.CommentId,
                TaskId = comment.TaskId,
                Text = comment.Text,
                CreatedById = comment.CreatedBy,
                CreatedDate = comment.CreatedDate
            };

            _mockMapper.Setup(mapper => mapper.Map<CommentDbo>(It.IsAny<Comment>()))
                .Returns(commentDbo)
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<Comment>(It.IsAny<CommentDbo>()))
                .Returns(comment)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var commentRepository = new CommentRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);

            await commentRepository.AddAsync(comment);
            await commentRepository.SaveAsync();

            var newComment = await commentRepository.GetAsync(comment.CommentId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(newComment.TaskId, comment.TaskId);
            Assert.Equal(newComment.CommentId, comment.CommentId);
            Assert.Equal(newComment.Text, comment.Text);
            Assert.Equal(newComment.CreatedBy, comment.CreatedBy);
        }
    }
}
