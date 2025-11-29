using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FiveDegrees.Messages.Task;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using TaskManager.API.Models;
using TaskManager.Infrastructure.Models;
using TaskManager.Tests.Mocks;
using Xunit;

namespace TaskManager.Tests.IntegrationTests.API
{
    public class TasksControllerTests : TestFixture
    {
        private const string _apiUrl = "api/tasks";

        [Fact]
        public async Task GetTaskById_Returns_Task()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("x-user-id", "F82D575E-4D17-47AB-A4F0-3B1011838345");

            const string taskId = "21fc38b3-da2e-4c0e-a058-3b4e170e592f";

            using var context = Resolve<TasksDbContext>();
            var expectedTask = new TaskDbo
            {
                TaskId = Guid.Parse(taskId),
                Callback = "https://www.test.com",
                AssignmentType = "User",
                AssignedToEntityId = Guid.NewGuid(),
                Data = "{}",
                SourceId = Guid.NewGuid().ToString(),
                Status = "active",
                TaskType = "Object",
                Comments = new List<CommentDbo>()
                {
                    new CommentDbo()
                    {
                        CommentId = Guid.NewGuid(),
                        CreatedById = Guid.NewGuid(),
                        TaskId = Guid.Parse(taskId),
                        Text = "commentText",
                        CreatedDate = DateTime.Now
                    }
                }
            };
            context.Tasks.Add(expectedTask);
            context.SaveChanges();

            // Act

            var response = await client.GetAsync($"{_apiUrl}/{taskId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<TaskDto>();

            Assert.NotNull(result);
            Assert.Equal(expectedTask.TaskId, result.TaskId);
            Assert.Equal(expectedTask.AssignedToEntityId, result.AssignedToEntityId);
            Assert.Equal(expectedTask.AssignmentType, result.AssignmentType);
            //Assert.Equal(expectedTask.Callback, result.Callback); 
            //remove this from Dto, it's an internal thing..
            Assert.Equal(expectedTask.SourceId, result.SourceId);
            Assert.Equal(expectedTask.Data, result.Data);
            Assert.Equal(expectedTask.Status, result.Status);
            Assert.Equal(expectedTask.FinalState, result.FinalState);
            Assert.Equal(expectedTask.TaskType, result.TaskType);

            Assert.Equal(expectedTask.Comments.Single().CommentId, result.Comments.Single().CommentId);
            Assert.Equal(expectedTask.Comments.Single().TaskId, result.Comments.Single().TaskId);
            Assert.Equal(expectedTask.Comments.Single().Text, result.Comments.Single().Text);
            Assert.Equal(expectedTask.Comments.Single().CreatedById, result.Comments.Single().CreatedById);
            Assert.Equal(expectedTask.Comments.Single().CreatedDate, result.Comments.Single().CreatedDate);
        }

        [Fact]
        public async Task GetTaskById_Returns_NotFound()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            const string taskId = "7df152f8-e967-4a07-9406-441392216158";

            // Act
            var response = await client.GetAsync($"{_apiUrl}/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTaskById_With_Invalid_Int_Query_Returns_BadRequest()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            const int taskId = 1;

            // Act
            var response = await client.GetAsync($"{_apiUrl}/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTaskById_With_Invalid_String_Query_Returns_BadRequest()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            const string taskId = "invalid_string";

            // Act
            var response = await client.GetAsync($"{_apiUrl}/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTaskById_With_Invalid_Empty_Guid_Returns_NotFound()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var taskId = Guid.Empty;

            // Act
            var response = await client.GetAsync($"{_apiUrl}/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Send_CreateTask_Message_Successful()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var message = new SendCreateTaskMessageRequest
            {
                CorrelationId = Guid.NewGuid(),
                SourceId = "asdasd",
                Data = "{}",
                Callback = "http://www.test.com",
                Status = "New",
                TaskType = TaskType.ApproveCreate,//"approve-create",
                AssignedToEntityId = Guid.NewGuid(),
                AssignmentType = "User",
                SourceName = "sourceName",
                Subject = "subject",
                RequestorId = Guid.NewGuid(),
                Comment = "",
                Relations = new RelationDto[] { new RelationDto { EntityId = "123", EntityType = "Loan" }, new RelationDto { EntityId = "321", EntityType = "Person" } }
            };
            var request = new ObjectContent<SendCreateTaskMessageRequest>(message, new JsonMediaTypeFormatter());

            // Act
            var response = await client.PostAsync(_apiUrl, request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var taskDto = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskDto>(content);

            Assert.Equal(message.AssignedToEntityId, taskDto.AssignedToEntityId);
            Assert.Empty(taskDto.Comments);
            Assert.NotEqual(default, taskDto.CreatedBy);
        }

        [Fact]
        public async Task Send_CreateTaskWithComment_Message_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var comment = "Test comment";

            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var message = new SendCreateTaskMessageRequest
            {
                CorrelationId = Guid.NewGuid(),
                SourceId = "asdasd",
                Data = "{}",
                Callback = "http://www.test.com",
                Status = "New",
                TaskType = TaskType.ApproveCreate,//"approve-create",
                AssignedToEntityId = Guid.NewGuid(),
                AssignmentType = "User",
                SourceName = "sourceName",
                Subject = "subject",
                RequestorId = userId,
                Relations = new RelationDto[] { new RelationDto { EntityId = "123", EntityType = "Loan" }, new RelationDto { EntityId = "321", EntityType = "Person" } },
                Comment = comment
            };
            var request = new ObjectContent<SendCreateTaskMessageRequest>(message, new JsonMediaTypeFormatter());

            // Act
            var response = await client.PostAsync(_apiUrl, request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var taskDto = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskDto>(content);

            Assert.Equal(message.AssignedToEntityId, taskDto.AssignedToEntityId);
            Assert.Equal(userId, taskDto.CreatedBy);
            Assert.Single(taskDto.Comments);
            Assert.Equal(comment, taskDto.Comments.Single().Text);
            Assert.NotEqual(default, taskDto.CreatedBy);
        }

        [Fact]
        public async Task Send_Invalid_CreateTask_Message_Fails()
        {
            // Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var message = new SendCreateTaskMessageRequest
            {
                CorrelationId = Guid.Empty,
                SourceId = string.Empty,
                Data = string.Empty,
                Callback = string.Empty,
                Status = string.Empty,
                TaskType = TaskType.ApproveCreate,//"approve-create",
                AssignedToEntityId = Guid.Empty,
                AssignmentType = string.Empty
            };
            var request = new ObjectContent<SendCreateTaskMessageRequest>(message, new JsonMediaTypeFormatter());

            // Act
            var response = await client.PostAsync(_apiUrl, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private TResult Resolve<TResult>()
        {
            return host.Services.GetAutofacRoot().Resolve<TResult>();
        }
    }
}
