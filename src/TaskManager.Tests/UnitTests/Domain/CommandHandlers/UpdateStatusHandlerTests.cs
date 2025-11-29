using Moq;
using System;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Handlers;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class UpdateStatusHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<UpdateStatusValidator> _mockValidator = new Mock<UpdateStatusValidator>();

        private Task TestTask { get; set; } = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default, default, default, default, default);

        public UpdateStatusHandlerTests()
        {
            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestTask)
               .Verifiable();

            _mockRepository.Setup(
                repository => repository.Update(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                repository => repository.UpdateTaskStatus(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(
                validator => validator.ValidateAndThrow(It.IsAny<UpdateStatus>()))
                .Verifiable();
        }

        [Fact]
        public async System.Threading.Tasks.Task Returns_Updated_Task_On_Valid_Command()
        {
            //Arrange
            var command = new UpdateStatus(TestTask.TaskId, "status", Guid.NewGuid());
           
            var updateTaskHandler = new UpdateStatusHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var task = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.UpdateTaskStatus(It.Is<Task>(t => t.Status == command.Status && t.TaskId == command.TaskId)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()));
            _mockRepository.VerifyNoOtherCalls();

            Assert.Equal(TestTask, task);
            Assert.Equal(task.Status, command.Status);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_TaskAlreadyFinalStatus()
        {
            //Arrange
            Task FinalTask = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default,
            new Assignment(null, "n", Guid.Empty), default, change: "Final", isFinal: true);

            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(FinalTask);

            var command = new UpdateStatus(TestTask.TaskId, "status", Guid.NewGuid());
            var handler = new UpdateStatusHandler(_mockRepository.Object, _mockValidator.Object);

            //Act
            var exception = await Assert.ThrowsAsync<CannotModifyFinalizedTaskException>(async () => await handler.Handle(command, default));

            //Assert
            Assert.IsType<CannotModifyFinalizedTaskException>(exception);
        }
    }
}
