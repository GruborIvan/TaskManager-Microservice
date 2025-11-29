using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class SaveTaskHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<SaveTaskValidator> _mockValidator = new Mock<SaveTaskValidator>();

        public SaveTaskHandlerTests()
        {
            _mockValidator.Setup(validator => validator.ValidateAndThrow(It.IsAny<SaveTask>()))
                .Verifiable();
        }

        [Theory]
        [MemberData(nameof(ValidCommands))]
        public async System.Threading.Tasks.Task Valid_Command_Succeeds((Assignment assignment, bool final) expected)
        {
            // Arrange
            var taskId = Guid.NewGuid();

            var command = new SaveTask(
                "source",
                Guid.NewGuid(),
                "{}",
                "http://www.test.com/",
                "CreateTask",
                "New",
                expected.assignment,
                Guid.NewGuid(),
                default,
                new List<Relation>(),
                "sourcename",
                "sourceSubject",
                "comment");

            var task = new Task(
                taskId, 
                command.TaskType, 
                new HttpCallback(new Uri(command.Callback)), 
                command.FourEyeSubjectId, 
                command.Subject, 
                new Source(command.SourceId, command.SourceName), 
                default, 
                command.Status, 
                command.Data,
                expected.assignment, 
                command.Relations);

            _mockRepository.Setup(repository => repository.AddAsync(It.IsAny<Task>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(task)
                .Verifiable();

            _mockRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            var saveTaskHandler = new SaveTaskHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var newTask = await saveTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.VerifyAll();
            _mockRepository.Verify(x=>x.AddAsync(It.Is<Task>(x=>x.Comments.Count == 1), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(command.SourceId, newTask.Source.SourceId);
            Assert.Equal(command.SourceName, newTask.Source.SourceName);
            Assert.Equal(command.Assignment.Type, newTask.Assignment.Type);
            Assert.Equal(command.Assignment.AssignedToEntityId, newTask.Assignment.AssignedToEntityId);
            Assert.Equal(command.Callback, (newTask.Callback as HttpCallback).Url.AbsoluteUri);
            Assert.Equal(command.Data, newTask.Data);
            Assert.Equal(command.FourEyeSubjectId, newTask.FourEyeSubjectId);
            Assert.Equal(command.InitiatedBy, newTask.CreatedBy);
            Assert.Equal(command.Relations, newTask.Relations);
            Assert.Equal(command.Status, newTask.Status);
            Assert.Equal(command.Subject, newTask.Subject);
            Assert.Equal(command.TaskType, newTask.TaskType);
        }

        [Theory]
        [MemberData(nameof(ValidCommands))]
        public async System.Threading.Tasks.Task Valid_Command_WithoutComment_Succeeds((Assignment assignment, bool final) expected)
        {
            // Arrange
            var taskId = Guid.NewGuid();

            var command = new SaveTask(
                "source",
                Guid.NewGuid(),
                "{}",
                "http://www.test.com/",
                "CreateTask",
                "New",
                expected.assignment,
                Guid.NewGuid(),
                default,
                new List<Relation>(),
                "sourcename",
                "sourceSubject",
                null);

            var task = new Task(
                taskId,
                command.TaskType,
                new HttpCallback(new Uri(command.Callback)),
                command.FourEyeSubjectId,
                command.Subject,
                new Source(command.SourceId, command.SourceName),
                default,
                command.Status,
                command.Data,
                expected.assignment,
                command.Relations);

            _mockRepository.Setup(repository => repository.AddAsync(It.IsAny<Task>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(task)
                .Verifiable();

            _mockRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            var saveTaskHandler = new SaveTaskHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var newTask = await saveTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.VerifyAll();
            _mockRepository.Verify(x => x.AddAsync(It.Is<Task>(x => x.Comments.Count == 1), It.IsAny<CancellationToken>()), Times.Never);

            Assert.Equal(command.SourceId, newTask.Source.SourceId);
            Assert.Equal(command.SourceName, newTask.Source.SourceName);
            Assert.Equal(command.Assignment.Type, newTask.Assignment.Type);
            Assert.Equal(command.Assignment.AssignedToEntityId, newTask.Assignment.AssignedToEntityId);
            Assert.Equal(command.Callback, (newTask.Callback as HttpCallback).Url.AbsoluteUri);
            Assert.Equal(command.Data, newTask.Data);
            Assert.Equal(command.FourEyeSubjectId, newTask.FourEyeSubjectId);
            Assert.Equal(command.InitiatedBy, newTask.CreatedBy);
            Assert.Equal(command.Relations, newTask.Relations);
            Assert.Equal(command.Status, newTask.Status);
            Assert.Equal(command.Subject, newTask.Subject);
            Assert.Equal(command.TaskType, newTask.TaskType);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_TaskAlreadyExists()
        {
            _mockRepository.Setup(repository => repository.AddAsync(It.IsAny<Task>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception())
                .Verifiable();

            _mockRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            var createTaskHandler = new SaveTaskHandler(_mockRepository.Object, _mockValidator.Object);

            var taskId = Guid.NewGuid();

            var command = new SaveTask(
                "source",
                Guid.NewGuid(),
                "{}",
                "http://www.test.com/",
                "CreateTask",
                "New",
                new Assignment(Guid.NewGuid(), "Person", taskId),
                Guid.NewGuid(),
                default,
                new List<Relation>(),
                "sourcename",
                "sourceSubject",
                "comment");

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await createTaskHandler.Handle(command, default));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        public static IEnumerable<object[]> ValidCommands
        {
            get
            {
                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), "User", default), true)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, "User", default), true)
                };
                yield return new object[]
                {
                    (new Assignment(null, "User", default), true)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), string.Empty, default), true)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), null, default), true)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, null, default), true)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, string.Empty, default), true)
                };
                yield return new object[]
                {
                    (new Assignment(null, null, default), true)
                };
                yield return new object[]
                {
                    (new Assignment(null, string.Empty, default), true)
                };


                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), "User", default), false)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, "User", default), false)
                };
                yield return new object[]
                {
                    (new Assignment(null, "User", default), false)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), string.Empty, default), false)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.NewGuid(), null, default), false)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, null, default), false)
                };
                yield return new object[]
                {
                    (new Assignment(Guid.Empty, string.Empty, default), false)
                };
                yield return new object[]
                {
                    (new Assignment(null, null, default), false)
                };
                yield return new object[]
                {
                    (new Assignment(null, string.Empty, default), false)
                };
            }
        }

    }
}
