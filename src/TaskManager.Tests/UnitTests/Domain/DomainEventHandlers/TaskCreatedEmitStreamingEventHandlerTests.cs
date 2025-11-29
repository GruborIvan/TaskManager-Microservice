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
    public class TaskCreatedEmitStreamingEventHandlerTests
    {
        private readonly Mock<IEventStreamingService> _mockEventStreamingService = new Mock<IEventStreamingService>();

        [Fact]
        public async System.Threading.Tasks.Task Valid_Notification_EventService_Emits_Stream_Event()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()));

            TaskCreated notification = new TaskCreated(new Task());

            TaskCreatedEmitStreamEventHandler handler = 
                new TaskCreatedEmitStreamEventHandler(_mockEventStreamingService.Object);

            //Act
            await handler.Handle(notification, default);

            //Assert
            _mockEventStreamingService.VerifyAll();
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_Notification_EventService_Throws_Exception()
        {
            //Arrange
            _mockEventStreamingService.Setup(s => s.SendAsync(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new NullReferenceException())
                .Verifiable();

            TaskCreated notification = null;

            TaskCreatedEmitStreamEventHandler handler = new TaskCreatedEmitStreamEventHandler(_mockEventStreamingService.Object);

            //Act
            var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(notification, default));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}
