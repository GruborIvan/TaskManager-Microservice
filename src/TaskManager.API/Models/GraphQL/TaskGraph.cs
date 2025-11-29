using GraphQL.EntityFramework;
using GraphQL.Types;
using System.Linq;
using TaskManager.Infrastructure.Models;

namespace TaskManager.API.Models.GraphQL
{
    public class TaskGraph : EfObjectGraphType<TasksDbContext, TaskDbo>
    {
        private const string _taskRelationsNavigationName =
            nameof(TaskDbo.TaskRelations);

        private const string _commentsNavigationName =
            nameof(TaskDbo.Comments);

        public TaskGraph(IEfGraphQLService<TasksDbContext> graphQlService) : base(graphQlService)
        {
            Field(t => t.TaskId, type: typeof(IdGraphType));
            Field(t => t.CreatedById, type: typeof(GuidGraphType));
            Field(t => t.ChangedBy, type: typeof(GuidGraphType));
            Field(t => t.Callback);
            Field(t => t.CreatedDate, type: typeof(DateTimeGraphType));
            Field(t => t.ChangedDate, type: typeof(DateTimeGraphType));
            Field(t => t.Data);
            Field(t => t.FinalState);
            Field(t => t.SourceId);
            Field(t => t.SourceName);
            Field(t => t.Subject);
            Field(t => t.Status);
            Field(t => t.TaskType);
            Field(t => t.AssignedToEntityId, type: typeof(GuidGraphType));
            Field(t => t.AssignmentType, nullable: true);


            AddNavigationListField(
                name: "relations",
                resolve: context => context.Source.TaskRelations.Select(cs => cs),
                includeNames: new[] { _taskRelationsNavigationName },
                graphType: typeof(TaskRelationGraph));

            AddNavigationListField(
                name: "comments",
                resolve: context => context.Source.Comments.Select(cs => cs),
                includeNames: new[] { _commentsNavigationName },
                graphType: typeof(TaskCommentGraph));
        }
    }
}
