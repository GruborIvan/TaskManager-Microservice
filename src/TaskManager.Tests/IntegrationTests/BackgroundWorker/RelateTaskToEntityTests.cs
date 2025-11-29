using FiveDegrees.Messages.Task;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Infrastructure.Models;
using Xunit;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class RelateTaskToEntityTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;

        private static readonly Guid _taskId = Guid.NewGuid();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public RelateTaskToEntityTests()
        {
            StartHost();

            var expectedTask = new TaskDbo
            {
                TaskId = _taskId,
                Callback = "https://www.test.com",
                Data = "{}",
                SourceId = Guid.NewGuid().ToString(),
                Status = "active",
                TaskType = "Object"
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
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsg_Relates_Entity_To_Task()
        {
            // Arrange
            var message = new RelateTaskToEntityMsg(Guid.NewGuid(), _taskId, Guid.NewGuid(), "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.Equal(message.EntityId.ToString(), task.TaskRelations.First().EntityId);
            Assert.Equal(message.EntityType.ToString(), task.TaskRelations.First().EntityType);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsgV2_Relates_Entity_To_Task()
        {
            // Arrange
            var message = new RelateTaskToEntityMsgV2(Guid.NewGuid(), _taskId, "123", "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.Equal(message.EntityId.ToString(), task.TaskRelations.First().EntityId);
            Assert.Equal(message.EntityType.ToString(), task.TaskRelations.First().EntityType);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsgV3_Relates_Entity_To_Task()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new RelateTaskToEntityMsgV3(_taskId, "123", "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsgV3>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.Equal(message.EntityId.ToString(), task.TaskRelations.First().EntityId);
            Assert.Equal(message.EntityType.ToString(), task.TaskRelations.First().EntityType);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsgV3_Headers_In_Uppercase_Relates_Entity_To_Task()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var message = new RelateTaskToEntityMsgV3(_taskId, "123", "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsgV3>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.Equal(message.EntityId.ToString(), task.TaskRelations.First().EntityId);
            Assert.Equal(message.EntityType.ToString(), task.TaskRelations.First().EntityType);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntitySucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsgV3_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new RelateTaskToEntityMsgV3(_taskId, "123", "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsgV3>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.NotNull(task);
            Assert.Empty(task.TaskRelations);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<RelateTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<RelateTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("RelateTaskToEntityMsgV3", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_RelateTaskToEntityMsgV3_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var message = new RelateTaskToEntityMsgV3(_taskId, "123", "Person");

            // Act
            await Subscribe<RelateTaskToEntityMsgV3>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.NotNull(task);
            Assert.Empty(task.TaskRelations);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<RelateTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<RelateTaskToEntityFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("RelateTaskToEntityMsgV3", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_RelateTaskToEntityMsgs_Doesnt_Relate_Entity_To_Task(RelateTaskToEntityMsg msg)
        {
            // Act
            await Subscribe<RelateTaskToEntityMsg>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.NotNull(task);
            Assert.Empty(task.TaskRelations);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("RelateTaskToEntityMsg", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_RelateTaskToEntityMsgsV2_Doesnt_Relate_Entity_To_Task(RelateTaskToEntityMsgV2 msg)
        {
            // Act
            await Subscribe<RelateTaskToEntityMsgV2>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.NotNull(task);
            Assert.Empty(task.TaskRelations);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("RelateTaskToEntityMsgV2", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV3))]
        public async System.Threading.Tasks.Task Invalid_RelateTaskToEntityMsgV3_Doesnt_Relate_Entity_To_Task(RelateTaskToEntityMsgV3 msg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<RelateTaskToEntityMsgV3>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(x => x.TaskRelations).Single();

            // Assert
            Assert.NotNull(task);
            Assert.Empty(task.TaskRelations);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<RelateTaskToEntityFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("RelateTaskToEntityMsgV3", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new RelateTaskToEntityMsg[]
                {
                    new RelateTaskToEntityMsg(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "Person")
                };
                yield return new RelateTaskToEntityMsg[]
                {
                     new RelateTaskToEntityMsg(Guid.NewGuid(), _taskId, Guid.Empty, string.Empty)
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new RelateTaskToEntityMsgV2[]
                {
                    new RelateTaskToEntityMsgV2(Guid.NewGuid(), Guid.Empty, "123", "Person")
                };
                yield return new RelateTaskToEntityMsgV2[]
                {
                    new RelateTaskToEntityMsgV2(Guid.NewGuid(), _taskId, string.Empty, "Person")
                };
                yield return new RelateTaskToEntityMsgV2[]
                {
                     new RelateTaskToEntityMsgV2(Guid.NewGuid(), _taskId, "123", string.Empty)
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV3
        {
            get
            {
                yield return new RelateTaskToEntityMsgV3[]
                {
                    new RelateTaskToEntityMsgV3(Guid.Empty, "123", "Person")
                };
                yield return new RelateTaskToEntityMsgV3[]
                {
                    new RelateTaskToEntityMsgV3(_taskId, string.Empty, "Person")
                };
                yield return new RelateTaskToEntityMsgV3[]
                {
                     new RelateTaskToEntityMsgV3(_taskId, "123", string.Empty)
                };
            }
        }
    }
}
