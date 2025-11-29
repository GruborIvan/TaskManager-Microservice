using System;
using System.Linq;
using System.Threading;
using FiveDegrees.Messages.Task;
using TaskManager.Infrastructure.Models;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.IntegrationEvents;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class CreateTaskTests : TestFixture
    {
        private const int _waitTimeInMiliseconds = 10000;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public CreateTaskTests()
        {
            RegisterMockHttpClient();
            StartHost();
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }
        
        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsg_Creates_Task()
        {
            // Arrange
            var createTaskMessage = new CreateTaskMsg
            (
                Guid.NewGuid(),
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[] 
                {
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"), 
                    new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person") 
                }
            );

            // Act
            await Subscribe<CreateTaskMsg>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Arrange
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById); 
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(2, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsg_Without_Relations_Creates_Task()
        {
            // Arrange
            var createTaskMessage = new CreateTaskMsg
            (
                Guid.NewGuid(),
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new List<FiveDegrees.Messages.Task.Relation>()
                {

                }
            );

            // Act
            await Subscribe<CreateTaskMsg>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Arrange
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(0, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV2_Creates_Task()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV2
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                    new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Assert
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(2, task.TaskRelations.Count);
            
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV2_Headers_In_Uppercase_Creates_Task()
        {
            // Arrange
            _headers.Add("X-REQUEST-ID", "11111111-1111-1111-1111-111111111111");
            _headers.Add("X-COMMAND-ID", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV2
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                    new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Assert
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(2, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEvent>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV2_Missing_RequestId_Fails()
        {
            // Arrange
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV2
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                    new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);

            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Tasks.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<CreateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<CreateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-request-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-request-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("CreateTaskMsgV2", errorMessageBody.GetType().Name);
            Assert.Equal(Guid.Empty, ((CreateTaskMsgV2)errorMessageBody).TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV2_Missing_CommandId_Fails()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV2
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                    new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Tasks.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.Is<CreateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.Is<CreateTaskFailedEvent>(x => x.Error.Message.Contains("Missing x-command-id in headers")), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Missing x-command-id in headers", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("CreateTaskMsgV2", errorMessageBody.GetType().Name);
            Assert.Equal(Guid.Empty, ((CreateTaskMsgV2)errorMessageBody).TaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsg_With_New_Relation_Type_Emits_Only_CreateTaskSucceededStreamEventV2_Event()
        {
            // Arrange
            var createTaskMessage = new CreateTaskMsg
            (
                Guid.NewGuid(),
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[] 
                { 
                    new FiveDegrees.Messages.Task.Relation("123", "Person"), 
                    new FiveDegrees.Messages.Task.Relation("321", "Person"),
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                }
            );

            // Act
            await Subscribe<CreateTaskMsg>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Assert
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(3, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();
            
            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV2_With_New_Relation_Type_Emits_Only_CreateTaskSucceededStreamEventV2_Event()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV2
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("123", "Person"),
                    new FiveDegrees.Messages.Task.Relation("321", "Person"),
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Assert
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(3, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_CreateTaskMsgV3_With_New_Relation_Type_Emits_Only_CreateTaskSucceededStreamEventV2_Event()
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");
            var createTaskMessage = new CreateTaskMsgV3
            (
                "asdasd",
                "sourcename",
                "sourceSubject",
                "{\"data\":\"asd\"}",
                "https://www.testestes.com/qweewqwewewe",
                TaskType.ApproveCreate,
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(),
                new FiveDegrees.Messages.Task.Relation[]
                {
                    new FiveDegrees.Messages.Task.Relation("123", "Person"),
                    new FiveDegrees.Messages.Task.Relation("321", "Person"),
                    new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                }
            );

            // Act
            await Subscribe<CreateTaskMsgV3>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);

            using var context = Resolve<TasksDbContext>();
            var x = context.Tasks.AsEnumerable().ToList();
            var task = context.Tasks.Include(t => t.TaskRelations).Single(t => t.SourceId == createTaskMessage.SourceId);

            // Assert
            Assert.Equal(createTaskMessage.TaskType.ToString(), task.TaskType);
            Assert.Equal(createTaskMessage.Callback, task.Callback);
            Assert.Equal(createTaskMessage.Data, task.Data);
            Assert.Equal(createTaskMessage.FourEyeSubjectId, task.FourEyeSubjectId);
            Assert.Equal(InitiatorId, task.CreatedById);
            Assert.Equal(createTaskMessage.Status, task.Status);
            Assert.Equal(createTaskMessage.SourceId, task.SourceId);
            Assert.Equal(createTaskMessage.SourceName, task.SourceName);
            Assert.Equal(createTaskMessage.Subject, task.Subject);
            Assert.Equal(createTaskMessage.AssignedToEntityId, task.AssignedToEntityId);
            Assert.Equal(createTaskMessage.AssignmentType.ToString(), task.AssignmentType);
            Assert.Equal(3, task.TaskRelations.Count);

            _mockEventStreamingService.Verify(service => service.SendAsync<object>(It.IsAny<CreateTaskSucceededStreamEventV2>(), It.IsAny<CancellationToken>()));
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskSucceededNotificationEvent>(), It.IsAny<string>()));
            _mockEventNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_CreateTaskMsg_Doesnt_Create_Task()
        {
            // Arrange
            var createTaskMessage = new CreateTaskMsg
            (
                Guid.NewGuid(),
                "asdasd",
                "sourcename",
                "sourceSubject",
                "}",
                "https://www.testestes.com/qweewqwewewe",
                "TaskType.ApproveCreate",
                "status",
                Guid.NewGuid(),
                AssignmentType.User,
                Guid.NewGuid(), default
            );

            // Act
            await Subscribe<CreateTaskMsg>();
            await Publish(createTaskMessage, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Tasks.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Data not in JSON format", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("CreateTaskMsg", errorMessageBody.GetType().Name);
            Assert.Equal(Guid.Empty, ((CreateTaskMsg)errorMessageBody).TaskId);
        }

        [Theory]
        [MemberData(nameof(InvalidMessages))]
        public async System.Threading.Tasks.Task Invalid_CreateTaskMsgs_Doesnt_Create_Task(CreateTaskMsg msg)
        {
            // Act
            await Subscribe<CreateTaskMsg>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Tasks.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("CreateTaskMsg", errorMessageBody.GetType().Name);
            Assert.Equal(Guid.Empty, ((CreateTaskMsg)errorMessageBody).TaskId);
        }

        [Theory]
        [MemberData(nameof(InvalidMessagesV2))]
        public async System.Threading.Tasks.Task Invalid_CreateTaskMsgsV2_Doesnt_Create_Task(CreateTaskMsgV2 msg)
        {
            // Arrange
            _headers.Add("x-request-id", "11111111-1111-1111-1111-111111111111");
            _headers.Add("x-command-id", "11111111-1111-1111-1111-111111111111");

            // Act
            await Subscribe<CreateTaskMsgV2>();
            await Publish(msg, _headers);

            _msgHandled.WaitOne(_waitTimeInMiliseconds);
            await Task.Delay(100);
            using var context = Resolve<TasksDbContext>();

            // Assert
            Assert.True(context.Tasks.Count() == 0);

            _mockEventStreamingService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEventStreamingService.VerifyNoOtherCalls();

            _mockEventNotificationService.Verify(service => service.SendAsync(It.IsAny<CreateTaskFailedEvent>(), It.IsAny<string>()), Times.Once);
            _mockEventNotificationService.VerifyNoOtherCalls();

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("CreateTaskMsgV2", errorMessageBody.GetType().Name);
            Assert.Equal(Guid.Empty, ((CreateTaskMsgV2)errorMessageBody).TaskId);
        }

        public static IEnumerable<object[]> InvalidMessages
        {
            get
            {
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        string.Empty,
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data:\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "hts://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        string.Empty,
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        "",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        string.Empty,
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "")
                        }
                    )
                };
                yield return new CreateTaskMsg[]
                {
                    new CreateTaskMsg
                    (
                        Guid.NewGuid(),
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        string.Empty,
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation(string.Empty, "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", string.Empty)
                        }
                    )
                };
            }
        }

        public static IEnumerable<object[]> InvalidMessagesV2
        {
            get
            {
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        string.Empty,
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data:\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "hts://www.testestes.com/qweewqwewewe",
                        "TaskType.ApproveCreate",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        string.Empty,
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        "",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        string.Empty,
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("aad8f69f-1c08-4a3d-808a-14e6a1e63338", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "Person")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        "status",
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation("", "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", "")
                        }
                    )
                };
                yield return new CreateTaskMsgV2[]
                {
                    new CreateTaskMsgV2
                    (
                        "sourceId",
                        "sourcename",
                        "sourceSubject",
                        "{\"data\":\"asd\"}",
                        "https://www.testestes.com/qweewqwewewe",
                        "approve",
                        string.Empty,
                        Guid.NewGuid(),
                        AssignmentType.User,
                        Guid.NewGuid(),
                        new FiveDegrees.Messages.Task.Relation[]
                        {
                            new FiveDegrees.Messages.Task.Relation(string.Empty, "Person"),
                            new FiveDegrees.Messages.Task.Relation("0ed97a54-9891-4f56-914e-313187cc2651", string.Empty)
                        }
                    )
                };
            }
        }
    }
}
