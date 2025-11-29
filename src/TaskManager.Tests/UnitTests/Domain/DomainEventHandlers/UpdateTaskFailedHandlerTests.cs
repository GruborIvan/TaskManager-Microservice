using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Moq;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class UpdateTaskFailedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<CancellationToken>()));

            UpdateTaskFailed notification = new UpdateTaskFailed(Guid.NewGuid(), new ErrorData("error message", "errorCode"));

            UpdateTaskFailedHandler handler = new UpdateTaskFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<string>()))
                .Throws(new Exception())
                .Verifiable();

            UpdateTaskFailed notification = new UpdateTaskFailed(Guid.NewGuid(), new ErrorData("error message", "errorCode"));

            UpdateTaskFailedHandler handler = new UpdateTaskFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();

            UpdateTaskFailed notification = new UpdateTaskFailed(Guid.NewGuid(), new ErrorData("error message", "errorCode"));

            UpdateTaskFailedHandler handler = new UpdateTaskFailedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<Exception>(exception);
        }
    }
}
