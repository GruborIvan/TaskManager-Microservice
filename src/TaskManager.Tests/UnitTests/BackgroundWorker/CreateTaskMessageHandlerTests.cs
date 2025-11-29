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
using TaskManager.Domain.Models;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class CreateTaskMessageHandlerTests
    {
        private static readonly ILogger<CreateTaskMsgHandler> _mockLoggerObject =
            new Mock<ILogger<CreateTaskMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand()
        {
            // Arrange
            var expectedCommand = new SaveTask("asdasd", Guid.NewGuid(), "{}", "http://www.test.com", "ApproveCreate", "New", new Assignment(Guid.NewGuid(), "User", Guid.Empty), default, default, default, default, default, default);

            _mockMapper.Setup(mapper => mapper.Map<SaveTask>(It.IsAny<CreateTaskMsg>()))
                    .Returns(expectedCommand)
                    .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Task(
                    Guid.NewGuid(),
                    default, default, default, default, default, default, default, default, default, default, default, default))
                .Verifiable();

            var createTaskMessageHandler = new CreateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);
            // Act
            var assignToEntityId = expectedCommand.Assignment.AssignedToEntityId;
            var assignmentType = Enum.Parse<AssignmentType>(expectedCommand.Assignment.Type);

            var message = new CreateTaskMsg
            (
                Guid.NewGuid(),
                expectedCommand.SourceId,
                expectedCommand.SourceName,
                expectedCommand.Subject,
                expectedCommand.Data,
                expectedCommand.Callback,
                "TaskType.ApproveCreate",
                expectedCommand.Status,
                assignToEntityId,
                assignmentType,
                default, 
                default
            );
            await createTaskMessageHandler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify();
            _mockMediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV2_SendsCommand()
        {
            // Arrange
            var expectedCommand = new SaveTask("asdasd", Guid.NewGuid(), "{}", "http://www.test.com", "ApproveCreate", "New", new Assignment(Guid.NewGuid(), "User", Guid.Empty), default, default, default, default, default, default);

            _mockMapper.Setup(mapper => mapper.Map<SaveTask>(It.IsAny<CreateTaskMsgV2>()))
                    .Returns(expectedCommand)
                    .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<SaveTask>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Task(
                    Guid.NewGuid(),
                    default, default, default, default, default, default, default, default, default, default, default, default))
                .Verifiable();

            var createTaskMessageHandler = new CreateTaskMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);
            
            // Act
            var assignToEntityId = expectedCommand.Assignment.AssignedToEntityId;
            var assignmentType = Enum.Parse<AssignmentType>(expectedCommand.Assignment.Type);

            var message = new CreateTaskMsgV2
            (
                expectedCommand.SourceId,
                expectedCommand.SourceName,
                expectedCommand.Subject,
                expectedCommand.Data,
                expectedCommand.Callback,
                "TaskType.ApproveCreate",
                expectedCommand.Status,
                assignToEntityId,
                assignmentType,
                default,
                default
            );
            await createTaskMessageHandler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify();
            _mockMediator.VerifyNoOtherCalls();
        }
    }
}
