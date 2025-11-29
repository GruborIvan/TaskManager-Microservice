using GraphQL;
using GraphQL.Types;

namespace TaskManager.API.Models.GraphQL
{
    public class GraphQlSchema : Schema
    {
        public GraphQlSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<GraphQlQuery>();
        }
    }
}
