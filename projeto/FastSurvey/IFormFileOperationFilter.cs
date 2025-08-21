using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FASTSURVEY
{
    public class IFormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile));
            if (fileParams.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = {
                                    [fileParams.First().Name] = new OpenApiSchema { Type = "string", Format = "binary" }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
