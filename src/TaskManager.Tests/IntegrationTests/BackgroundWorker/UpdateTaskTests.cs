using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FiveDegrees.Messages.Task;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rebus.Activation;
using Rebus.Handlers;
using TaskManager.Domain.IntegrationEvents;
using TaskManager.Infrastructure.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class UpdateTaskTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;
        private static readonly Guid _taskId = Guid.NewGuid();
        private static readonly Guid _task2Id = Guid.NewGuid();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public UpdateTaskTests()
        {
            RegisterMockHttpClient();
            StartHost();

            var taskDbo = new TaskDbo
            {
                TaskId = _taskId,
                Subject = "Test",
                Callback = "https://www.test.com",
                AssignmentType = "User",
                AssignedToEntityId = Guid.NewGuid(),
                Data = "{}",
                SourceId = Guid.NewGuid().ToString(),
                Status = "active",
                TaskType = "Object",
                FourEyeSubjectId = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                ChangedBy = Guid.Empty,
                FinalState = false
            };

            var taskDbo2 = new TaskDbo
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
            };

            using var context = Resolve<TasksDbContext>();
            context.Tasks.Add(taskDbo);
            context.Tasks.Add(taskDbo2);
            context.SaveChanges();

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Valid_UpdateTaskMsg_Updates_Task(bool final)
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskMsg
            (
                Guid.NewGuid(),
                _taskId,
                "{\"data\":\"123\"}",
                final,
                "sutatS"
            );

            // Act
            await Subscribe<UpdateTaskMsg>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == _taskId);

            // Assert
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);
        }

        [Fact]
        public async Task Valid_UpdateTaskMsgV2_Updates_Task()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _taskId,
                "{\r\n  \"description\": \"Manual task description\"\r\n}",
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Valid_UpdateTaskMsgV2_Headers_In_Uppercase_Updates_Task()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _taskId,
                "{\r\n  \"description\": \"Manual task description\"\r\n}",
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Arrange
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.Equal(updateTaskMessage.Data, task.Data);
            Assert.Equal(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskSucceededEvent>(), It.IsAny<string>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Valid_UpdateTaskMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _taskId,
                "{\r\n  \"description\": \"Manual task description\"\r\n}",
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.NotEqual(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async Task Valid_UpdateTaskMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _taskId,
                "{\r\n  \"description\": \"Manual task description\"\r\n}",
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.First();

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.NotEqual(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<UpdateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<UpdateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async Task Valid_UpdateTaskMsgV2_Task_Already_In_Final_State_Doesnt_Update()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _task2Id,
                "{\r\n  \"description\": \"Manual task description\"\r\n}",
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.NotEqual(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains($"Task with TaskId: {_task2Id} is finalized and cannot be modified.", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskMsgV2", errorMessageBody.GetType().Name);
        }

        [Fact]
        public async Task Invalid_UpdateTaskMsgV2_Update_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var updateTaskMessage = new UpdateTaskMsgV2
            (
                _taskId,
                null,
                "TestSubject"
            );

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(updateTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == updateTaskMessage.TaskId);

            // Assert
            Assert.Equal(updateTaskMessage.TaskId, task.TaskId);
            Assert.NotEqual(updateTaskMessage.Data, task.Data);
            Assert.NotEqual(updateTaskMessage.Subject, task.Subject);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Data not in JSON format", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskMsgV2", errorMessageBody.GetType().Name);
        }

        //to simulate parallel execution, we can add delay to AssignTaskToEntityHandler, after get task
        [Fact]
        public async Task Valid_UpdateTaskMsg_And_UpdateAndAssignTask_UpdatesTask()
        {
            // Arrange
            var updateTaskMessage = new UpdateTaskMsg
            (
                Guid.NewGuid(),
                _taskId,
                "{\"data\":\"123\"}",
                true,
                "sutatS"
            );

            var assignTaskMessage = new AssignTaskToEntityMsg(Guid.NewGuid(), _taskId, Guid.NewGuid(), AssignmentType.User);

            // Act
            var subscriberActivator = new BuiltinHandlerActivator();
            var updateTaskHandler = Resolve<IHandleMessages<UpdateTaskMsg>>();
            var assignTaskHandler = Resolve<IHandleMessages<AssignTaskToEntityMsg>>();
            subscriberActivator.Register(x => updateTaskHandler);
            subscriberActivator.Register(x => assignTaskHandler);
            var subscriber = CreateSubscriber(subscriberActivator);
            var sBus = subscriber.Start();
            await sBus.Subscribe<UpdateTaskMsg>();
            await sBus.Subscribe<AssignTaskToEntityMsg>();

            var pBus = ResolvePublisher().Start();
            await pBus.Publish(assignTaskMessage, _headers);
            await pBus.Publish(updateTaskMessage, _headers);
            await Task.Delay(3000);

            using var context2 = Resolve<TasksDbContext>();
            var task = context2.Tasks.Single(t => t.TaskId == _taskId);

            // Assert
            Assert.Equal(updateTaskMessage.Status, task.Status);
            Assert.Equal(assignTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(assignTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.True(task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async Task Invalid_UpdateTaskMsg_Update_Task_Fails(UpdateTaskMsg invalidMsg)
        {
            // Act
            await Subscribe<UpdateTaskMsg>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault();

            // Assert
            Assert.NotNull(task);
            Assert.NotNull(task.AssignedToEntityId);
            Assert.Equal("User", task.AssignmentType);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, invalidMsg.Data);
            Assert.NotEqual(task.Status, invalidMsg.Status);
            Assert.NotEqual(task.FinalState, invalidMsg.FinalState);
            Assert.Null(task.ChangedDate);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async Task Invalid_UpdateTaskMsgsV2_Update_Fails(UpdateTaskMsgV2 invalidMsg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<UpdateTaskMsgV2>();
            await Publish(invalidMsg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.FirstOrDefault(x => x.TaskId == _taskId);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.NotEqual(task.Data, invalidMsg.Data);
            Assert.NotEqual(task.Subject, invalidMsg.Subject);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("UpdateTaskMsgV2", errorMessageBody.GetType().Name);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new UpdateTaskMsg[]
                {
                    new UpdateTaskMsg
                        (
                            Guid.NewGuid(),
                            Guid.Empty,
                            "{\"data\":\"123\"}",
                            true,
                            "sutatS"
                        )
                };
                yield return new UpdateTaskMsg[]
                {
                    new UpdateTaskMsg
                    (
                        Guid.NewGuid(),
                        _taskId,
                        "{data:\"123\"}",
                        true,
                        "sutatS"
                    )
                };
                yield return new UpdateTaskMsg[]
                {
                   new UpdateTaskMsg
                    (
                        Guid.NewGuid(),
                        _taskId,
                        "{\"data\":\"123\"}",
                        true,
                        string.Empty
                    )
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new UpdateTaskMsgV2[]
                {
                    new UpdateTaskMsgV2
                    (
                        _taskId,
                        null,
                        null
                    )
                };
                yield return new UpdateTaskMsgV2[]
                {
                    new UpdateTaskMsgV2
                    (
                        Guid.Empty,
                        "{\"data\":\"123\"}",
                        "test"
                    )
                };
            }
        }
    }
}
