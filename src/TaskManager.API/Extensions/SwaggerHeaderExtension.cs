using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TaskManager.API.Extensions
{
    public class SwaggerHeaderExtension : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            if (context.ApiDescription.ActionDescriptor.DisplayName.Equals("TaskManager.API.Controllers.TasksController.GetTaskById (TaskManager.API)"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "x-user-id",
                    In = ParameterLocation.Header,
                    Required = true,
                    Example = new OpenApiString("x-user-id")
                });
            }
        }
    }
}