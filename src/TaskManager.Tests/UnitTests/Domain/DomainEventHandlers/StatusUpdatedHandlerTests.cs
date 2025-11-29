using Moq;
using System;
using System.Threading;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using Xunit;
using Task = TaskManager.Domain.Models.Task;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class StatusUpdatedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));

            StatusUpdated notification = new StatusUpdated(new Task());

            StatusUpdatedHandler handler = new StatusUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            StatusUpdated notification = new StatusUpdated(null);

            StatusUpdatedHandler handler = new StatusUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            StatusUpdated notification = new StatusUpdated(null);

            StatusUpdatedHandler handler = new StatusUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
