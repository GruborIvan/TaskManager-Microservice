using TaskManager.Infrastructure.Models;
using FiveDegrees.Messages.Task;
using System;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Moq;
using TaskManager.Domain.IntegrationEvents;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class StoreCommentMsgTests : TestFixture
    {
        private readonly string _expectedText = "CommentText";
        private readonly DateTime _expectedCreatedDate = DateTime.Now.AddSeconds(-2);
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private const int _waitTimeInMiliseconds = 10000;

        private static readonly Guid _taskId = Guid.NewGuid();

        public StoreCommentMsgTests()
        {
            RegisterMockHttpClient();
            StartHost();

            // Arrange
            var assignedEntityIdToRemove = Guid.NewGuid();
            var expectedTask = new TaskDbo
            {
                TaskId = _taskId,
                Callback = "https://www.test.com",
                Data = "{}",
                SourceId = Guid.NewGuid().ToString(),
                Status = "active",
                TaskType = "Object",
                AssignedToEntityId = assignedEntityIdToRemove,
                AssignmentType = AssignmentType.User.ToString()
            };

            using var context = Resolve<TasksDbContext>();
            context.Tasks.Add(expectedTask);
            context.SaveChanges();

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_StoreCommentMsg_Adds_Comment_To_Task()
        {
            // Arrange
            var storeCommentMessage = new StoreCommentMsg(Guid.NewGuid(), _taskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsg>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.Comments).Single();

            Assert.True(task.Comments.Count == 1);
            Assert.Equal(task.Comments.First().Text, _expectedText);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_StoreCommentMsgV2_Adds_Comment_To_Task()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var storeCommentMessage = new StoreCommentMsgV2(_taskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.Comments).Single();

            Assert.True(task.Comments.Count == 1);
            Assert.Equal(task.Comments.First().Text, _expectedText);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_StoreCommentMsgV2_Headers_In_Uppercase_Adds_Comment_To_Task()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var storeCommentMessage = new StoreCommentMsgV2(_taskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.Comments).Single();

            Assert.True(task.Comments.Count == 1);
            Assert.Equal(task.Comments.First().Text, _expectedText);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_StoreCommentMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var storeCommentMessage = new StoreCommentMsgV2(_taskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<StoreCommentFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<StoreCommentFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_StoreCommentMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var storeCommentMessage = new StoreCommentMsgV2(_taskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<StoreCommentFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<StoreCommentFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task StoreCommentMsg_WrongTaskId_Comment_Not_Saved()
        {
            // Arrange
            var nonExistingTaskId = Guid.NewGuid();
            var storeCommentMessage = new StoreCommentMsg(Guid.NewGuid(), nonExistingTaskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsg>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            
            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {nonExistingTaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task StoreCommentMsgV2_WrongTaskId_Comment_Not_Saved()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var nonExistingTaskId = Guid.NewGuid();
            var storeCommentMessage = new StoreCommentMsgV2(nonExistingTaskId, _expectedText, _expectedCreatedDate);

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(storeCommentMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {nonExistingTaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsgV2", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_StoreCommentMsgs_Doesnt_Store_Comment(StoreCommentMsg msg)
        {
            // Act
            await Subscribe<StoreCommentMsg>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsg", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_StoreCommentMsgV2_Doesnt_Store_Comment(StoreCommentMsgV2 msg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<StoreCommentMsgV2>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Comments.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<StoreCommentFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("StoreCommentMsgV2", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new StoreCommentMsg[]
                {
                    new StoreCommentMsg(Guid.NewGuid(), Guid.Empty, "Text", DateTime.UtcNow)
                };
                yield return new StoreCommentMsg[]
                {
                     new StoreCommentMsg(Guid.NewGuid(), _taskId, "", DateTime.UtcNow)
                };
                yield return new StoreCommentMsg[]
                {
                     new StoreCommentMsg(Guid.NewGuid(), _taskId, string.Empty, DateTime.UtcNow)
                };
                yield return new StoreCommentMsg[]
                {
                     new StoreCommentMsg(Guid.NewGuid(), _taskId, "Text", DateTime.Now.AddDays(30))
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new StoreCommentMsgV2[]
                {
                    new StoreCommentMsgV2(Guid.Empty, "Text", DateTime.UtcNow)
                };
                yield return new StoreCommentMsgV2[]
                {
                     new StoreCommentMsgV2(_taskId, "", DateTime.UtcNow)
                };
                yield return new StoreCommentMsgV2[]
                {
                     new StoreCommentMsgV2(_taskId, string.Empty, DateTime.UtcNow)
                };
                yield return new StoreCommentMsgV2[]
                {
                     new StoreCommentMsgV2(_taskId, "Text", DateTime.Now.AddDays(30))
                };
            }
        }
    }
}
