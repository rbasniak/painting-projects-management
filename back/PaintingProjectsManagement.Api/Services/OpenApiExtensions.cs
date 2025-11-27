using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PaintingProjectsManagement.Api;

public static class OpenApiExtensions
{
    public static OpenApiOptions CustomSchemaIds(this OpenApiOptions config,
        Func<Type, string?> typeSchemaTransformer,
        bool includeValueTypes = false)
    {
        return config.AddSchemaTransformer(new CustomSchemaTransformer(typeSchemaTransformer, includeValueTypes));
    }
}

public class CustomSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly Func<Type, string?> _typeSchemaTransformer;
    private readonly bool _includeValueTypes;

    public CustomSchemaTransformer(Func<Type, string?> typeSchemaTransformer, bool includeValueTypes)
    {
        _typeSchemaTransformer = typeSchemaTransformer;
        _includeValueTypes = includeValueTypes;
    }

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Skip value types and strings
        if (!_includeValueTypes &&
            (context.JsonTypeInfo.Type.IsValueType ||
             context.JsonTypeInfo.Type == typeof(String) ||
             context.JsonTypeInfo.Type == typeof(string)))
        {
            return Task.CompletedTask;
        }

        // Skip if we already processed this schema (check if Title has been set by us)
        if (!string.IsNullOrEmpty(schema.Title))
        {
            return Task.CompletedTask;
        }

        // transform the typename based on the provided delegate
        string? transformedTypeName = _typeSchemaTransformer(context.JsonTypeInfo.Type);

        if (!string.IsNullOrEmpty(transformedTypeName))
        {
            // Set the schema title for Swagger and Scalar
            schema.Title = transformedTypeName;
        }

        return Task.CompletedTask;
    }
}