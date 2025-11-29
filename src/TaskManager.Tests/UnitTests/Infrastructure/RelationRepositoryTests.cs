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
    public class RelationRepositoryTests
    {
        private readonly DbContextOptions<TasksDbContext> _options =
            new DbContextOptionsBuilder<TasksDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private readonly Mock<IMediator> _mediatorMock = new Mock<IMediator>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly Relation _initialRelation;

        private readonly TaskDbo _initialTaskDbo;
        private readonly CommentDbo _initialCommentDbo;
        private readonly TaskRelationDbo _initialRelationDbo;
        private readonly TasksDbContext context;

        public RelationRepositoryTests()
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

            _initialRelation = new Relation(
                    _initialRelationDbo.RelationId,
                    taskId,
                    _initialRelationDbo.EntityId,
                    _initialRelationDbo.EntityType
                );
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Returns_Expected_Relation()
        {
            // Arrange
            _mockMapper.Setup(mapper => mapper.Map<Relation>(It.IsAny<TaskRelationDbo>()))
                .Returns(_initialRelation)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var relationRepository = new RelationRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);
            var relation = await relationRepository.GetAsync(_initialRelation.RelationId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(_initialRelation.TaskId, relation.TaskId);
            Assert.Equal(_initialRelation.RelationId, relation.RelationId);
            Assert.Equal(_initialRelation.EntityType, relation.EntityType);
            Assert.Equal(_initialRelation.EntityId, relation.EntityId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Wrong_RelationId_Throws_Exception()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // Arange  
                var nonExistentRelationId = Guid.NewGuid();
                using var dbContext = new TasksDbContext(_options);

                var relationRepository = new RelationRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);

                // Act 
                var error = Record.ExceptionAsync(async () => await relationRepository.GetAsync(nonExistentRelationId));

                // Assert  
                Assert.NotNull(error.Result);
                Assert.IsType<TaskRelationNotFoundException>(error.Result);
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task AddAsync_Adds_New_Relation()
        {
            // Arrange
            var relation = new Relation(
                Guid.NewGuid(),
                _initialTaskDbo.TaskId,
                Guid.NewGuid().ToString(),
                "Person");

            var relationDbo = new TaskRelationDbo
            {
                TaskId = relation.TaskId,
                RelationId = relation.RelationId,
                EntityId = relation.EntityId,
                EntityType = relation.EntityType
            };

            _mockMapper.Setup(mapper => mapper.Map<TaskRelationDbo>(It.IsAny<Relation>()))
                .Returns(relationDbo)
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<Relation>(It.IsAny<TaskRelationDbo>()))
                .Returns(relation)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var relationRepository = new RelationRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);

            await relationRepository.AddAsync(relation);
            await relationRepository.SaveAsync();

            var newRelation = await relationRepository.GetAsync(relation.RelationId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(newRelation.TaskId, relation.TaskId);
            Assert.Equal(newRelation.RelationId, relation.RelationId);
            Assert.Equal(newRelation.EntityId, relation.EntityId);
            Assert.Equal(newRelation.EntityType, relation.EntityType);
        }
    }
}
