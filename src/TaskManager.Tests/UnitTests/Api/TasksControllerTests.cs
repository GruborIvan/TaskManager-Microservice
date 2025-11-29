using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.Models;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using Xunit;

namespace TaskManager.Tests.UnitTests.API
{
    public class TasksControllerTests
    {
        private static readonly ILogger<TasksController> _mockLoggerObject =
            new Mock<ILogger<TasksController>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<ITaskRepository> _mockTaskRepo = new Mock<ITaskRepository>();
        private readonly System.Security.Claims.ClaimsPrincipal _userClaimsPrincipal;

        public TasksControllerTests()
        {
            _mockTaskRepo.Setup(repo =>
                    repo.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(Task))
                .Verifiable();

            _mockTaskRepo.Setup(repo =>
                    repo.GetTaskHistoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Task>() { default, default })
                .Verifiable();

            _userClaimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim("UserId", Guid.NewGuid().ToString())
            }, "mock"));
        }

        [Fact]
        public async System.Threading.Tasks.Task SendCreateTaskMessage_Returns_AcceptedResult()
        {
            // Arrange
            var expectedCommand = new SaveTask("asdasd", Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", "New", new Assignment(Guid.NewGuid(), "a", Guid.Empty), default, default, default,
                "sourcename",
                "sourceSubject", "comment");

            _mockMapper.Setup(mapper => mapper.Map<SaveTask>(It.IsAny<SendCreateTaskMessageRequest>()))
                .Returns(expectedCommand);

            _mockMediator.Setup(mediator =>
                    mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Task(
                    Guid.NewGuid(),
                    default, default, default, default, default, default, default, default, default, default, default, default))
                .Verifiable();

            var controller = new TasksController(
                _mockMediator.Object,
                _mockMapper.Object,
                _mockLoggerObject,
                _mockTaskRepo.Object
                );
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            // Act
            var request = new SendCreateTaskMessageRequest();
            var actionResult = await controller.SendCreateTaskMessage(request);

            // Assert
            _mockMediator.VerifyAll();

            Assert.NotNull(actionResult);
            Assert.IsType<OkObjectResult>(actionResult);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendCreateTaskMessage_Returns_BadRequest()
        {
            // Arrange
            var invalidCommand = new SaveTask("asdasd", Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", "New", new Assignment(Guid.NewGuid(), "a", Guid.Empty), default, default, default, default, default, default);

            _mockMapper.Setup(mapper => mapper.Map<SaveTask>(It.IsAny<SendCreateTaskMessageRequest>()))
                .Returns(invalidCommand);

            _mockMediator.Setup(mediator =>
                    mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("error message"));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var request = new SendCreateTaskMessageRequest();
            var actionResult = await controller.SendCreateTaskMessage(request);

            // Assert
            _mockMediator.Verify(service => service.Send(invalidCommand, It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(service => service.Publish(It.Is<CreateTaskFailed>(x => x.Error.Message.Contains("error message")), It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(actionResult);
            Assert.IsType<BadRequestResult>(actionResult);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendCreateTaskMessage_Returns_InternalServerError()
        {
            _mockMediator.Setup(mediator =>
                    mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error message"))
                .Verifiable();

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var request = new SendCreateTaskMessageRequest();
            var actionResult = await controller.SendCreateTaskMessage(request);

            // Assert
            _mockMediator.Verify(service => service.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(service => service.Publish(It.Is<CreateTaskFailed>(x => x.Error.Message.Contains("error message")), It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(actionResult);

            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskById_Returns_Task()
        {
            // Arrange
            var taskId = Guid.Parse("21fc38b3-da2e-4c0e-a058-3b4e170e592f");

            _mockMapper.Setup(mapper => mapper.Map<TaskDto>(It.IsAny<(Task, Guid)>()))
                .Returns(new TaskDto() { TaskId = taskId });

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            controller.Request.Headers.Add("x-user-id", "F82D575E-4D17-47AB-A4F0-3B1011838345");

            // Act
            var actionResult = await controller.GetTaskById(taskId);

            // Assert
            Assert.NotNull(actionResult);

            var task = actionResult.Value;

            Assert.NotNull(task);
            Assert.IsType<TaskDto>(task);
            Assert.Equal(taskId, task.TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskById_Returns_TaskWithRelations()
        {
            // Arrange
            var taskId = Guid.Parse("21fc38b3-da2e-4c0e-a058-3b4e170e592f");
            var personId = Guid.NewGuid();

            _mockTaskRepo.Setup(repo =>
                    repo.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Task(taskId, "test", null, Guid.Empty, "test",null,null,null,null,null,new List<Relation>(){new Relation(Guid.NewGuid(), taskId, personId.ToString(),"Person")}))
                .Verifiable();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Comment, CommentDto>()
                    .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy));

                cfg.CreateMap<Relation, RelationDto>();

                cfg.CreateMap<(Task task, Guid userId), TaskDto>()
                    .ForMember(dest => dest.Callback, opt => opt.MapFrom(src => src.task.Callback.Parameters))
                    .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.task.Change))
                    .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.task.ChangedBy))
                    .ForMember(dest => dest.ChangedDate, opt => opt.MapFrom(src => src.task.ChangedDate))
                    .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.task.Comments))
                    .ForMember(dest => dest.Relations, opt => opt.MapFrom(src => src.task.Relations))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.task.Status))
                    .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.task.CreatedBy))
                    .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.task.CreatedDate))
                    .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.task.Data))
                    .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.task.Subject))
                    .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.task.TaskId))
                    .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => src.task.TaskType))
                    .ForMember(dest => dest.AssignedToEntityId, opt => opt.MapFrom(src => src.task.Assignment.AssignedToEntityId))
                    .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.task.Assignment.Type))
                    .ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => src.task.Source.SourceId))
                    .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.task.Source.SourceName))
                    .ForMember(dest => dest.FinalState, opt => opt.MapFrom(src => src.task.IsFinal))
                    .ForMember(dest => dest.SubjectUnder4Eye, opt => opt.MapFrom(src => src.task.FourEyeSubjectId == src.userId));
            }).CreateMapper();

            var controller = new TasksController(_mockMediator.Object, mapper, _mockLoggerObject, _mockTaskRepo.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            controller.Request.Headers.Add("x-user-id", "F82D575E-4D17-47AB-A4F0-3B1011838345");

            // Act
            var actionResult = await controller.GetTaskById(taskId);

            // Assert
            Assert.NotNull(actionResult);

            var task = actionResult.Value;

            Assert.NotNull(task);
            Assert.IsType<TaskDto>(task);
            Assert.Equal(taskId, task.TaskId);
            Assert.Equal("Person", task.Relations.Single().EntityType);
            Assert.Equal(personId.ToString(), task.Relations.Single().EntityId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskById_Returns_NotFound()
        {
            // Arrange
            var nonExistingTaskId = Guid.NewGuid();

            _mockMediator.Setup(mediator =>
                    mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskNotFoundException(nonExistingTaskId));

            _mockTaskRepo.Setup(repo => repo.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskNotFoundException(nonExistingTaskId));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var actionResult = await controller.GetTaskById(nonExistingTaskId);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskById_With_Empty_Guid_Returns_BadRequest()
        {
            // Arrange
            var invalidTaskId = Guid.Empty;

            _mockTaskRepo.Setup(repo => repo.GetAsync(It.Is<Guid>(g => g == default), It.IsAny<CancellationToken>())).ThrowsAsync(new ValidationException(invalidTaskId.ToString()));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var actionResult = await controller.GetTaskById(invalidTaskId);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskById_With_Empty_Guid_Returns_InternalServerError()
        {
            // Arrange
            var invalidTaskId = Guid.Empty;

            _mockTaskRepo.Setup(repo => repo.GetAsync(It.Is<Guid>(g => g == default), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException(invalidTaskId.ToString()));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var actionResult = await controller.GetTaskById(invalidTaskId);

            // Assert
            Assert.NotNull(actionResult);
            var statusCodeResult = actionResult.Result as StatusCodeResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskHistoryByTaskId_Returns_TaskHistory()
        {
            // Arrange
            var taskId = Guid.Parse("21fc38b3-da2e-4c0e-a058-3b4e170e592f");

            _mockMapper.Setup(mapper => mapper.Map<TaskDto>(It.IsAny<(Task, Guid)>()))
                .Returns(new TaskDto { TaskId = taskId });


            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            // Act
            var actionResult = await controller.GetTaskHistoryByTaskId(taskId);

            // Assert
            Assert.NotNull(actionResult);

            var tasks = actionResult.Value;

            Assert.NotNull(tasks);
            Assert.IsType<TaskDto[]>(tasks);
            Assert.Equal(taskId, tasks.First().TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskHistoryByTaskId_Returns_BadRequest()
        {
            // Arrange
            var invalidTaskId = Guid.NewGuid();

            _mockMapper.Setup(mapper => mapper.Map<TaskDto>(It.IsAny<(Task, Guid)>()))
                .Throws(new ValidationException(Guid.NewGuid().ToString()));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            // Act
            var actionResult = await controller.GetTaskHistoryByTaskId(invalidTaskId);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskHistoryByTaskId_Returns_NotFound()
        {
            // Arrange
            var invalidTaskId = Guid.NewGuid();

            _mockMapper.Setup(mapper => mapper.Map<TaskDto>(It.IsAny<(Task, Guid)>()))
                .Throws(new TaskNotFoundException(Guid.NewGuid()));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _userClaimsPrincipal }
            };
            // Act
            var actionResult = await controller.GetTaskHistoryByTaskId(invalidTaskId);

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskHistoryByTaskId_Returns_InternalServerError()
        {
            // Arrange
            var invalidTaskId = Guid.NewGuid();

            _mockMapper.Setup(mapper => mapper.Map<TaskDto>(It.IsAny<(Task,Guid)>()))
                .Throws(new ArgumentNullException(invalidTaskId.ToString()));

            var controller = new TasksController(_mockMediator.Object, _mockMapper.Object, _mockLoggerObject, _mockTaskRepo.Object);

            // Act
            var actionResult = await controller.GetTaskHistoryByTaskId(invalidTaskId);

            // Assert
            Assert.NotNull(actionResult);
            var statusCodeResult = actionResult.Result as StatusCodeResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }
    }
}
