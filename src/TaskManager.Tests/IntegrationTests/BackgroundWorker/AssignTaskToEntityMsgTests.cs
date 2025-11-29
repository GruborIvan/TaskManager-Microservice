using System;
using System.Linq;
using System.Threading;
using TaskManager.Infrastructure.Models;
using Xunit;
using FiveDegrees.Messages.Task;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Domain.IntegrationEvents;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class AssignTaskToEntityMsgTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;

        private readonly Guid _taskId = Guid.NewGuid();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public AssignTaskToEntityMsgTests()
        {
            RegisterMockHttpClient();
            StartHost();

            var expectedTask = new TaskDbo
            {
                TaskId = _taskId,
                Callback = "https://www.test.com",
                Data = "{}",
                SourceId = Guid.NewGuid().ToString(),
                Status = "active",
                TaskType = "Object",
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
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsg_Adds_Assignment()
        {
            // Arrange
            var message = new AssignTaskToEntityMsg(Guid.NewGuid(), _taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.Equal(message.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(message.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
            
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Adds_Assignment()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new AssignTaskToEntityMsgV2(_taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.Equal(message.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(message.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Headers_In_Uppercase_Adds_Assignment()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var message = new AssignTaskToEntityMsgV2(_taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.Equal(message.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(message.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new AssignTaskToEntityMsgV2(_taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<AssignTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service =>
                service.SendAsync(
                    It.Is<AssignTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsgV2", errorMessageBody.GetType().Name);
            Assert.Equal(_taskId, ((AssignTaskToEntityMsgV2)errorMessageBody).TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var message = new AssignTaskToEntityMsgV2(_taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<AssignTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<AssignTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsgV2", errorMessageBody.GetType().Name);
            Assert.Equal(_taskId, ((AssignTaskToEntityMsgV2)errorMessageBody).TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsg_Updates_Task_With_Existing_Assignment()
        {   
            // Arrange
            var message = new AssignTaskToEntityMsg(Guid.NewGuid(), _taskId, Guid.NewGuid(), AssignmentType.Unassigned);

            // Act
            await Subscribe<AssignTaskToEntityMsg>();
            await Publish(message, _headers);
            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.Equal(message.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(message.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Updates_Task_With_Existing_Assignment()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new AssignTaskToEntityMsgV2(_taskId, Guid.NewGuid(), AssignmentType.Unassigned);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.Equal(message.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(message.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_AssignTaskToEntityMsgs_Doesnt_Assign_Task_To_Entity(AssignTaskToEntityMsg msg)
        {
            // Act
            await Subscribe<AssignTaskToEntityMsg>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Assignment: '' must not be empty.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsg", errorMessageBody.GetType().Name);
            Assert.NotEqual(_taskId, ((AssignTaskToEntityMsg)errorMessageBody).TaskId);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new AssignTaskToEntityMsg[]
                {
                    new AssignTaskToEntityMsg(Guid.NewGuid(), Guid.NewGuid(), null, AssignmentType.User)
                };
                yield return new AssignTaskToEntityMsg[]
                {
                    new AssignTaskToEntityMsg(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, AssignmentType.User)
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidV2Messages))]
        public async System.Threading.Tasks.Task Invalid_AssignTaskToEntityMsgsV2_Doesnt_Assign_Task_To_Entity(AssignTaskToEntityMsgV2 msg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.SingleOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Assignment: '' must not be empty.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsgV2", errorMessageBody.GetType().Name);
            Assert.NotEqual(_taskId, ((AssignTaskToEntityMsgV2)errorMessageBody).TaskId);
        }

        public static IEnumerable<object[]> InvalidV2Messages
        {
            get
            {
                yield return new AssignTaskToEntityMsgV2[]
                {
                    new AssignTaskToEntityMsgV2(Guid.NewGuid(), null, AssignmentType.User)
                };
                yield return new AssignTaskToEntityMsgV2[]
                {
                    new AssignTaskToEntityMsgV2(Guid.NewGuid(), Guid.Empty, AssignmentType.User)
                };
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsg_Non_Existing_TaskId_Doesnt_Add_Assignment()
        {
            // Arrange
            var nonExistentTaskId = Guid.NewGuid();
            var message = new AssignTaskToEntityMsg(Guid.NewGuid(), nonExistentTaskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {nonExistentTaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsg", errorMessageBody.GetType().Name);
            Assert.NotEqual(_taskId, ((AssignTaskToEntityMsg)errorMessageBody).TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_AssignTaskToEntityMsgV2_Non_Existing_TaskId_Doesnt_Add_Assignment()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var nonExistentTaskId = Guid.NewGuid();
            var message = new AssignTaskToEntityMsgV2(nonExistentTaskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            await Subscribe<AssignTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Single();

            // Assert
            Assert.NotNull(task);
            Assert.Null(task.AssignedToEntityId);
            Assert.Null(task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<AssignTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {message.TaskId} not found.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("AssignTaskToEntityMsgV2", errorMessageBody.GetType().Name);
            Assert.NotEqual(_taskId, ((AssignTaskToEntityMsgV2)errorMessageBody).TaskId);
        }
    }
}
