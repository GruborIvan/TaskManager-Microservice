using GraphQL.EntityFramework;
using GraphQL.Types;
using TaskManager.Infrastructure.Models;

namespace TaskManager.API.Models.GraphQL
{
    public class TaskCommentGraph : EfObjectGraphType<TasksDbContext, CommentDbo>
    {
        public TaskCommentGraph(IEfGraphQLService<TasksDbContext> efGraphQlService) 
            : base(efGraphQlService)
        {
            Field(tc => tc.CommentId, type: typeof(IdGraphType));
            Field(tc => tc.TaskId, type: typeof(GuidGraphType));
            Field(tc => tc.Text);
            Field(tc => tc.CreatedById, type: typeof(GuidGraphType));
            Field(tc => tc.CreatedDate, type: typeof(DateTimeGraphType));
        }
    }
}
