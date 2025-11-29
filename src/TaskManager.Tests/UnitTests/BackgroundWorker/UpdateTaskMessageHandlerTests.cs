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
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class UpdateTaskMessageHandlerTests
    {
        private static readonly ILogger<UpdateTaskMsgHandler> _mockLoggerObject =
            new Mock<ILogger<UpdateTaskMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async Task ValidMessage_SendCommand()
        {
            //Arrange
            var expectedCommand = new UpdateTask(Guid.NewGuid(), "{\"name\":\"asd\"}", "status", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateTask>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task())
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateTask>(It.IsAny<UpdateTaskMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var message = new UpdateTaskMsg
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                expectedCommand.Data,
                expectedCommand.FinalState,
                expectedCommand.Status
            );
            await updateTaskMessageHandler.Handle(message);

            //Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessageV2_SendCommand()
        {
            //Arrange
            var expectedCommand = new UpdateTaskV2(Guid.NewGuid(), "{\"name\":\"asd\"}", "subject", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateTaskV2>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task())
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateTaskV2>(It.IsAny<UpdateTaskMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var message = new UpdateTaskMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Data,
                expectedCommand.Subject
            );
            await updateTaskMessageHandler.Handle(message);

            //Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessage_Mediator_Throws_Exception()
        {
            //Arrange
            var expectedCommand = new UpdateTask(Guid.NewGuid(), "{\"name\":\"asd\"}", "status", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UpdateTask>(It.IsAny<UpdateTaskMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateTask>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();

            var handler = new UpdateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var message = new UpdateTaskMsg
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                expectedCommand.Data,
                expectedCommand.FinalState,
                expectedCommand.Status
            );
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            //Assert
            Assert.IsType<Exception>(exception);

            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async Task ValidMessageV2_Mediator_Throws_Exception()
        {
            //Arrange
            var expectedCommand = new UpdateTaskV2(Guid.NewGuid(), "{\"name\":\"asd\"}", "subject", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UpdateTaskV2>(It.IsAny<UpdateTaskMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateTaskV2>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();

            var handler = new UpdateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var message = new UpdateTaskMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Data,
                expectedCommand.Subject
            );
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            //Assert
            Assert.IsType<Exception>(exception);

            _mockMapper.Verify();
            _mockMediator.Verify();
        }
    }
}