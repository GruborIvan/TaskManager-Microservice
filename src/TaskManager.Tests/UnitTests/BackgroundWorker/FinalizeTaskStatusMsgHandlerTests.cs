using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Rebus.Bus;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class FinalizeTaskStatusMsgHandlerTests
    {
        private static readonly ILogger<FinalizeTaskStatusMsgHandler> _mockLoggerObject =
            new Mock<ILogger<FinalizeTaskStatusMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand_FinalStatus()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var expectedCommand = new FinalizeStatus(taskId, "taskStatus", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<FinalizeStatus>(It.IsAny<FinalizeTaskStatusMsg>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<FinalizeStatus>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new FinalizeTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var status = expectedCommand.Status;

            var message = new FinalizeTaskStatusMsg(taskId, status, true, Guid.NewGuid());

            await handler.Handle(message);

            // Assert
            _mockMapper.Verify(mapper => mapper.Map<FinalizeStatus>(message));
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify(mediator => mediator.Send(expectedCommand, It.IsAny<CancellationToken>()));
            _mockMediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV2_SendsCommand_Finalizes_Status()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var expectedCommand = new FinalizeStatus(taskId, "taskStatus", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<FinalizeStatus>(It.IsAny<FinalizeTaskStatusMsgV2>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<FinalizeStatus>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new FinalizeTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var status = expectedCommand.Status;

            var message = new FinalizeTaskStatusMsgV2(taskId, status, true);

            await handler.Handle(message);

            // Assert
            _mockMapper.Verify(mapper => mapper.Map<FinalizeStatus>(message));
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify(mediator => mediator.Send(expectedCommand, It.IsAny<CancellationToken>()));
            _mockMediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand_IsNotFinalStatus()
        {
            //Arrange
            var taskId = Guid.NewGuid();
            var expectedCommand = new FinalizeStatus(taskId, "taskStatus", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<FinalizeStatus>(It.IsAny<FinalizeTaskStatusMsg>()))
                .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<FinalizeStatus>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new FinalizeTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var status = expectedCommand.Status;

            var message = new FinalizeTaskStatusMsg
            (taskId, status, false, Guid.NewGuid());

            await handler.Handle(message);

            //Assert
            _mockMapper.Verify(mapper => mapper.Map<FinalizeStatus>(message));
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify(mediator => mediator.Send(expectedCommand, It.IsAny<CancellationToken>()));
            _mockMediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV2_SendsCommand_IsNotFinalStatus()
        {
            //Arrange
            var taskId = Guid.NewGuid();
            var expectedCommand = new FinalizeStatus(taskId, "taskStatus", Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<FinalizeStatus>(It.IsAny<FinalizeTaskStatusMsgV2>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<FinalizeStatus>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new FinalizeTaskStatusMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var status = expectedCommand.Status;

            var message = new FinalizeTaskStatusMsgV2(taskId, status, false);

            await handler.Handle(message);

            //Assert
            _mockMapper.Verify(mapper => mapper.Map<FinalizeStatus>(message));
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify(mediator => mediator.Send(expectedCommand, It.IsAny<CancellationToken>()));
            _mockMediator.VerifyNoOtherCalls();
        }
    }
}
