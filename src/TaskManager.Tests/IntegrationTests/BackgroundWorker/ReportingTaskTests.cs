using System;
using System.Collections.Generic;
using FiveDegrees.Messages.Task;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManager.Infrastructure.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Tests.IntegrationTests.BackgroundWorker
{
    public class ReportingTaskTests : TestFixture
    {
        private const int WaitTimeInMilliseconds = 10000;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            { "x-external-id", "11111111-1111-1111-1111-111111111111" },
            { "x-user-id", "11111111-1111-1111-1111-111111111111" },
        };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ReportingTaskTests()
        {
            StartHost();
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        [Fact]
        public async Task ValidMessage_ReportingTask_CreateBlobWithCorrelationIdName()
        {
            // Arrange 
            var taskDbos = new List<TaskDbo>
                {
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow,
                        ChangedDate = null
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = null
                    },
                };
            using var context = Resolve<TasksDbContext>();
            context.Tasks.AddRange(taskDbos);
            context.SaveChanges();

            var correlationId = Guid.NewGuid();
            // Act
            var reportingTaskMsg = new ReportingTaskMsg(correlationId, new List<ReportingTaskEntities> { ReportingTaskEntities.Task }, DateTime.Now.AddDays(-1), null);

            await Subscribe<ReportingTaskMsg>();
            await Publish(reportingTaskMsg, _headers);

            _msgHandled.WaitOne(WaitTimeInMilliseconds);

            // Assert
            Assert.Equal($"Task/{correlationId}.json", blobData.blobName);
        }

        [Fact]
        public async Task InvalidMessage_ReportingTask_InvalidDatetimeRangeException()
        {
            // Arrange 
            var taskDbos = new List<TaskDbo>
                {
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow,
                        ChangedDate = DateTime.UtcNow
                    },
                    new TaskDbo
                    {
                        TaskId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        ChangedBy = Guid.NewGuid(),
                        SourceId = Guid.NewGuid().ToString(),
                        SourceName = "SourceName",
                        TaskType = "Create",
                        Subject = "Subject",
                        Data = "{\"testData\" : \"exampe json\"}",
                        Status = "Not Started",
                        Callback = "https://uri.com",
                        FinalState = false,
                        AssignedToEntityId = Guid.NewGuid(),
                        AssignmentType = "Person",
                        FourEyeSubjectId = Guid.NewGuid(),
                        Change = null,
                        TaskRelations = null,
                        Comments = null,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        ChangedDate = DateTime.UtcNow.AddDays(-2)
                    },
                };
            using var context = Resolve<TasksDbContext>();
            context.Tasks.AddRange(taskDbos);
            context.SaveChanges();

            var correlationId = Guid.NewGuid();
            // Act
            var reportingTaskMsg = new ReportingTaskMsg(correlationId, new List<ReportingTaskEntities> { ReportingTaskEntities.Task }, DateTime.Now.AddDays(1), null);

            await Subscribe<ReportingTaskMsg>();
            await Publish(reportingTaskMsg, _headers);

            _msgHandled.WaitOne(WaitTimeInMilliseconds);
            await Task.Delay(100);

            // Assert
            Assert.Contains("Invalid Datetime Range", exceptionMessage);

            var messageFromErrorQueue = _network.GetNextOrNull("error");
            var errorMessage = messageFromErrorQueue.Headers["rbs2-error-details"];
            Assert.Contains("Invalid Datetime Range", errorMessage);
            var jsonMessage = JObject.Parse(System.Text.Encoding.Default.GetString(messageFromErrorQueue.Body));
            var errorMessageBody = JsonConvert.DeserializeObject(jsonMessage.ToString(), _jsonSerializerSettings);
            Assert.Equal("ReportingTaskMsg", errorMessageBody.GetType().Name);
        }
    }
}
