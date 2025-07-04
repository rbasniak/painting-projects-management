using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace PaintingProjectsManagement.AppHost;

internal static class ResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithSwaggerUI<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("swagger-ui-docs", "Swagger UI Documentation", "swagger");
    }
    public static IResourceBuilder<T> WithScalarUI<T>(this IResourceBuilder<T> builder)
    where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("scalar-ui-docs", "Scalar UI Documentation", "scalar/v1");
    }

    public static IResourceBuilder<T> WithReDoc<T>(this IResourceBuilder<T> builder)
    where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs("redoc-ui-docs", "ReDoc UI Documentation", "api-docs");
    }

    private static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder, string name, string displayName, string relativeUrl)
        where T : IResourceWithEndpoints
    {
        relativeUrl = relativeUrl.TrimStart('/');

        return builder.WithCommand(name, displayName,
            executeCommand: async _ =>
            {
                ExecuteCommandResult result;

                try
                {
                    var baseUrl = builder.GetEndpoint("https").Url;

                    var url = $"{baseUrl}/{relativeUrl}";

                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    });

                    result = new ExecuteCommandResult { Success = true };
                }
                catch (Exception ex)
                {
                    result = new ExecuteCommandResult { Success = false, ErrorMessage = ex.ToString() };
                }

                return await Task.FromResult(result);
            },
            updateState: context =>
            {
                if (context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy)
                {
                    return ResourceCommandState.Enabled;
                }
                else
                {
                    return ResourceCommandState.Disabled;
                }
            });
    }
}
