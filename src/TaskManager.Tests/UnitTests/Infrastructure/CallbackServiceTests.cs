using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Services;
using Xunit;

namespace TaskManager.Tests.UnitTests.Infrastructure
{
    public class CallbackServiceTests
    {
        [Fact]
        public async void Callback_Posts_Task_To_CallbackUrl()
        {
            //Arrange
            var task = new Task(
                    Guid.NewGuid(), default,
                    new HttpCallback(new Uri("https://uri")), default, default, default, default, default, default, default, default, default, default);
            
            var _loggerMock = new Mock<ILogger<CallbackService>>();
            var mockMessageHandler = new Mock<HttpMessageHandler>();
                mockMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Verifiable();

            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockMessageHandler.Object));

            var taskService = new CallbackService(mockFactory.Object, _loggerMock.Object);

            //Act
            await taskService.Callback(task.Callback, task);

            //Assert
            mockMessageHandler.VerifyAll();
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Unsuccessful callback for task {task.TaskId}, status code: BadRequest")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }

        [Fact]
        public async void Callback_Post_Callback_Unsuccessful_Log()
        {
            //Arrange
            var taskId = Guid.NewGuid();
            var task = new Task(
                taskId, default,
                new HttpCallback(new Uri("https://uri")), default, default, default, default, default, default, default, default, default, default);
            var _loggerMock = new Mock<ILogger<CallbackService>>();
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            mockMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                })
                .Verifiable();
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockMessageHandler.Object));
            var taskService = new CallbackService(mockFactory.Object, _loggerMock.Object);
            //Act
            await taskService.Callback(task.Callback, task);
            //Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Unsuccessful callback for task {task.TaskId}, status code: BadRequest")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
