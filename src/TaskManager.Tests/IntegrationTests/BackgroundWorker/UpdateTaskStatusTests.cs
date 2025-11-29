using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FiveDegrees.Messages.Task;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Infrastructure.Models;
using Xunit;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class UpdateTaskStatusTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;

        private static readonly Guid _taskId = Guid.NewGuid();
        private static readonly Guid _task2Id = Guid.NewGuid();
        private static readonly Guid _task3Id = Guid.NewGuid();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public UpdateTaskStatusTests()
        {
            RegisterMockHttpClient();
            StartHost();

            var expectedTasks = new List<TaskDbo>()
            {
                new TaskDbo
                {
                    TaskId = _taskId,
                    Callback = "https://www.test.com",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    Status = "active",
                    TaskType = "Object",
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.Empty,
                    FinalState = false
                },
                new TaskDbo
                {
                    TaskId = _task2Id,
                    Callback = "https://www.test2.com",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    Status = "resolved",
                    TaskType = "Object",
                    FinalState = true,
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.Empty
                },
                new TaskDbo
                {
                    TaskId = _task3Id,
                    Callback = "https://www.test3.com",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    Status = "active",
                    TaskType = "Object",
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.Empty,
                    TaskRelations = new List<TaskRelationDbo>()
                    {
                        new TaskRelationDbo
                        {
                            RelationId = Guid.NewGuid(),
                            EntityId = Guid.NewGuid().ToString(),
                            EntityType = "Person",
                            IsMain = true
                        },
                        new TaskRelationDbo
                        {
                            RelationId = Guid.NewGuid(),
                            EntityId = "123",
                            EntityType = "Loan",
                            IsMain = true
                        }
                    }
                },
            };

            using var context = Resolve<TasksDbContext>();
            context.Tasks.AddRange(expectedTasks);
            context.SaveChanges();

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsg_Updates_Task_Status()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskStatusMsg
            (
                _taskId,
                "sutatS",
                false,
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskStatusMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Updates_Task_Status()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _taskId,
                "sutatS",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Headers_In_Uppercase_Updates_Task_Status()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _taskId,
                "sutatS",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _taskId,
                "sutatS",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _taskId,
                "sutatS",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidTask_UpdatesTaskStatus_Emits_Only_UpdateTaskStatusSucceededEventV2_Event()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskStatusMsg
            (
                _task3Id,
                "sutatS",
                false,
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskStatusMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Emits_Only_UpdateTaskStatusSucceededEventV2_Event()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _task3Id,
                "sutatS",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsg_Task_Already_In_Final_State_Doesnt_Update_Status()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskStatusMsg
            (
                _task2Id,
                "status",
                false,
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskStatusMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {_task2Id} is finalized and cannot be modified.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskStatusMsgV2_Task_Already_In_Final_State_Doesnt_Update_Status()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _task2Id,
                "status",
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {_task2Id} is finalized and cannot be modified.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskStatusMsg_Update_Status_Fails()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskStatusMsg
            (
                _taskId,
                null,
                false,
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskStatusMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("'Status' must not be empty.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskStatusMsgV2_Update_Status_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskStatusMsgV2
            (
                _taskId,
                null,
                false
            );

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("'Status' must not be empty.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsgV2", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskStatusMsgs_Update_Status_Fails(UpdateTaskStatusMsg invalidMsg)
        {
            // Act
            await Subscribe<UpdateTaskStatusMsg>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault(x => x.TaskId == _taskId);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Status, invalidMsg.Status);
            Assert.NotEqual(task.FinalState, invalidMsg.FinalStatus);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsg", errorMessageBody.GetType().Name);
        }


        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskStatusMsgsV2_Update_Status_Fails(UpdateTaskStatusMsgV2 invalidMsg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<UpdateTaskStatusMsgV2>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault(x => x.TaskId == _taskId);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Status, invalidMsg.Status);
            Assert.NotEqual(task.FinalState, invalidMsg.FinalStatus);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskStatusMsgV2", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new UpdateTaskStatusMsg[]
                {
                   new UpdateTaskStatusMsg
                    (
                        _taskId,
                        null,
                        true,
                        Guid.NewGuid()
                    )
                };
                yield return new UpdateTaskStatusMsg[]
                {
                 new UpdateTaskStatusMsg
                    (
                        Guid.Empty,
                        "status",
                        true,
                        Guid.NewGuid()
                    )
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new UpdateTaskStatusMsgV2[]
                {
                   new UpdateTaskStatusMsgV2
                    (
                        _taskId,
                        null,
                        true
                    )
                };
                yield return new UpdateTaskStatusMsgV2[]
                {
                 new UpdateTaskStatusMsgV2
                    (
                        Guid.Empty,
                        "status",
                        true
                    )
                };
            }
        }
    }
}
