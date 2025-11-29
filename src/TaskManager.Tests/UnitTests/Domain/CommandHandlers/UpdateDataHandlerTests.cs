using Moq;
using System;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Handlers;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class UpdateDataHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository = new Mock<ITaskRepository>();
        private readonly Mock<UpdateDataValidator> _mockValidator = new Mock<UpdateDataValidator>();

        private Task TestTask { get; set; } = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default, default, default, default, default);

        public UpdateDataHandlerTests()
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
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(
                validator => validator.ValidateAndThrow(It.IsAny<UpdateData>())
                );
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_Command_Updates_Data()
        {
            //Arrange
            var command = new UpdateData(TestTask.TaskId, "{\"name\":\"asd\"}", Guid.NewGuid());
            var updateTaskHandler = new UpdateDataHandler(_mockRepository.Object, _mockValidator.Object); 

            //Act
            var task = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(_ => _.Update(
                It.Is<Task>(t => t.Data == command.Data && t.TaskId == command.TaskId)), Times.Once);
            _mockRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()));
            _mockRepository.VerifyNoOtherCalls();

            Assert.Equal(TestTask, task);
            Assert.Equal(task.Data, command.Data);
        }
    }
}
