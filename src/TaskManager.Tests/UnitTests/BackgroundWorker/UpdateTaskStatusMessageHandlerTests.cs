using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class UpdateTaskStatusMessageHandlerTests
    {
        private static readonly ILogger<UpdateTaskStatusMsgHandler> _mockLoggerObject =
            new Mock<ILogger<UpdateTaskStatusMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async Task ValidMessage_SendCommand()
        {
            // Arrange
            var expectedCommand = new UpdateStatus(Guid.NewGuid(), "status", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateStatus>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task(
                    expectedCommand.TaskId,
                    default, default, default, default, default, default, default, default, default, default,default,default))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateStatus>(It.IsAny<UpdateTaskStatusMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskStatusMsg
            (
                expectedCommand.TaskId,
                expectedCommand.Status,
                expectedCommand.FinalState,
                Guid.NewGuid()
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessageV2_SendCommand()
        {
            // Arrange
            var expectedCommand = new UpdateStatus(Guid.NewGuid(), "status", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateStatus>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task(
                    expectedCommand.TaskId,
                    default, default, default, default, default, default, default, default, default, default,default, default))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateStatus>(It.IsAny<UpdateTaskStatusMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskStatusMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Status,
                expectedCommand.FinalState
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessage_Mediator_Throws_Exception()
        {
            // Arrange
            var expectedCommand = new UpdateStatus(Guid.NewGuid(), "status", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UpdateStatus>(It.IsAny<UpdateTaskStatusMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateStatus>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("error message"))
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Publish(It.Is<UpdateStatusFailed>(x => x.Error.Message.Contains("error message")), It.IsAny<CancellationToken>()));

            var handler = new UpdateTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskStatusMsg
            (
                expectedCommand.TaskId,
                expectedCommand.Status,
                expectedCommand.FinalState,
                Guid.NewGuid()
            );

            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            // Assert
            Assert.IsType<Exception>(exception);

            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessageV2_Mediator_Throws_Exception()
        {
            // Arrange
            var expectedCommand = new UpdateStatus(Guid.NewGuid(), "status", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UpdateStatus>(It.IsAny<UpdateTaskStatusMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateStatus>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("error message"))
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Publish(It.Is<UpdateStatusFailed>(x => x.Error.Message.Contains("error message")), It.IsAny<CancellationToken>()));

            var handler = new UpdateTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskStatusMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Status,
                expectedCommand.FinalState
            );

            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            // Assert
            Assert.IsType<Exception>(exception);

            _mockMapper.Verify();
            _mockMediator.Verify();
        }
    }
}