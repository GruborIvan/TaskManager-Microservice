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
    public class UpdateTaskDataMessageHandlerTests
    {
        private static readonly ILogger<UpdateTaskDataMsgHandler> _mockLoggerObject =
            new Mock<ILogger<UpdateTaskDataMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async Task ValidMessage_SendCommand()
        {
            // Arrange
            var expectedCommand = new UpdateData(Guid.NewGuid(), "{}", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task(
                taskId: Guid.NewGuid(),
                taskType: "",
                callback: default,
                fourEyeSubjectId: default,
                subject: default,
                source: default,
                comments: default,
                status: default,
                data: default,
                assignment: default,
                relations: default))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateData>(It.IsAny<UpdateTaskDataMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskDataMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskDataMsg
            (
                expectedCommand.TaskId,
                expectedCommand.Data,
                Guid.NewGuid()
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMediator.Verify();
            _mockMapper.Verify();
        }

        [Fact]
        public async Task ValidMessageV2_SendCommand()
        {
            // Arrange
            var expectedCommand = new UpdateData(Guid.NewGuid(), "{}", Guid.NewGuid());

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<UpdateData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskManager.Domain.Models.Task(
                taskId: Guid.NewGuid(),
                taskType: "",
                callback: default,
                fourEyeSubjectId: default,
                subject: default,
                source: default,
                comments: default,
                status: default,
                data: default,
                assignment: default,
                relations: default))
                .Verifiable();

            _mockMapper.Setup(mapper => mapper.Map<UpdateData>(It.IsAny<UpdateTaskDataMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            var updateTaskMessageHandler = new UpdateTaskDataMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new UpdateTaskDataMsgV2
            (
                expectedCommand.TaskId,
                expectedCommand.Data
            );
            await updateTaskMessageHandler.Handle(message);

            // Assert
            _mockMediator.Verify();
            _mockMapper.Verify();
        }
    }
}