using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models.Reporting;
using TaskManager.Infrastructure.Models;
using TaskManager.Infrastructure.Services;
using Xunit;

namespace TaskManager.Tests.UnitTests.Infrastructure
{
    public class ReportingServiceTests
    {
        private readonly Mock<ILogger<ReportingService>> _loggerMock = new Mock<ILogger<ReportingService>>();
        private readonly Mock<IReportingRepository> _reportingRepositoryMock = new Mock<IReportingRepository>();
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient = new Mock<BlobServiceClient>();
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient = new Mock<BlobContainerClient>();

        public ReportingServiceTests()
        {
            _mockBlobServiceClient.Setup(c => c.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);
            _mockBlobContainerClient.Setup(
                c => c.CreateIfNotExistsAsync(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<BlobContainerEncryptionScopeOptions>(),
                    It.IsAny<CancellationToken>())
                );
            _mockBlobContainerClient.Setup(
                c => c.UploadBlobAsync(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task StoreReportAsync_NoFiles_Successful()
        {
            var requestedFiles = new Dictionary<string, byte[]>() { };
            var service = new ReportingService(_reportingRepositoryMock.Object, _mockBlobServiceClient.Object, "file", _loggerMock.Object);

            await service.StoreReportAsync(Guid.NewGuid(), requestedFiles);

            _mockBlobContainerClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockBlobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task StoreReportAsync_OneFile_Successful()
        {
            var requestedFiles = new Dictionary<string, byte[]>() { { "entitytable", Encoding.UTF8.GetBytes("{\"testData\" : \"exampe json\"}") } };
            var service = new ReportingService(_reportingRepositoryMock.Object, _mockBlobServiceClient.Object, "file", _loggerMock.Object);

            await service.StoreReportAsync(Guid.NewGuid(), requestedFiles);

            _mockBlobContainerClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockBlobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task StoreReportAsync_TwoFiles_Successful()
        {
            var requestedFiles = new Dictionary<string, byte[]>() {
                { "entitytable", Encoding.UTF8.GetBytes("{\"testData\" : \"exampe json\"}") },
                { "entitytable2", Encoding.UTF8.GetBytes("{\"testData2\" : \"exampe json 2\"}") }
            };
            var service = new ReportingService(_reportingRepositoryMock.Object, _mockBlobServiceClient.Object, "file", _loggerMock.Object);

            await service.StoreReportAsync(Guid.NewGuid(), requestedFiles);

            _mockBlobContainerClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockBlobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetReportingData_Returns_ReportingData()
        {
            // Arange  
            var taskReports = new List<TaskReport>
            {
                new TaskReport
                {
                    TaskId = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    ChangedDate = null,
                    Status = "status",
                    Callback = "https://uri.com"
                },
                new TaskReport
                {
                    TaskId = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    ChangedDate = DateTime.UtcNow,
                    Status = "status",
                    Callback = "https://uri.com"
                },
            };
            var commentReports = new List<CommentReport>
            {
                new CommentReport
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = Guid.NewGuid()
                }
            };

            _reportingRepositoryMock
                .Setup(x => x.GetTasksAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskReports);
            _reportingRepositoryMock
                .Setup(x => x.GetCommentsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commentReports);

            var repository = new ReportingService(_reportingRepositoryMock.Object, _mockBlobServiceClient.Object, "file", _loggerMock.Object);

            // Act 
            var reportingData = await repository.GetReportingDataAsync(new List<string> { "Task", "Comment" }, DateTime.UtcNow.AddDays(-1), null);

            // Assert  
            Assert.NotNull(reportingData);
            var taskReportResult = reportingData.Single(x => x.Key == "Task");
            var commentReportResult = reportingData.Single(x => x.Key == "Comment");

            var taskList = JsonConvert.DeserializeObject<List<TaskDbo>>(Encoding.UTF8.GetString(taskReportResult.Value));
            var commentList = JsonConvert.DeserializeObject<List<CommentDbo>>(Encoding.UTF8.GetString(commentReportResult.Value));
            var expectedTask1 = taskReports.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var taskObject1 = taskList.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var expectedTask2 = taskReports.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));
            var taskObject2 = taskList.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));

            var expectedComment = commentReports.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var commentObject = commentList.Single();

            Assert.Equal(2, taskList.Count);
            Assert.Single(commentList);

            Assert.Equal(expectedTask1.TaskId, taskObject1.TaskId);
            Assert.Equal(expectedTask1.Callback, taskObject1.Callback);
            Assert.Equal(expectedTask1.Status, taskObject1.Status);
            Assert.Equal(expectedTask1.CreatedDate, taskObject1.CreatedDate);
            Assert.Equal(expectedTask1.ChangedDate, taskObject1.ChangedDate);
            Assert.Equal(expectedTask2.TaskId, taskObject2.TaskId);
            Assert.Equal(expectedTask2.Callback, taskObject2.Callback);
            Assert.Equal(expectedTask2.Status, taskObject2.Status);
            Assert.Equal(expectedTask2.CreatedDate, taskObject2.CreatedDate);
            Assert.Equal(expectedTask2.ChangedDate, taskObject2.ChangedDate);

            Assert.Equal(expectedComment.CommentId, commentObject.CommentId);
            Assert.Equal(expectedComment.TaskId, commentObject.TaskId);
            Assert.Equal(expectedComment.Text, commentObject.Text);
            Assert.Equal(expectedComment.CreatedDate, commentObject.CreatedDate);
            Assert.Equal(expectedComment.CreatedById, commentObject.CreatedById);
        }

        [Fact]
        public async Task GetReportingDataForTask_Returns_OnlyTasks()
        {
            // Arange  
            var taskReports = new List<TaskReport>
            {
                new TaskReport
                {
                    TaskId = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    ChangedDate = null,
                    Status = "status",
                    Callback = "https://uri.com"
                },
                new TaskReport
                {
                    TaskId = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    ChangedDate = DateTime.UtcNow,
                    Status = "status",
                    Callback = "https://uri.com"
                },
            };
            var commentReports = new List<CommentReport>
            {
                new CommentReport
                {
                    CommentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Text = "test",
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = Guid.NewGuid()
                }
            };

            _reportingRepositoryMock
                .Setup(x => x.GetTasksAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskReports);
            _reportingRepositoryMock
                .Setup(x => x.GetCommentsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commentReports);

            var repository = new ReportingService(_reportingRepositoryMock.Object, _mockBlobServiceClient.Object, "file", _loggerMock.Object);

            // Act 
            var reportingData = await repository.GetReportingDataAsync(new List<string> { "Task" }, DateTime.UtcNow.AddDays(-1), null);

            // Assert  
            Assert.NotNull(reportingData);
            var taskReportResult = reportingData.Single(x => x.Key == "Task");
            var commentReportResult = reportingData.SingleOrDefault(x => x.Key == "Comment");

            var taskList = JsonConvert.DeserializeObject<List<TaskDbo>>(Encoding.UTF8.GetString(taskReportResult.Value));
            var expectedTask1 = taskReports.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var taskObject1 = taskList.Single(x => x.CreatedDate > DateTime.UtcNow.AddDays(-1));
            var expectedTask2 = taskReports.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));
            var taskObject2 = taskList.Single(x => x.CreatedDate < DateTime.UtcNow.AddDays(-1));

            Assert.Equal(2, taskList.Count);
            Assert.Null(commentReportResult.Key);
            Assert.Null(commentReportResult.Value);

            Assert.Equal(expectedTask1.TaskId, taskObject1.TaskId);
            Assert.Equal(expectedTask1.Callback, taskObject1.Callback);
            Assert.Equal(expectedTask1.Status, taskObject1.Status);
            Assert.Equal(expectedTask1.CreatedDate, taskObject1.CreatedDate);
            Assert.Equal(expectedTask1.ChangedDate, taskObject1.ChangedDate);
            Assert.Equal(expectedTask2.TaskId, taskObject2.TaskId);
            Assert.Equal(expectedTask2.Callback, taskObject2.Callback);
            Assert.Equal(expectedTask2.Status, taskObject2.Status);
            Assert.Equal(expectedTask2.CreatedDate, taskObject2.CreatedDate);
            Assert.Equal(expectedTask2.ChangedDate, taskObject2.ChangedDate);
        }
    }
}
