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
    public class RelateTaskToEntityMsgHandlerTests
    {
        private static readonly ILogger<RelateTaskToEntityMsgHandler> _mockLoggerObject =
            new Mock<ILogger<RelateTaskToEntityMsgHandler>>().Object;

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<IMediator> _mockMediator = new Mock<IMediator>();
        private readonly Mock<IContextAccessor> _mockContextAccessor = new Mock<IContextAccessor>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        [Fact]
        public async System.Threading.Tasks.Task ValidMessage_SendsCommand()
        {
            //Arrange
            var expectedCommand = new RelateTaskToEntity(Guid.NewGuid().ToString(), "type", Guid.NewGuid(), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<RelateTaskToEntity>(It.IsAny<RelateTaskToEntityMsg>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<RelateTaskToEntity>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new RelateTaskToEntityMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            //Act
            var message = new RelateTaskToEntityMsg
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                Guid.NewGuid(),
                "Person"
            );
            await handler.Handle(message);

            //Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV2_SendsCommand()
        {
            // Arrange
            var expectedCommand = new RelateTaskToEntity(Guid.NewGuid().ToString(), "type", Guid.NewGuid(), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<RelateTaskToEntity>(It.IsAny<RelateTaskToEntityMsgV2>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<RelateTaskToEntity>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new RelateTaskToEntityMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new RelateTaskToEntityMsgV2
            (
                Guid.NewGuid(),
                expectedCommand.TaskId,
                Guid.NewGuid().ToString(),
                "Person"
            );
            await handler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidMessageV3_SendsCommand()
        {
            // Arrange
            var expectedCommand = new RelateTaskToEntity(Guid.NewGuid().ToString(), "type", Guid.NewGuid(), Guid.NewGuid());

            _mockMapper.Setup(mapper => mapper.Map<RelateTaskToEntity>(It.IsAny<RelateTaskToEntityMsgV3>()))
                    .Returns(expectedCommand);

            _mockMediator.Setup(mediator => mediator.Send(It.IsAny<RelateTaskToEntity>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var handler = new RelateTaskToEntityMsgHandler(_mockMediator.Object, _mockLoggerObject, _mockMapper.Object, _mockContextAccessor.Object, _busMock.Object);

            // Act
            var message = new RelateTaskToEntityMsgV3
            (
                expectedCommand.TaskId,
                Guid.NewGuid().ToString(),
                "Person"
            );
            await handler.Handle(message);

            // Assert
            _mockMapper.Verify();
            _mockMediator.Verify();
        }
    }
}
