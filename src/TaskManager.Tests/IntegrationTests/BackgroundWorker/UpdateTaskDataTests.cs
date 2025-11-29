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
    public class UpdateTaskDataTests : TestFixture
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

        public UpdateTaskDataTests()
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
                    ChangedBy = Guid.Empty
                },
                new TaskDbo
                {
                    TaskId = _task2Id,
                    Callback = "https://www.test2.com",
                    Data = "{\"name\": \"John Doe\"}",
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
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsg_Updates_Task_Data()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskDataMsg
            (
                _taskId,
                "{ \"body\":\"body\"}",
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskDataMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsgV2_Updates_Task_Data()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _taskId,
                "{ \"body\":\"body\"}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsgV2_Headers_In_Uppercase_Updates_Task_Data()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _taskId,
                "{ \"body\":\"body\"}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _taskId,
                "{ \"body\":\"body\"}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, updateTaskMessage.Data);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskDataFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskDataFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _taskId,
                "{ \"body\":\"body\"}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, updateTaskMessage.Data);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskDataFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskDataFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidTask_UpdateTaskDataMsg_Emits_Only_UpdateTaskDataSucceededEventV2_Event()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskDataMsg
            (
                _task3Id,
                "{ \"body\":\"body\"}",
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskDataMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First(x => x.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidTask_UpdateTaskDataMsgV2_Emits_Only_UpdateTaskDataSucceededEventV2_Event()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _task3Id,
                "{ \"body\":\"body\"}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First(x => x.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsg_Task_Already_In_Final_State_Doesnt_Update_Data()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskDataMsg
            (
                _task2Id,
                "{}",
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskDataMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);
            
            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {_task2Id} is finalized and cannot be modified.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_UpdateTaskDataMsgV2_Task_Already_In_Final_State_Doesnt_Update_Data()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _task2Id,
                "{}"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {_task2Id} is finalized and cannot be modified.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskDataMsg_Update_Task_Data()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskDataMsg
            (
                _taskId,
                "{",
                Guid.NewGuid()
            );

            // Act
            await Subscribe<UpdateTaskDataMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Data not in JSON format", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsg", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskDataMsgV2_Update_Task_Data_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskDataMsgV2
            (
                _taskId,
                "{"
            );

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Data not in JSON format", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsgV2", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskDataMsg_Update_Data_Fails(UpdateTaskDataMsg invalidMsg)
        {
            // Act
            await Subscribe<UpdateTaskDataMsg>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault(x => x.TaskId == _taskId);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, invalidMsg.Data);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsg", errorMessageBody.GetType().Name);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_UpdateTaskDataMsgV2_Update_Data_Fails(UpdateTaskDataMsgV2 invalidMsg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<UpdateTaskDataMsgV2>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await System.Threading.Tasks.Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault(x => x.TaskId == _taskId);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, invalidMsg.Data);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskDataFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskDataMsgV2", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new UpdateTaskDataMsg[]
                {
                    new UpdateTaskDataMsg
                    (
                        _taskId,
                        "{",
                        Guid.NewGuid()
                    )
                };
                yield return new UpdateTaskDataMsg[]
                {
                    new UpdateTaskDataMsg
                    (
                        _taskId,
                        "[",
                        Guid.NewGuid()
                    )
                };
                yield return new UpdateTaskDataMsg[]
                {
                     new UpdateTaskDataMsg
                    (
                        _taskId,
                        "{name: \"John Doe\"}",
                        Guid.NewGuid()
                    )
                };
                yield return new UpdateTaskDataMsg[]
                {
                     new UpdateTaskDataMsg
                    (
                        Guid.Empty,
                        "{\"name\": \"John Doe\"}",
                        Guid.NewGuid()
                    )
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new UpdateTaskDataMsgV2[]
                {
                    new UpdateTaskDataMsgV2
                    (
                        _taskId,
                        "{"
                    )
                };
                yield return new UpdateTaskDataMsgV2[]
                {
                    new UpdateTaskDataMsgV2
                    (
                        _taskId,
                        "["
                    )
                };
                yield return new UpdateTaskDataMsgV2[]
                {
                     new UpdateTaskDataMsgV2
                    (
                        _taskId,
                        "{name: \"John Doe\"}"
                    )
                };
                yield return new UpdateTaskDataMsgV2[]
                {
                     new UpdateTaskDataMsgV2
                    (
                        Guid.Empty,
                        "{\"name\": \"John Doe\"}"
                    )
                };
            }
        }
    }
}
