using Moq;
using System;
using System.Threading;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;
using Task = TaskManager.Domain.Models.Task;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class DataUpdatedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<CancellationToken>()));

            DataUpdated notification = new DataUpdated(new Task());

            DataUpdatedHandler handler = new DataUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            DataUpdated notification = new DataUpdated(null);

            DataUpdatedHandler handler = new DataUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            DataUpdated notification = new DataUpdated(null);

            DataUpdatedHandler handler = new DataUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
