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
    public class UpdateTaskHandlerTests
    {
        private readonly Mock<ICallbackService> _mockService = new Mock<ICallbackService>();
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<UpdateTaskValidator> _mockValidator = new Mock<UpdateTaskValidator>();

        private Task TestTask { get; set; } = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default,
            new Assignment(null, "n", Guid.Empty), default);
        
        public UpdateTaskHandlerTests()
        {
            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestTask)
               .Verifiable();

            _mockRepository.Setup(
                    repository => repository.UpdateTaskData(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(
                validator => validator.ValidateAndThrow(It.IsAny<UpdateTask>()))
                .Verifiable();
        }

        /// <summary>
        /// Tests whether a valid command results in a repository call and whether proper commands have been sent out based on final parameter.
        /// </summary>
        /// <param name="final">If true FinalizeTask command should be sent.</param>
        /// <returns></returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async System.Threading.Tasks.Task Valid_Command_Updates_Task(bool final)
        {
            //Arrange
            var command = new UpdateTask(TestTask.TaskId, "{\"name\":\"asd\"}", "status", Guid.NewGuid(), final);
            var updateTaskHandler = new UpdateTaskHandler(_mockRepository.Object, _mockService.Object, _mockValidator.Object);

            //Act
            var task = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.UpdateTaskData(It.Is<Task>(
                t => t.Data == command.Data 
                && t.TaskId == command.TaskId
                && command.FinalState == final)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()));
            _mockRepository.VerifyNoOtherCalls();

            Assert.Equal(TestTask, task);
            Assert.Equal(command.Data, task.Data);

            _mockRepository.VerifyAll();
            if (final)
            {
                _mockService.Verify(service => service.Callback(It.IsAny<Callback>(), It.IsAny<Task>()), Times.Once);
            }

            _mockService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_TaskAlreadyFinalStatus()
        {
            //Arrange
            Task FinalTask  = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default,
            new Assignment(null, "n", Guid.Empty), default, change: "Final", isFinal: true);

            _mockRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(FinalTask);

            var command = new UpdateTask(TestTask.TaskId, "{\"name\":\"asd\"}", "status", Guid.NewGuid(), true);
            var updateTaskHandler = new UpdateTaskHandler(_mockRepository.Object, _mockService.Object, _mockValidator.Object);

            //Act
            var exception = await Assert.ThrowsAsync<CannotModifyFinalizedTaskException>(async () => await updateTaskHandler.Handle(command, default));

            //Assert
            Assert.IsType<CannotModifyFinalizedTaskException>(exception);
        }
    }
}
