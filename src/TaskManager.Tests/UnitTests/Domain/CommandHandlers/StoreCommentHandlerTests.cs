using Moq;
using System;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class StoreCommentHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository = new Mock<ITaskRepository>();
        private readonly Mock<ICommentRepository> _mockCommentRepository = new Mock<ICommentRepository>();
        private readonly Mock<StoreCommentValidator> _mockValidator = new Mock<StoreCommentValidator>();

        private static Task TestTask { get; set; } = new Task(Guid.NewGuid(), default, default, default, default, default, default, default, default,
            new Assignment(null, "n", default), default);
        private static Comment TestComment { get; set; } = new Comment(Guid.NewGuid(), TestTask.TaskId, "comment", Guid.NewGuid(), DateTime.UtcNow);

        public StoreCommentHandlerTests()
        {
            _mockTaskRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestTask)
               .Verifiable();

            _mockCommentRepository.Setup(
                repository => repository.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestComment)
                .Verifiable();

            _mockCommentRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(
                validator => validator.ValidateAndThrow(It.IsAny<StoreComment>()))
                .Verifiable();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_Command_Stores_Comment()
        {
            //Arrange
            var command = new StoreComment(TestTask.TaskId, TestComment.Text, TestComment.CreatedBy, TestComment.CreatedDate);
            var updateTaskHandler = new StoreCommentHandler(_mockTaskRepository.Object, _mockCommentRepository.Object, _mockValidator.Object);

            //Act
            var comment = await updateTaskHandler.Handle(command, default);

            //Assert
            _mockTaskRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockCommentRepository.Verify(_ => _.AddAsync(It.Is<Comment>(t => t.TaskId == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockCommentRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()));

            _mockTaskRepository.VerifyNoOtherCalls();
            _mockCommentRepository.VerifyNoOtherCalls();

            Assert.Equal(command.TaskId, comment.TaskId);
            Assert.Equal(command.Text, comment.Text);
        }
    }
}
