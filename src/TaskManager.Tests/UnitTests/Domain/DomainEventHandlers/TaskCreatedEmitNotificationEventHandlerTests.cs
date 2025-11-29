using Moq;
using System;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Domain.Interfaces;
using Xunit;
using Task = TaskManager.Domain.Models.Task;

namespace TaskManager.Tests.UnitTests.Domain.DomainEventHandlers
{
    public class TaskCreatedEmitNotificationEventHandlerTests
    {
        private readonly Mock<IEventNotificationService> _mockEventNotificationService = new Mock<IEventNotificationService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_Sends_Notification()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));

            TaskCreated notification = new TaskCreated(new Task());

            TaskCreatedEmitNotificationEventHandler handler = 
                new TaskCreatedEmitNotificationEventHandler(_mockEventNotificationService.Object);

            //Act
            await handler.Handle(notification, default);

            //Assert
            _mockEventNotificationService.VerifyAll();
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventService_Throws_Exception()
        {
            //Arrange
            _mockEventNotificationService.Setup(s => s.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskCreated notification = null;

            TaskCreatedEmitNotificationEventHandler handler = new TaskCreatedEmitNotificationEventHandler(_mockEventNotificationService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
