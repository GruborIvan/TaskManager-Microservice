using GraphQL.EntityFramework;
using GraphQL.Types;
using System;
using System.Linq;
using TaskManager.Infrastructure.Models;

namespace TaskManager.API.Models.GraphQL
{
    public class GraphQlQuery : QueryGraphType<TasksDbContext>
    {
        public GraphQlQuery(IEfGraphQLService<TasksDbContext> graphQlService)
            : base(graphQlService)
        {
            AddQueryConnectionField(
                name: "tasks",
                resolve: context => context.DbContext.Tasks,
                graphType: typeof(TaskGraph));

            AddQueryConnectionField(
                name: "relatedTasks",
                graphType: typeof(TaskGraph),
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "entityId" }
                ),
                resolve: context =>
                {
                    var id = context.GetArgument<string>("entityId");
                    return context.DbContext.Tasks.Where(t => t.TaskRelations.Any(tr => tr.EntityId == id));
                });

            AddQueryConnectionField(
                name: "comments",
                graphType: typeof(TaskCommentGraph),
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "taskId" }
                ),
                resolve: context =>
                {
                    var id = context.GetArgument<Guid>("taskId");
                    return context.DbContext.Comments.Where(t => t.TaskId == id);
                });
        }
    }
}
