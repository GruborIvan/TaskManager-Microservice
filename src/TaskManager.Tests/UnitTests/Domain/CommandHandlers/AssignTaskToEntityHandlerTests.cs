using Moq;
using System;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class AssignTaskToEntityHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<AssignTaskToEntityValidator> _mockValidator = new Mock<AssignTaskToEntityValidator>();

        private static Guid _taskId = Guid.NewGuid();
        private Task TestTask { get; set; } = new Task(_taskId, default, default, default, default, default, default, default, default,
            new Assignment(Guid.NewGuid(), "n", _taskId), default);

        public AssignTaskToEntityHandlerTests()
        {
            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestTask);

            _mockRepository.Setup(
                repository => repository.Update(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                    repository => repository.UpdateAssignment(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(v => v.ValidateAndThrow(It.IsAny<AssignTaskToEntity>()))
                .Verifiable();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidCommand_AssignsTask()
        {
            //Arrange
            var command = new AssignTaskToEntity(TestTask.TaskId, new Assignment(Guid.NewGuid(), "Group", default), Guid.NewGuid());

            var assignTaskHandler = new AssignTaskToEntityHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var task = await assignTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.UpdateAssignment(It.Is<Task>(t => t.Assignment.AssignedToEntityId == command.Assignment.AssignedToEntityId && t.TaskId == command.TaskId)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.VerifyNoOtherCalls();

            Assert.Equal(TestTask, task);
            Assert.Equal(command.Assignment, task.Assignment);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_TaskAlreadyFinalStatus()
        {
            //Arrange
            Task FinalTask = new Task(_taskId, default, default, default, default, default, default, default, default,
            new Assignment(Guid.NewGuid(), "n", _taskId), default, change: "Final", isFinal: true);

            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(FinalTask);

            var command = new AssignTaskToEntity(TestTask.TaskId, new Assignment(Guid.NewGuid(), "Group", default), Guid.NewGuid());
            var assignTaskHandler = new AssignTaskToEntityHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var exception = await Assert.ThrowsAsync<CannotModifyFinalizedTaskException>(async () => await assignTaskHandler.Handle(command, default));

            //Assert
            Assert.IsType<CannotModifyFinalizedTaskException>(exception);
        }
    }
}
