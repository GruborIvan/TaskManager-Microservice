using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.UnitTests.Infrastructure
{
    public class TaskRepositoryTests
    {
        private readonly DbContextOptions<TasksDbContext> _options =
            new DbContextOptionsBuilder<TasksDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private readonly Mock<IMediator> _mediatorMock = new Mock<IMediator>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly Task _initialTask;

        private readonly TaskDbo _initialTaskDbo;
        private readonly CommentDbo _initialCommentDbo;
        private readonly TaskRelationDbo _initialRelationDbo;
        private readonly IEnumerable<TaskDbo> _initialTaskHistoryDbo;
        private readonly Guid _initatiorId = Guid.NewGuid();
        private readonly TasksDbContext context;

        public TaskRepositoryTests()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _initialCommentDbo = new CommentDbo
            {
                CommentId = Guid.NewGuid(),
                TaskId = taskId,
                Text = "test comment"
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
            _initialTaskHistoryDbo = new List<TaskDbo>
            {
                _initialTaskDbo
            };

            context = new TasksDbContext(_options);
            context.Tasks.Add(_initialTaskDbo);
            context.Comments.Add(_initialCommentDbo);

            context.SaveChanges();

            _initialTask = new Task(
                _initialTaskDbo.TaskId,
               _initialTaskDbo.TaskType,
                new HttpCallback(new Uri(_initialTaskDbo.Callback)),
                _initialTaskDbo.FourEyeSubjectId,
                _initialTaskDbo.Subject,
                new Source(
                    _initialTaskDbo.SourceId,
                    _initialTaskDbo.SourceName),
                new Comment[] { new Comment(_initialCommentDbo.CommentId, _initialTaskDbo.TaskId, _initialCommentDbo.Text) }.AsEnumerable(),
                _initialTaskDbo.Status,
                _initialTaskDbo.Data,
                new Assignment(null, "Unassigned", _initialTaskDbo.TaskId),
                new Relation[] { new Relation(_initialRelationDbo.RelationId, _initialTaskDbo.TaskId, _initialRelationDbo.EntityId, _initialRelationDbo.EntityType) }.AsEnumerable());
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Returns_Expected_Task()
        {
            // Arrange
            _mockMapper.Setup(mapper => mapper.Map<Task>(It.IsAny<TaskDbo>()))
                .Returns(_initialTask)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var taskRepository = new TaskRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);
            var fetchedTask = await taskRepository.GetAsync(_initialTask.TaskId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(_initialTask.TaskId, fetchedTask.TaskId);
            Assert.Equal(_initialTask.TaskType, fetchedTask.TaskType);
            Assert.Equal(_initialTask.FourEyeSubjectId, fetchedTask.FourEyeSubjectId);
            Assert.Equal(_initialTask.Status, fetchedTask.Status);
            Assert.Equal(_initialTask.Data, fetchedTask.Data);
            Assert.Equal(_initialCommentDbo.Text, fetchedTask.Comments.First().Text);
            Assert.Equal(_initialRelationDbo.EntityId, fetchedTask.Relations.First().EntityId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAsync_Wrong_TaskId_Throws_Exception()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // Arange  
                var nonExistentTaskId = Guid.NewGuid();
                using var dbContext = new TasksDbContext(_options);

                var taskRepository = new TaskRepository(dbContext, _mediatorMock.Object, _mockMapper.Object);

                // Act 
                var error = Record.ExceptionAsync(async () => await taskRepository.GetAsync(nonExistentTaskId));

                // Assert  
                Assert.NotNull(error.Result);
                Assert.IsType<TaskNotFoundException>(error.Result);
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task AddAsync_Adds_New_Task()
        {
            // Arrange
            var newTaskId = Guid.NewGuid();
            var callbackUri = new Uri("https://url.com");
            var newTask = new Task(
                newTaskId,
                "task-type",
                new HttpCallback(callbackUri),
                Guid.NewGuid(),
                "subject",
                new Source(
                    "sourceId",
                    "sourceName"),
                new Comment[0].AsEnumerable(),
                "status",
                "data",
                new Assignment(null, "Unassigned", Guid.Empty),
                new Relation[] { new Relation(Guid.NewGuid(), newTaskId, Guid.NewGuid().ToString(), "Person") }.AsEnumerable());

            var newTaskDbo = new TaskDbo
            {
                TaskId = newTaskId,
                TaskType = newTask.TaskType,
                Callback = newTask.Callback.Parameters,
                FourEyeSubjectId = newTask.FourEyeSubjectId,
                Subject = newTask.Subject,
                SourceId = newTask.Source.SourceId,
                SourceName = newTask.Source.SourceName,
                Status = newTask.Status,
                AssignedToEntityId = newTask.Assignment.AssignedToEntityId,
                AssignmentType = newTask.Assignment.Type,
                TaskRelations = newTask.Relations.Select(x => new TaskRelationDbo
                {
                    RelationId = x.RelationId,
                    EntityId = x.EntityId,
                    EntityType = x.EntityType
                }).ToList()
            };

            _mockMapper.Setup(mapper => mapper.Map<TaskDbo>(It.IsAny<Task>()))
                .Returns(newTaskDbo)
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<Task>(It.IsAny<TaskDbo>()))
                .Returns(newTask)
                .Verifiable();

            // Act
            using var dbContext = new TasksDbContext(_options);
            var taskRepository = new TaskRepository(context, _mediatorMock.Object, _mockMapper.Object);

            await taskRepository.AddAsync(newTask);
            await taskRepository.SaveAsync();

            var task = await taskRepository.GetAsync(newTask.TaskId);

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(newTask.TaskId, task.TaskId);
            Assert.Equal(newTask.Status, task.Status);
            Assert.Equal(1, task.Relations.Count);
            Assert.Equal(callbackUri.ToString(), task.Callback.Parameters);
        }

        [Fact]
        public async System.Threading.Tasks.Task Update_Updates_Existing_Task()
        {
            // Arrange
            var newStatus = "newStatus";
            _mockMapper.Setup(mapper => mapper.Map<Task>(It.IsAny<TaskDbo>()))
                .Returns(_initialTask)
                .Verifiable();

            var taskRepository = new TaskRepository(context, _mediatorMock.Object, _mockMapper.Object);
            var task = await taskRepository.GetAsync(_initialTask.TaskId);
            task.UpdateStatus(newStatus, Guid.NewGuid());
            _initialTaskDbo.Status = newStatus;

            _mockMapper.Setup(mapper => mapper.Map<TaskDbo>(It.IsAny<Task>()))
                .Returns(_initialTaskDbo)
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<Task>(It.IsAny<TaskDbo>()))
                .Returns(task)
                .Verifiable();

            context.Entry<TaskDbo>(_initialTaskDbo).State = EntityState.Detached;

            // Act
            var updatedTask = taskRepository.Update(task);
            await taskRepository.SaveAsync();

            // Assert
            _mockMapper.Verify();
            _mediatorMock.Verify();

            Assert.Equal(task.Status, updatedTask.Status);
        }
    }
}
