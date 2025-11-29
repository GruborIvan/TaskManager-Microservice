using Moq;
using System;
using System.Threading;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class AssignTaskToEntityFailedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()));

            AssignTaskToEntityFailed notification = new AssignTaskToEntityFailed(Guid.NewGuid(), new ErrorData("error message", "errorCode"));

            AssignTaskToEntityFailedHandler handler = new AssignTaskToEntityFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            await handler.Handle(notification, default);

            //Assert
            _mockEventNotificationService.VerifyAll();
            _mockEventStreamingService.VerifyAll();
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventNotificationService_Throws_Exception()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()))
                .Throws(new Exception())
                .Verifiable();

            AssignTaskToEntityFailed notification = new AssignTaskToEntityFailed(Guid.Empty, new ErrorData("error message", "errorCode"));

            AssignTaskToEntityFailedHandler handler = new AssignTaskToEntityFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();

            AssignTaskToEntityFailed notification = new AssignTaskToEntityFailed(Guid.Empty, new ErrorData("error message", "errorCode"));

            AssignTaskToEntityFailedHandler handler = new AssignTaskToEntityFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<Exception>(exception);
        }
    }
}
