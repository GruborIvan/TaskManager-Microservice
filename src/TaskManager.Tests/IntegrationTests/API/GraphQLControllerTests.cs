using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphQL.EntityFramework.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using TaskManager.Infrastructure.Models;
using TaskManager.Tests.Mocks;
using Xunit;

namespace TaskManager.Tests.IntegrationTests.API
{
    public class GraphQLControllerTests : TestFixture
    {
        public GraphQLControllerTests()
        {
            ClientQueryExecutor.SetQueryUri("/api/graphql");
        }

        [Fact]
        public async Task Get_ReturnsAllTasks()
        {
            //Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var tasks = GetTasks();
            foreach(TaskDbo task in tasks)
            {
                task.Comments = GetComments(task.TaskId);
                task.TaskRelations = GetTaskRelations(task.TaskId);
            }

            await SaveTasksToDb(tasks);

            var query = @"
            {
                tasks {
                    items {
                        taskId
                        createdById
                        changedBy
                        taskType
                        callback
                        createdDate
                        changedDate
                        status
                        data
                        sourceName
                        subject
                        sourceId
                        finalState
                        assignedToEntityId
                        assignmentType
                        comments {
                            commentId
                            createdDate
                            taskId
                            text
                        }   
                        relations {
                            id,
                            taskId,
                            entityId,
                            entityType
                        }
                    }
                }
            }";

            //Act
            using var response = await ClientQueryExecutor.ExecuteGet(client, query);

            //Assert
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(result);

            Assert.NotNull(result);
            // Task[0]
            Assert.Equal(tasks[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][0]["taskId"]);
            Assert.Equal(tasks[0].CreatedById, (Guid)jobj["data"]["tasks"]["items"][0]["createdById"]);
            Assert.Equal(tasks[0].ChangedBy, (Guid)jobj["data"]["tasks"]["items"][0]["changedBy"]);
            Assert.Equal(tasks[0].TaskType, (string)jobj["data"]["tasks"]["items"][0]["taskType"]);
            Assert.Equal(tasks[0].Callback, (string)jobj["data"]["tasks"]["items"][0]["callback"]);
            Assert.Contains(DateTimeToStringFormatter(tasks[0].CreatedDate), (string)jobj["data"]["tasks"]["items"][0]["createdDate"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[0].ChangedDate), (string)jobj["data"]["tasks"]["items"][0]["changedDate"]);
            Assert.Equal(tasks[0].Status, (string)jobj["data"]["tasks"]["items"][0]["status"]);
            Assert.Equal(tasks[0].Data, (string)jobj["data"]["tasks"]["items"][0]["data"]);
            Assert.Equal(tasks[0].SourceName, (string)jobj["data"]["tasks"]["items"][0]["sourceName"]);
            Assert.Equal(tasks[0].Subject, (string)jobj["data"]["tasks"]["items"][0]["subject"]);
            Assert.Equal(tasks[0].SourceId, (string)jobj["data"]["tasks"]["items"][0]["sourceId"]);
            Assert.Equal(tasks[0].FinalState, (bool)jobj["data"]["tasks"]["items"][0]["finalState"]);
            Assert.Equal(tasks[0].AssignedToEntityId, (Guid)jobj["data"]["tasks"]["items"][0]["assignedToEntityId"]);
            Assert.Equal(tasks[0].AssignmentType, (string)jobj["data"]["tasks"]["items"][0]["assignmentType"]);

            Assert.Equal(tasks[0].Comments[0].CommentId, (Guid)jobj["data"]["tasks"]["items"][0]["comments"][0]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[0].Comments[0].CreatedDate), (string)jobj["data"]["tasks"]["items"][0]["comments"][0]["createdDate"]);
            Assert.Equal(tasks[0].Comments[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][0]["comments"][0]["taskId"]);
            Assert.Equal(tasks[0].Comments[0].Text, (string)jobj["data"]["tasks"]["items"][0]["comments"][0]["text"]);

            Assert.Equal(tasks[0].Comments[1].CommentId, (Guid)jobj["data"]["tasks"]["items"][0]["comments"][1]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[0].Comments[1].CreatedDate), (string)jobj["data"]["tasks"]["items"][0]["comments"][1]["createdDate"]);
            Assert.Equal(tasks[0].Comments[1].TaskId, (Guid)jobj["data"]["tasks"]["items"][0]["comments"][1]["taskId"]);
            Assert.Equal(tasks[0].Comments[1].Text, (string)jobj["data"]["tasks"]["items"][0]["comments"][1]["text"]);

