using System;
using System.Collections.Generic;
using System.Linq;
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
using Rebus.Retry.Simple;
using Rebus.TestHelpers;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class AssignTaskToEntityMsgHandlerTests
    {
        private static readonly ILogger<AssignTaskToEntityMsgHandler> _mockLoggerObject =
            new Mock<ILogger<AssignTaskToEntityMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();
        

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var expectedCommand = new AssignTaskToEntity(taskId, new Assignment(Guid.NewGuid(), "User", taskId), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<AssignTaskToEntity>(It.IsAny<AssignTaskToEntityMsg>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<AssignTaskToEntity>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new AssignTaskToEntityMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var assignToEntityId = expectedCommand.Assignment.AssignedToEntityId;
            var assignmentType = Enum.Parse<AssignmentType>(expectedCommand.Assignment.Type);

            var message = new AssignTaskToEntityMsg
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                assignToEntityId,
                assignmentType
            );

            await handler.Handle(message);

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
            var taskId = Guid.NewGuid();
            var expectedCommand = new AssignTaskToEntity(taskId, new Assignment(Guid.NewGuid(), "User", taskId), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<AssignTaskToEntity>(It.IsAny<AssignTaskToEntityMsgV2>()))
                .Returns(expectedCommand)
                .Verifiable();

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<AssignTaskToEntity>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new AssignTaskToEntityMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var assignToEntityId = expectedCommand.Assignment.AssignedToEntityId;
            var assignmentType = Enum.Parse<AssignmentType>(expectedCommand.Assignment.Type);

            var message = new AssignTaskToEntityMsgV2
            (
                expectedCommand.TaskId,
                assignToEntityId,
                assignmentType
            );

            await handler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMapper.VerifyNoOtherCalls();

            _mockMediator.Verify();
            _mockMediator.VerifyNoOtherCalls();
        }
    }
}
