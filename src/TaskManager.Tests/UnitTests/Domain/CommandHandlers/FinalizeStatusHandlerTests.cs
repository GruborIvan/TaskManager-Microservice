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
    public class FinalizeStatusHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<ICallbackService> _mockCallbackService = new Mock<ICallbackService>();
        private readonly Mock<FinalizeStatusValidator> _mockValidator = new Mock<FinalizeStatusValidator>();

        private Task TestTask { get; set; } = new Task(Guid.NewGuid(), default, new HttpCallback(new Uri("http://test.com")), default, default, default, default, default, default, default, default, default, default);

        public FinalizeStatusHandlerTests()
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
                repository => repository.FinalizeTask(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(v => v.ValidateAndThrow(It.IsAny<FinalizeStatus>()))
                .Verifiable();

            _mockCallbackService.Setup(x=> x.Callback(It.IsAny<Callback>(), It.IsAny<Task>()))
                .Verifiable();
        }

        [Fact]
        public async System.Threading.Tasks.Task Returns_Updated_Task_On_Valid_Command()
        {
            //Arrange
            var command = new FinalizeStatus(TestTask.TaskId, "status", Guid.NewGuid());
           
            var updateTaskHandler = new FinalizeStatusHandler(_mockRepository.Object, _mockCallbackService.Object, _mockValidator.Object);

            //Act
            var task = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.FinalizeTask(It.Is<Task>(t => t.Status == command.Status && t.TaskId == command.TaskId)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()), Times.Once());
            _mockCallbackService.Verify(_ => _.Callback(It.IsAny<Callback>(), It.IsAny<Task>()), Times.Once());
            _mockRepository.VerifyNoOtherCalls();

            Assert.Equal(TestTask, task);
            Assert.Equal(task.Status, command.Status);
        }

        [Fact]
        public async System.Threading.Tasks.Task TaskWithoutCallback_Returns_Updated_Task_On_Valid_Command()
        {
            //Arrange
             var testTask = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default, default, default, default, default);
            _mockRepository.Setup(
                    repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testTask)
                .Verifiable();
            _mockRepository.Setup(
                    repository => repository.FinalizeTask(It.IsAny<Task>()))
                .Returns(testTask)
                .Verifiable();

            var command = new FinalizeStatus(testTask.TaskId, "status", Guid.NewGuid());

            var updateTaskHandler = new FinalizeStatusHandler(_mockRepository.Object, _mockCallbackService.Object, _mockValidator.Object);

            //Act
            var task = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.FinalizeTask(It.Is<Task>(t => t.Status == command.Status && t.TaskId == command.TaskId)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()), Times.Once());
            _mockCallbackService.Verify(_ => _.Callback(It.IsAny<Callback>(), It.IsAny<Task>()), Times.Never());

            Assert.Equal(testTask, task);
            Assert.Equal(task.Status, command.Status);
        }

        [Fact]
        public async System.Threading.Tasks.Task On_FourEye_Review_Throws_Before_Updating_Repository()
        {
            //Arrange
            var command = new FinalizeStatus(TestTask.TaskId, "status", TestTask.FourEyeSubjectId, true);
            var updateTaskHandler = new FinalizeStatusHandler(_mockRepository.Object, _mockCallbackService.Object, _mockValidator.Object);

            //Act
            //Assert
            await Assert.ThrowsAsync<FourEyeRequirementNotMetException>(async () => await updateTaskHandler.Handle(command, default));

            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.VerifyNoOtherCalls();

            Assert.NotEqual(TestTask.Status, command.Status);
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

            var command = new FinalizeStatus(TestTask.TaskId, "status", TestTask.FourEyeSubjectId, true);
            var updateTaskHandler = new FinalizeStatusHandler(_mockRepository.Object, _mockCallbackService.Object, _mockValidator.Object);

            //Act
            var exception = await Assert.ThrowsAsync<CannotModifyFinalizedTaskException>(async () => await updateTaskHandler.Handle(command, default));

            //Assert
            Assert.IsType<CannotModifyFinalizedTaskException>(exception);
        }
    }
}
