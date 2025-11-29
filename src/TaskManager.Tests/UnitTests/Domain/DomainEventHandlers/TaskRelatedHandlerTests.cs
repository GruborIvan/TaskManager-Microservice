using Moq;
using System;
using System.Threading;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;
using Relation = TaskManager.Domain.Models.Relation;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class TaskRelatedHandlerTests
    {
        readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();
        readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));

            TaskRelated notification = new TaskRelated(new Relation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), "entityType"));

            TaskRelatedHandler handler = new TaskRelatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

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
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskRelated notification = new TaskRelated(null);

            TaskRelatedHandler handler = new TaskRelatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventStreamingService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskRelated notification = new TaskRelated(null);

            TaskRelatedHandler handler = new TaskRelatedHandler(_mockEventNotificationService.Object, _mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
