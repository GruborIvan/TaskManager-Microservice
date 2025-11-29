using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using TaskManager.API.Exceptions;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphQlController : Controller
    {
        private readonly IDocumentExecuter _executer;
        private readonly ISchema _schema;

        public GraphQlController(ISchema schema, IDocumentExecuter executer)
        {
            _schema = schema;
            _executer = executer;
        }

        [HttpPost]
        public Task<ExecutionResult> Post(
            [BindRequired, FromBody] PostBody body,
            CancellationToken cancellation)
        {
            return Execute(body.Query, body.OperationName, body.Variables, cancellation);
        }

        public class PostBody
        {
            public string? OperationName { get; set; }
            public string Query { get; set; } = null!;
            public JObject? Variables { get; set; }
        }

        [HttpGet]
        public Task<ExecutionResult> Get(
            [FromQuery] string query,
            [FromQuery] string? variables,
            [FromQuery] string? operationName,
            CancellationToken cancellation)
        {
            var jObject = ParseVariables(variables);
            return Execute(query, operationName, jObject, cancellation);
        }

        Task<ExecutionResult> Execute(string query,
            string? operationName,
            JObject? variables,
            CancellationToken cancellation)
        {
            var options = new ExecutionOptions
            {
                Schema = _schema,
                Query = query,
                OperationName = operationName,
                Inputs = variables?.ToInputs(),
                CancellationToken = cancellation,
#if (DEBUG)
                ExposeExceptions = true,
                EnableMetrics = true,
#endif
            };

            try
            {
                return _executer.ExecuteAsync(options);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        static JObject? ParseVariables(string? variables)
        {
            if (variables == null)
            {
                return null;
            }

            try
            {
                return JObject.Parse(variables);
            }
            catch (Exception exception)
            {
                throw new GraphQlException("Could not parse variables.", exception);
            }
        }
    }
}