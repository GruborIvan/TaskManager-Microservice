using FiveDegrees.Messages.Task;
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
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class FinalizeTaskStatusTests : TestFixture
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

        public FinalizeTaskStatusTests()
        {
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
                    ChangedBy = Guid.Parse("00000000-0000-0000-0000-000000000000")
                },
                new TaskDbo
                {
                    TaskId = _task2Id,
                    Callback = "https://www.test.com",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    Status = "active",
                    TaskType = "Object",
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
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsg_Task_Gets_Finalized()
        {
            // Arrange
            var message = new FinalizeTaskStatusMsg(_taskId, "Approved", true, Guid.NewGuid());

            // Act
            await Subscribe<FinalizeTaskStatusMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(message.Status, task.Status);
            Assert.Equal(message.FinalStatus, task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsgV2_Task_Gets_Finalized()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new FinalizeTaskStatusMsgV2(_taskId, "Approved", true);

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(message.Status, task.Status);
            Assert.Equal(message.FinalStatus, task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsgV2_Headers_In_Uppercase_Task_Gets_Finalized()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var message = new FinalizeTaskStatusMsgV2(_taskId, "Approved", true);

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.Equal(message.Status, task.Status);
            Assert.Equal(message.FinalStatus, task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new FinalizeTaskStatusMsgV2(_taskId, "Approved", true);

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.NotEqual(task.Status, message.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<FinalizeTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<FinalizeTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("FinalizeTaskStatusMsgV2", errorMessageBody.GetType().Name);
            Assert.False(task.FinalState);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var message = new FinalizeTaskStatusMsgV2(_taskId, "Approved", true);

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.NotEqual(task.Status, message.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<FinalizeTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<FinalizeTaskStatusFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("FinalizeTaskStatusMsgV2", errorMessageBody.GetType().Name);
            Assert.False(task.FinalState);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsg_Finalizes_Task_Emits_Only_V2_Events()
        {
            // Arrange
            var message = new FinalizeTaskStatusMsg(_task2Id, "Approved", true, Guid.NewGuid());

            // Act
            await Subscribe<FinalizeTaskStatusMsg>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First(x => x.TaskId == _task2Id);

            // Assert
            Assert.Equal(message.Status, task.Status);
            Assert.Equal(message.FinalStatus, task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
             _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_FinalizeTaskStatusMsgV2_Finalizes_Task_Emits_Only_V2_Events()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var message = new FinalizeTaskStatusMsgV2(_task2Id, "Approved", true);

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(message, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First(x => x.TaskId == _task2Id);

            // Assert
            Assert.Equal(message.Status, task.Status);
            Assert.Equal(message.FinalStatus, task.FinalState);
            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), task.ChangedBy);
            Assert.NotNull(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<UpdateTaskStatusSucceededEventV2>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_FinalizeTaskStatusMsgs_Doesnt_Finalize_Task(FinalizeTaskStatusMsg msg)
        {
            // Act
            await Subscribe<FinalizeTaskStatusMsg>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.NotEqual(task.Status, msg.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("FinalizeTaskStatusMsg", errorMessageBody.GetType().Name);
            Assert.False(task.FinalState);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_FinalizeTaskStatusMsgsV2_Doesnt_Finalize_Task(FinalizeTaskStatusMsgV2 msg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<FinalizeTaskStatusMsgV2>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.First();

            // Assert
            Assert.NotNull(task);
            Assert.NotEqual(task.Status, msg.Status);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000000"), task.ChangedBy);
            Assert.Null(task.ChangedDate);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<FinalizeTaskStatusFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("FinalizeTaskStatusMsgV2", errorMessageBody.GetType().Name);
            Assert.False(task.FinalState);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new FinalizeTaskStatusMsg[]
                {
                    new FinalizeTaskStatusMsg(Guid.NewGuid(), "approved", true, Guid.NewGuid())
                };
                yield return new FinalizeTaskStatusMsg[]
                {
                     new FinalizeTaskStatusMsg(_taskId, string.Empty, true, Guid.NewGuid())
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new FinalizeTaskStatusMsgV2[]
                {
                    new FinalizeTaskStatusMsgV2(Guid.NewGuid(), "approved", true)
                };
                yield return new FinalizeTaskStatusMsgV2[]
                {
                     new FinalizeTaskStatusMsgV2(_taskId, string.Empty, true)
                };
            }
        }
    }
}