            Assert.Equal(tasks[0].TaskRelations[0].RelationId, (Guid)jobj["data"]["tasks"]["items"][0]["relations"][0]["id"]);
            Assert.Equal(tasks[0].TaskRelations[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][0]["relations"][0]["taskId"]);
            Assert.Equal(tasks[0].TaskRelations[0].EntityId, (string)jobj["data"]["tasks"]["items"][0]["relations"][0]["entityId"]);
            Assert.Equal(tasks[0].TaskRelations[0].EntityType, (string)jobj["data"]["tasks"]["items"][0]["relations"][0]["entityType"]);

            // Task[1]
            Assert.Equal(tasks[1].TaskId, (Guid)jobj["data"]["tasks"]["items"][1]["taskId"]);
            Assert.Equal(tasks[1].CreatedById, (Guid)jobj["data"]["tasks"]["items"][1]["createdById"]);
            Assert.Equal(tasks[1].ChangedBy, (Guid)jobj["data"]["tasks"]["items"][1]["changedBy"]);
            Assert.Equal(tasks[1].TaskType, (string)jobj["data"]["tasks"]["items"][1]["taskType"]);
            Assert.Equal(tasks[1].Callback, (string)jobj["data"]["tasks"]["items"][1]["callback"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[1].CreatedDate), (string)jobj["data"]["tasks"]["items"][1]["createdDate"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[1].ChangedDate) ?? "", (string)jobj["data"]["tasks"]["items"][1]["changedDate"]);
            Assert.Equal(tasks[1].Status, (string)jobj["data"]["tasks"]["items"][1]["status"]);
            Assert.Equal(tasks[1].Data, (string)jobj["data"]["tasks"]["items"][1]["data"]);
            Assert.Equal(tasks[1].SourceName, (string)jobj["data"]["tasks"]["items"][1]["sourceName"]);
            Assert.Equal(tasks[1].Subject, (string)jobj["data"]["tasks"]["items"][1]["subject"]);
            Assert.Equal(tasks[1].SourceId, (string)jobj["data"]["tasks"]["items"][1]["sourceId"]);
            Assert.Equal(tasks[1].FinalState, (bool)jobj["data"]["tasks"]["items"][1]["finalState"]);
            Assert.Equal(tasks[1].AssignedToEntityId, (Guid)jobj["data"]["tasks"]["items"][1]["assignedToEntityId"]);
            Assert.Equal(tasks[1].AssignmentType, (string)jobj["data"]["tasks"]["items"][1]["assignmentType"]);

