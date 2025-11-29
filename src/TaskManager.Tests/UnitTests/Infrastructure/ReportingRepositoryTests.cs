using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Models.Reporting;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.UnitTests.Infrastructure
{
    public class ReportingRepositoryTests
    {
        private readonly DbContextOptions<TasksDbContext> _options =
            new DbContextOptionsBuilder<TasksDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        private readonly IMapper _autoMapperMock;

        public ReportingRepositoryTests()
        {
            _autoMapperMock = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TaskDbo, TaskReport>();
                cfg.CreateMap<TaskRelationDbo, TaskRelationReport>();
                cfg.CreateMap<CommentDbo, CommentReport>();
            }).CreateMapper();
        }

        [Fact]
        public async Task GetTasksAsync_Returns_Tasks()
        {
            // Arange  
            var taskDbos = new List<TaskDbo>
                {
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow,
                        ChangedDate = null
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = null
                    },
                };
            using var context = new TasksDbContext(_options);
            context.Tasks.AddRange(taskDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var tasks = await repository.GetTasksAsync(null, null);

            // Assert  
            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count());

            var expectedTask1 = taskDbos.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var expectedTask2 = taskDbos.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));
            var taskObject1 = tasks.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var taskObject2 = tasks.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedTask1.TaskId, taskObject1.TaskId);
            Assert.Equal(expectedTask1.TaskType, taskObject1.TaskType);
            Assert.Equal(expectedTask1.FourEyeSubjectId, taskObject1.FourEyeSubjectId);
            Assert.Equal(expectedTask1.Status, taskObject1.Status);
            Assert.Equal(expectedTask1.Data, taskObject1.Data);
            Assert.Equal(expectedTask1.CreatedDate, taskObject1.CreatedDate);
            Assert.Equal(expectedTask1.ChangedDate, taskObject1.ChangedDate);

            Assert.Equal(expectedTask2.TaskId, taskObject2.TaskId);
            Assert.Equal(expectedTask2.TaskType, taskObject2.TaskType);
            Assert.Equal(expectedTask2.FourEyeSubjectId, taskObject2.FourEyeSubjectId);
            Assert.Equal(expectedTask2.Status, taskObject2.Status);
            Assert.Equal(expectedTask2.Data, taskObject2.Data);
            Assert.Equal(expectedTask2.CreatedDate, taskObject2.CreatedDate);
            Assert.Equal(expectedTask2.ChangedDate, taskObject2.ChangedDate);
        }

        [Fact]
        public async Task GetTasksAsync_FromDateYesterday_Returns_OneTask()
        {
            // Arange  
            var taskDbos = new List<TaskDbo>
                {
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow,
                        ChangedDate = null
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = null
                    },
                };
            using var context = new TasksDbContext(_options);
            context.Tasks.AddRange(taskDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var tasks = await repository.GetTasksAsync(DateTime.UtcNow.AddDays(-1), null);

            // Assert  
            Assert.NotNull(tasks);
            Assert.Single(tasks);

            var expectedTask = taskDbos.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var taskObject = tasks.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedTask.TaskId, taskObject.TaskId);
            Assert.Equal(expectedTask.TaskType, taskObject.TaskType);
            Assert.Equal(expectedTask.FourEyeSubjectId, taskObject.FourEyeSubjectId);
            Assert.Equal(expectedTask.Status, taskObject.Status);
            Assert.Equal(expectedTask.Data, taskObject.Data);
            Assert.Equal(expectedTask.CreatedDate, taskObject.CreatedDate);
            Assert.Equal(expectedTask.ChangedDate, taskObject.ChangedDate);
        }

        [Fact]
        public async Task GetTasksAsync_FromDateInPast_ToDateYesterday_Returns_OneTask()
        {
            // Arange  
            var taskDbos = new List<TaskDbo>
                {
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow,
                        ChangedDate = DateTime.UtcNow
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = DateTime.UtcNow
                    }
                };
            using var context = new TasksDbContext(_options);
            context.Tasks.AddRange(taskDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var tasks = await repository.GetTasksAsync(DateTime.Now.AddDays(-50), DateTime.Now.AddDays(-1));

            // Assert  
            Assert.NotNull(tasks);
            Assert.Single(tasks);
            var expectedTask = taskDbos.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1) && x.ChangedDate < DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedTask.TaskId, tasks.Single().TaskId);
            Assert.Equal(expectedTask.TaskType, tasks.Single().TaskType);
            Assert.Equal(expectedTask.FourEyeSubjectId, tasks.Single().FourEyeSubjectId);
            Assert.Equal(expectedTask.Status, tasks.Single().Status);
            Assert.Equal(expectedTask.Data, tasks.Single().Data);
            Assert.Equal(expectedTask.CreatedDate, tasks.Single().CreatedDate);
            Assert.Equal(expectedTask.ChangedDate, tasks.Single().ChangedDate);
        }

        [Fact]
        public async Task GetCommentsAsync_Returns_Comments()
        {
            // Arange  
            var commentDbos = new List<CommentDbo>
                {
                    new CommentDbo
                    {
                        CommentId = Guid.NewGuid(),
                        TaskId = Guid.NewGuid(),
                        Text = "test",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = Guid.NewGuid()
                    },
                    new CommentDbo
                    {
                        CommentId = Guid.NewGuid(),
                        TaskId = Guid.NewGuid(),
                        Text = "test",
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        CreatedById = Guid.NewGuid()
                    }
                };
            using var context = new TasksDbContext(_options);
            context.Comments.AddRange(commentDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var comments = await repository.GetCommentsAsync(null, null);

            // Assert  
            Assert.NotNull(comments);
            Assert.Equal(2, comments.Count());

            var expectedComment1 = commentDbos.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var expectedComment2 = commentDbos.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));
            var commentObject1 = comments.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var commentObject2 = comments.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedComment1.CommentId, commentObject1.CommentId);
            Assert.Equal(expectedComment1.TaskId, commentObject1.TaskId);
            Assert.Equal(expectedComment1.Text, commentObject1.Text);
            Assert.Equal(expectedComment1.CreatedDate, commentObject1.CreatedDate);
            Assert.Equal(expectedComment1.CreatedById, commentObject1.CreatedById);

            Assert.Equal(expectedComment2.CommentId, commentObject2.CommentId);
            Assert.Equal(expectedComment2.TaskId, commentObject2.TaskId);
            Assert.Equal(expectedComment2.Text, commentObject2.Text);
            Assert.Equal(expectedComment2.CreatedDate, commentObject2.CreatedDate);
            Assert.Equal(expectedComment2.CreatedById, commentObject2.CreatedById);
        }

        [Fact]
        public async Task GetCommentsAsync_FromDateYesterday_Returns_OneComment()
        {
            // Arange  
            var commentDbos = new List<CommentDbo>
            {
                new CommentDbo
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = Guid.NewGuid()
                },
                new CommentDbo
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    CreatedById = Guid.NewGuid()
                }
            };
            using var context = new TasksDbContext(_options);
            context.Comments.AddRange(commentDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var comments = await repository.GetCommentsAsync(DateTime.UtcNow.AddDays(-1), null);

            // Assert  
            Assert.NotNull(comments);
            Assert.Single(comments);

            var expectedComment = commentDbos.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var commentObject = comments.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedComment.CommentId, commentObject.CommentId);
            Assert.Equal(expectedComment.TaskId, commentObject.TaskId);
            Assert.Equal(expectedComment.Text, commentObject.Text);
            Assert.Equal(expectedComment.CreatedDate, commentObject.CreatedDate);
            Assert.Equal(expectedComment.CreatedById, commentObject.CreatedById);
        }

        [Fact]
        public async Task GetCommentsAsync_FromDateInPast_ToDateYesterday_Returns_OneComment()
        {
            // Arange  
            var commentDbos = new List<CommentDbo>
            {
                new CommentDbo
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = Guid.NewGuid()
                },
                new CommentDbo
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    CreatedById = Guid.NewGuid()
                }
            };
            using var context = new TasksDbContext(_options);
            context.Comments.AddRange(commentDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var comments = await repository.GetCommentsAsync(DateTime.Now.AddDays(-50), DateTime.Now.AddDays(-1));

            // Assert  
            Assert.NotNull(comments);
            Assert.Single(comments);
            var expectedComment = commentDbos.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));
            var commentObject = comments.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));

            Assert.Equal(expectedComment.CommentId, commentObject.CommentId);
            Assert.Equal(expectedComment.TaskId, commentObject.TaskId);
            Assert.Equal(expectedComment.Text, commentObject.Text);
            Assert.Equal(expectedComment.CreatedDate, commentObject.CreatedDate);
            Assert.Equal(expectedComment.CreatedById, commentObject.CreatedById);
        }

        [Fact]
        public async Task GetTaskRelationsAsync_Returns_TaskRelations()
        {
            // Arange  
            var relationDbos = new List<TaskRelationDbo>
            {
                new TaskRelationDbo
                {
                    RelationId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    EntityId = Guid.NewGuid().ToString(),
                    EntityType = "Person",
                    IsMain = true
                },
                new TaskRelationDbo
                {
                    RelationId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    EntityId = Guid.NewGuid().ToString(),
                    EntityType = "Person",
                    IsMain = false
                }
            };
            using var context = new TasksDbContext(_options);
            context.TaskRelations.AddRange(relationDbos);
            context.SaveChanges();

            var repository = new ReportingRepository(context, _autoMapperMock);

            // Act 
            var relations = await repository.GetTaskRelationsAsync();

            // Assert  
            Assert.NotNull(relations);
            Assert.Equal(2, relations.Count());
        }
    }
}
