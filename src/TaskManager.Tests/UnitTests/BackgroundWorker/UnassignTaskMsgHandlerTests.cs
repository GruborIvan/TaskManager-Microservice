using System;
using System.Threading;
using FiveDegrees.Messages.Task;
using MediatR;
using Moq;
using Xunit;
using TaskManager.Domain.Commands;
using TaskManager.BackgroundWorker.Handlers;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class UnassignTaskMsgHandlerTests
    {
        private static readonly ILogger<UnassignTaskMsgHandler> _mockLoggerObject =
            new Mock<ILogger<UnassignTaskMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand()
        {
            // Arrange
            var expectedCommand = new UnassignTask(Guid.NewGuid(), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UnassignTask>(It.IsAny<UnassignTaskMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UnassignTask>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new UnassignTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UnassignTaskMsg(Guid.NewGuid(), expectedCommand.TaskId);
            await handler.Handle(message);

            // Assert
            _mockMediator.Verify();
            _mockMapper.Verify();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV2_SendsCommand()
        {
            // Arrange
            var expectedCommand = new UnassignTask(Guid.NewGuid(), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<UnassignTask>(It.IsAny<UnassignTaskMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UnassignTask>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new UnassignTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UnassignTaskMsgV2(expectedCommand.TaskId);
            await handler.Handle(message);

            // Assert
            _mockMediator.Verify();
            _mockMapper.Verify();
        }
    }
}
