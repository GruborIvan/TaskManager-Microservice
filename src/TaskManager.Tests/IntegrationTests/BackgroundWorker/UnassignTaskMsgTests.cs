using System;
using System.Linq;
using TaskManager.Infrastructure.Models;
using Xunit;
using FiveDegrees.Messages.Task;
using System.Collections.Generic;
using Moq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Domain.IntegrationEvents;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class UnassignTaskMsgTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;

        private readonly Guid _taskId = Guid.NewGuid();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public UnassignTaskMsgTests()
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
                AssignmentType = AssignmentType.User.ToString(),
                CreatedById = Guid.NewGuid(),
                ChangedBy = Guid.Empty
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
        public async System.Threading.Tasks.Task Valid_UnassignTaskMsg_Unassigns()
        {
            // Arrange
            var message = new UnassignTaskMsg(Guid.NewGuid(), _taskId);

            // Act
            await Subscribe<UnassignTaskMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.True(string.IsNullOrEmpty(task.AssignmentType) 
                || task.AssignmentType == AssignmentType.Unassigned.ToString());
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UnassignTaskMsgV2_Unassigns()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new UnassignTaskMsgV2(_taskId);

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.True(string.IsNullOrEmpty(task.AssignmentType)
                || task.AssignmentType == AssignmentType.Unassigned.ToString());
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UnassignTaskMsgV2_Headers_In_Uppercase_Unassigns()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var message = new UnassignTaskMsgV2(_taskId);

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            // Assert
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.True(string.IsNullOrEmpty(task.AssignmentType)
                || task.AssignmentType == AssignmentType.Unassigned.ToString());
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UnassignTaskMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new UnassignTaskMsgV2(_taskId);

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.NotNull(task.AssignedToEntityId);
            Assert.Equal("User", task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UnassignTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UnassignTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UnassignTaskMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var message = new UnassignTaskMsgV2(_taskId);

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.NotNull(task.AssignedToEntityId);
            Assert.Equal("User", task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UnassignTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UnassignTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task UnassignTaskMsg_Wrong_TaskId_Unassign_Fails()
        {
            // Arrange
            var nonExistentTaskId = Guid.NewGuid();
            var message = new UnassignTaskMsg(Guid.NewGuid(), nonExistentTaskId);

            // Act
            await Subscribe<UnassignTaskMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            // Assert
            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {nonExistentTaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task UnassignTaskMsgV2_Wrong_TaskId_Unassign_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var nonExistentTaskId = Guid.NewGuid();
            var message = new UnassignTaskMsgV2(nonExistentTaskId);

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            // Assert
            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {nonExistentTaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMsgs))]
        public async System.Threading.Tasks.Task Invalid_UnassignTaskMsgs_Unassign_Fails(UnassignTaskMsg invalidMsg)
        {
            // Act
            await Subscribe<UnassignTaskMsg>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.NotNull(task.AssignedToEntityId);
            Assert.Equal("User", task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsg", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMsgsV2))]
        public async System.Threading.Tasks.Task Invalid_UnassignTaskMsgV2_Unassign_Fails(UnassignTaskMsgV2 invalidMsg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<UnassignTaskMsgV2>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.NotNull(task.AssignedToEntityId);
            Assert.Equal("User", task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UnassignTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UnassignTaskMsgV2", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMsgs
        {
            get
            {
                yield return new UnassignTaskMsg[]
                {
                    new UnassignTaskMsg(Guid.Empty, Guid.Empty)
                };
                yield return new UnassignTaskMsg[]
                {
                    new UnassignTaskMsg(Guid.Empty, Guid.NewGuid())
                };
                yield return new UnassignTaskMsg[]
                {
                    new UnassignTaskMsg(Guid.NewGuid(), Guid.Empty)
                };
            }
        }

        public static IEnumerable<object[]> InvalidMsgsV2
        {
            get
            {
                yield return new UnassignTaskMsgV2[]
                {
                    new UnassignTaskMsgV2(Guid.Empty)
                };
            }
        }
    }
}
