using System;
using System.Threading;
using Moq;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;
using Task = TaskManager.Domain.Models.Task;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class TaskUpdatedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync<object>(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<CancellationToken>()));

            TaskUpdated notification = new TaskUpdated(new Task());

            TaskUpdatedHandler handler = new TaskUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskUpdated notification = new TaskUpdated(null);

            TaskUpdatedHandler handler = new TaskUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskUpdated notification = new TaskUpdated(null);

            TaskUpdatedHandler handler = new TaskUpdatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
