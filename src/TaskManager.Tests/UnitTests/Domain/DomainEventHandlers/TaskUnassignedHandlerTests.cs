using Moq;
using System;
using System.Threading;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class TaskUnassignedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<CancellationToken>()));

            TaskUnassigned notification = new TaskUnassigned(new Assignment(Guid.NewGuid(), "type", Guid.NewGuid()));

            TaskUnassignedHandler handler = new TaskUnassignedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskUnassigned notification = new TaskUnassigned(null);

            TaskUnassignedHandler handler = new TaskUnassignedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskUnassigned notification = new TaskUnassigned(null);

            TaskUnassignedHandler handler = new TaskUnassignedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
