using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureNamingTool.Attributes
{
    /// <summary>
    /// Represents a custom header Swagger attribute.
    /// </summary>
    public class CustomHeaderSwaggerAttribute : IOperationFilter
    {
        /// <summary>
        /// Applies the custom header Swagger attribute to the specified operation.
        /// </summary>
        /// <param name="operation">The OpenApiOperation object representing the operation.</param>
        /// <param name="context">The OperationFilterContext object representing the context of the operation.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "APIKey",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                }
            });
        }
    }
}
