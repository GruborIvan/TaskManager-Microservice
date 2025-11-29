using GraphQL.EntityFramework;
using GraphQL.Types;
using TaskManager.Infrastructure.Models;

namespace TaskManager.API.Models.GraphQL
{
    public class TaskRelationGraph : EfObjectGraphType<TasksDbContext, TaskRelationDbo>
    {
        public TaskRelationGraph(IEfGraphQLService<TasksDbContext> graphQlService)
            : base(graphQlService)
        {
            Field("Id", tr => tr.RelationId, type: typeof(IdGraphType));
            Field(tr => tr.TaskId);
            Field(tr => tr.EntityId);
            Field(tr => tr.EntityType);
            Field(tr => tr.IsMain);
        }
    }
}
