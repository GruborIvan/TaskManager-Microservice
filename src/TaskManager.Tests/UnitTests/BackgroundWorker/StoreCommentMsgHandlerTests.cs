using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Bus;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class StoreCommentMsgHandlerTests
    {
        private static readonly ILogger<StoreCommentMsgHandler> _mockLoggerObject =
            new Mock<ILogger<StoreCommentMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async Task ValidMessage_SendCommand()
        {
            // Arrange
            var expectedCommand = new StoreComment(Guid.NewGuid(), "expectedText", Guid.NewGuid(), DateTime.UtcNow);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<StoreComment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Comment(Guid.NewGuid(), expectedCommand.TaskId, expectedCommand.Text))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<StoreComment>(It.IsAny<StoreCommentMsg>()))
                .Returns(expectedCommand);
            var updateTaskMessageHandler = new StoreCommentMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new StoreCommentMsg
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                expectedCommand.Text,
                DateTime.UtcNow
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMediator.VerifyAll();
            _mockMapper.VerifyAll();
        }

        [Fact]
        public async Task ValidMessageV2_SendCommand()
        {
            // Arrange
            var expectedCommand = new StoreComment(Guid.NewGuid(), "expectedText", Guid.NewGuid(), DateTime.UtcNow);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<StoreComment>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Comment(Guid.NewGuid(), expectedCommand.TaskId, expectedCommand.Text))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<StoreComment>(It.IsAny<StoreCommentMsgV2>()))
                .Returns(expectedCommand);
            var updateTaskMessageHandler = new StoreCommentMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new StoreCommentMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Text,
                DateTime.UtcNow
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMediator.VerifyAll();
            _mockMapper.VerifyAll();
        }
    }
}