            Assert.Equal(tasks[1].Comments[0].CommentId, (Guid)jobj["data"]["tasks"]["items"][1]["comments"][0]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[1].Comments[0].CreatedDate), (string)jobj["data"]["tasks"]["items"][1]["comments"][0]["createdDate"]);
            Assert.Equal(tasks[1].Comments[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][1]["comments"][0]["taskId"]);
            Assert.Equal(tasks[1].Comments[0].Text, (string)jobj["data"]["tasks"]["items"][1]["comments"][0]["text"]);

            Assert.Equal(tasks[1].Comments[1].CommentId, (Guid)jobj["data"]["tasks"]["items"][1]["comments"][1]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[1].Comments[1].CreatedDate), (string)jobj["data"]["tasks"]["items"][1]["comments"][1]["createdDate"]);
            Assert.Equal(tasks[1].Comments[1].TaskId, (Guid)jobj["data"]["tasks"]["items"][1]["comments"][1]["taskId"]);
            Assert.Equal(tasks[1].Comments[1].Text, (string)jobj["data"]["tasks"]["items"][1]["comments"][1]["text"]);

            Assert.Equal(tasks[1].TaskRelations[0].RelationId, (Guid)jobj["data"]["tasks"]["items"][1]["relations"][0]["id"]);
            Assert.Equal(tasks[1].TaskRelations[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][1]["relations"][0]["taskId"]);
            Assert.Equal(tasks[1].TaskRelations[0].EntityId, (string)jobj["data"]["tasks"]["items"][1]["relations"][0]["entityId"]);
            Assert.Equal(tasks[1].TaskRelations[0].EntityType, (string)jobj["data"]["tasks"]["items"][1]["relations"][0]["entityType"]);

            // Task[2]
            Assert.Equal(tasks[2].TaskId, (Guid)jobj["data"]["tasks"]["items"][2]["taskId"]);
            Assert.Equal(tasks[2].CreatedById, (Guid)jobj["data"]["tasks"]["items"][2]["createdById"]);
            Assert.Equal(tasks[2].ChangedBy, (Guid)jobj["data"]["tasks"]["items"][2]["changedBy"]);
            Assert.Equal(tasks[2].TaskType, (string)jobj["data"]["tasks"]["items"][2]["taskType"]);
            Assert.Equal(tasks[2].Callback, (string)jobj["data"]["tasks"]["items"][2]["callback"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[2].CreatedDate), (string)jobj["data"]["tasks"]["items"][2]["createdDate"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[2].ChangedDate), (string)jobj["data"]["tasks"]["items"][2]["changedDate"]);
            Assert.Equal(tasks[2].Status, (string)jobj["data"]["tasks"]["items"][2]["status"]);
            Assert.Equal(tasks[2].Data, (string)jobj["data"]["tasks"]["items"][2]["data"]);
            Assert.Equal(tasks[2].SourceName, (string)jobj["data"]["tasks"]["items"][2]["sourceName"]);
            Assert.Equal(tasks[2].Subject, (string)jobj["data"]["tasks"]["items"][2]["subject"]);
            Assert.Equal(tasks[2].SourceId, (string)jobj["data"]["tasks"]["items"][2]["sourceId"]);
            Assert.Equal(tasks[2].FinalState, (bool)jobj["data"]["tasks"]["items"][2]["finalState"]);
            Assert.Equal(tasks[2].AssignedToEntityId, (Guid)jobj["data"]["tasks"]["items"][2]["assignedToEntityId"]);
            Assert.Equal(tasks[2].AssignmentType, (string)jobj["data"]["tasks"]["items"][2]["assignmentType"]);

            Assert.Equal(tasks[2].Comments[0].CommentId, (Guid)jobj["data"]["tasks"]["items"][2]["comments"][0]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[2].Comments[0].CreatedDate), (string)jobj["data"]["tasks"]["items"][2]["comments"][0]["createdDate"]);
            Assert.Equal(tasks[2].Comments[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][2]["comments"][0]["taskId"]);
            Assert.Equal(tasks[2].Comments[0].Text, (string)jobj["data"]["tasks"]["items"][2]["comments"][0]["text"]);

            Assert.Equal(tasks[2].Comments[1].CommentId, (Guid)jobj["data"]["tasks"]["items"][2]["comments"][1]["commentId"]);
            Assert.Equal(DateTimeToStringFormatter(tasks[2].Comments[1].CreatedDate), (string)jobj["data"]["tasks"]["items"][2]["comments"][1]["createdDate"]);
            Assert.Equal(tasks[2].Comments[1].TaskId, (Guid)jobj["data"]["tasks"]["items"][2]["comments"][1]["taskId"]);
            Assert.Equal(tasks[2].Comments[1].Text, (string)jobj["data"]["tasks"]["items"][2]["comments"][1]["text"]);

            Assert.Equal(tasks[2].TaskRelations[0].RelationId, (Guid)jobj["data"]["tasks"]["items"][2]["relations"][0]["id"]);
            Assert.Equal(tasks[2].TaskRelations[0].TaskId, (Guid)jobj["data"]["tasks"]["items"][2]["relations"][0]["taskId"]);
            Assert.Equal(tasks[2].TaskRelations[0].EntityId, (string)jobj["data"]["tasks"]["items"][2]["relations"][0]["entityId"]);
            Assert.Equal(tasks[2].TaskRelations[0].EntityType, (string)jobj["data"]["tasks"]["items"][2]["relations"][0]["entityType"]);
        }

        [Fact]
        public async Task Get_NullField_ReturnsError()
        {
            //Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var tasks = GetTasks();
            tasks[0].Status = null;
            await SaveTasksToDb(tasks);
            var query = @"
                { 
                    tasks {
                        items {
                            taskId
                            status
                        }
                    }
                }";

            //Act
            using var response = await ClientQueryExecutor.ExecuteGet(client, query);

            //Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"errors\":[{", result);
            Assert.Contains("Cannot return null for non-null type. Field: status", result);
        }

        [Fact]
        public async Task Get_FilterApplied_ReturnsTask()
        {
            //Arrange
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ContainsAllClaimsAuthHandlers>(
                            "Test", options => { });
            });
            host = hostBuilder.Start();
            client = host.GetTestClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var tasks = GetTasks();
            tasks[1].FinalState = true;
            await SaveTasksToDb(tasks);

            var query = @"
            {
                tasks(
                    where: {
                        path: ""finalState""
                        comparison: ""equal""
                        value: ""false""
                    }
                ) {
                    items {
                        taskType
                    }
                }
            }";

             //Act
            using var response = await ClientQueryExecutor.ExecuteGet(client, query, AuthHeaders);

            //Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Assert.Contains("first", result);
            Assert.Contains("third", result);
        }

        [Fact]
        public async Task Query_Comments_By_TaskId_Returns_All_Comments()
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

            var tasks = GetTasks();
            foreach (TaskDbo task in tasks)
            {
                task.Comments = GetComments(task.TaskId);
            }
            var commentsList = tasks.Select(x => x.Comments).First();
            var taskId = commentsList.First().TaskId;
            var comments = commentsList.Where(c => c.TaskId == taskId).ToList();

            await SaveTasksToDb(tasks);

            // Act
            var query = @$"{{
                  comments(taskId: ""{taskId}""){{
                    items{{
                      commentId
                      taskId
                      text
                      createdDate
                      createdById
                    }}
                }}
            }}";

            using var response = await ClientQueryExecutor.ExecuteGet(client, query);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(result);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(comments[0].TaskId, (Guid)jobj["data"]["comments"]["items"][0]["taskId"]);
            Assert.Equal(comments[0].CommentId, (Guid)jobj["data"]["comments"]["items"][0]["commentId"]);
            Assert.Equal(comments[0].Text, (string)jobj["data"]["comments"]["items"][0]["text"]);
            Assert.Equal(comments[0].CreatedById, (Guid)jobj["data"]["comments"]["items"][0]["createdById"]);

            Assert.Equal(comments[1].TaskId, (Guid)jobj["data"]["comments"]["items"][1]["taskId"]);
            Assert.Equal(comments[1].CommentId, (Guid)jobj["data"]["comments"]["items"][1]["commentId"]);
            Assert.Equal(comments[1].Text, (string)jobj["data"]["comments"]["items"][1]["text"]);
            Assert.Equal(comments[1].CreatedById, (Guid)jobj["data"]["comments"]["items"][1]["createdById"]);
        }

        [Fact]
        public async Task Query_RelatedTasks_By_EntityId_Returns_All_RelatedTasks()
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

            var tasks = GetTasks();
            foreach (TaskDbo task in tasks)
            {
                task.Comments = GetComments(task.TaskId);
                task.TaskRelations = GetTaskRelations(task.TaskId);
            }
            var taskRelationsList = tasks.Select(x => x.TaskRelations).First();
            var entityId = taskRelationsList.First().EntityId;
            var taskId = taskRelationsList.First().TaskId;
            var relatedTasks = tasks.Where(t => t.TaskId == taskId).ToList();


            await SaveTasksToDb(tasks);

            // Act
            var query = @$"{{
                      relatedTasks(entityId: ""{entityId}""){{
                        items{{
                          taskId
                          createdById
                          changedBy
                          taskType
                          callback
                          createdDate
                          changedDate
                          status
                          data
                          sourceName
                          subject
                          sourceId
                          finalState
                          assignedToEntityId
                          assignmentType
                          comments {{
                            commentId
                            createdDate
                            taskId
                            text
                        }} 
                      }}
                    }}
                  }}";

            using var response = await ClientQueryExecutor.ExecuteGet(client, query);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(result);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(relatedTasks[0].TaskId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["taskId"]);
            Assert.Equal(relatedTasks[0].CreatedById, (Guid)jobj["data"]["relatedTasks"]["items"][0]["createdById"]);
            Assert.Equal(relatedTasks[0].ChangedBy, (Guid)jobj["data"]["relatedTasks"]["items"][0]["changedBy"]);
            Assert.Equal(relatedTasks[0].TaskType, (string)jobj["data"]["relatedTasks"]["items"][0]["taskType"]);
            Assert.Equal(relatedTasks[0].Callback, (string)jobj["data"]["relatedTasks"]["items"][0]["callback"]);
            Assert.Equal(relatedTasks[0].Status, (string)jobj["data"]["relatedTasks"]["items"][0]["status"]);
            Assert.Equal(relatedTasks[0].SourceName, (string)jobj["data"]["relatedTasks"]["items"][0]["sourceName"]);
            Assert.Equal(relatedTasks[0].Subject, (string)jobj["data"]["relatedTasks"]["items"][0]["subject"]);
            Assert.Equal(relatedTasks[0].SourceId, (string)jobj["data"]["relatedTasks"]["items"][0]["sourceId"]);
            Assert.Equal(relatedTasks[0].FinalState, (bool)jobj["data"]["relatedTasks"]["items"][0]["finalState"]);
            Assert.Equal(relatedTasks[0].AssignedToEntityId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["assignedToEntityId"]);
            Assert.Equal(relatedTasks[0].AssignmentType, (string)jobj["data"]["relatedTasks"]["items"][0]["assignmentType"]);

            Assert.Equal(relatedTasks[0].Comments[0].CommentId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["comments"][0]["commentId"]);
            Assert.Equal(relatedTasks[0].Comments[0].TaskId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["comments"][0]["taskId"]);
            Assert.Equal(relatedTasks[0].Comments[0].Text, (string)jobj["data"]["relatedTasks"]["items"][0]["comments"][0]["text"]);

            Assert.Equal(relatedTasks[0].Comments[1].CommentId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["comments"][1]["commentId"]);
            Assert.Equal(relatedTasks[0].Comments[1].TaskId, (Guid)jobj["data"]["relatedTasks"]["items"][0]["comments"][1]["taskId"]);
            Assert.Equal(relatedTasks[0].Comments[1].Text, (string)jobj["data"]["relatedTasks"]["items"][0]["comments"][1]["text"]);
        }

        private static Dictionary<string, string> AuthHeaders =
            new Dictionary<string, string>
            {
                { "Authorization", "Bearer {key}" },
                { "x-user-id", "A3C842DD-C47F-4B41-5F9C-08D7CC06D7BD" }
            };
        private static IReadOnlyList<TaskDbo> GetTasks()
        {
            Guid taskId = Guid.NewGuid();

            return new List<TaskDbo>
            {
                new TaskDbo
                {
                    TaskId = taskId,
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.NewGuid(),
                    TaskType = "first",
                    Callback = "https://www.asdddef.com/api/callback",
                    ChangedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    SourceName = "source",
                    Subject = "subject",
                    Status = "1",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    FinalState = false,
                    AssignedToEntityId = Guid.NewGuid(),
                    AssignmentType = "user"
                },
                new TaskDbo
                {
                    TaskId = Guid.NewGuid(),
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.NewGuid(),
                    TaskType = "second",
                    Callback = "https://www.asdddef.com/api/callback",
                    ChangedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    SourceName = "source",
                    Subject = "subject",
                    Status = "2",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    FinalState = false,
                    AssignedToEntityId = Guid.NewGuid(),
                    AssignmentType = "role"
                },
                new TaskDbo
                {
                    TaskId = Guid.NewGuid(),
                    CreatedById = Guid.NewGuid(),
                    ChangedBy = Guid.NewGuid(),
                    TaskType = "third",
                    Callback = "https://www.asdddef.com/api/callback",
                    ChangedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    SourceName = "source",
                    Subject = "subject",
                    Status = "3",
                    Data = "{}",
                    SourceId = Guid.NewGuid().ToString(),
                    FinalState = false,
                    AssignedToEntityId = Guid.NewGuid(),
                    AssignmentType = "user"
                },
            };
        }

        private static List<CommentDbo> GetComments(Guid taskId)
        {
            return new List<CommentDbo>()
            {
                GetComment(taskId),
                GetComment(taskId)
            };
        }

        private static List<TaskRelationDbo> GetTaskRelations(Guid taskId)
        {
            return new List<TaskRelationDbo>()
            {
                GetTaskRelation(taskId)
            };
        }

        private static CommentDbo GetComment(Guid taskId)
        {
            return new CommentDbo()
            {
                CommentId = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                TaskId = taskId,
                Text = "CommentText"
            };
        }

        private static TaskRelationDbo GetTaskRelation(Guid taskId)
        {
            return new TaskRelationDbo()
            {
                EntityId = Guid.NewGuid().ToString(),
                TaskId = taskId,
                EntityType = "person"
            };
        }

        private async Task SaveTasksToDb(IEnumerable<TaskDbo> tasks)
        {
            var context = Resolve<TasksDbContext>(host);
            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }

        // This is how SQL Server internally formats UTC date by default
        private static string GetSqlCompatibleDateTime(DateTime? dateTime)
        {
            var rgx = new Regex("0+Z$");
            return rgx.Replace(dateTime?.ToString("o"), "Z");
        }

        private static string DateTimeToStringFormatter(DateTime? dateTime)
        {
            return dateTime?.ToString("MM/dd/yyyy HH:mm:ss") ?? "";
        }
    }
}
