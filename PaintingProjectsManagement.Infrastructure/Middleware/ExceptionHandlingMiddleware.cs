using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PaintingProjectsManagement.Infrastructure;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InternalException ex)
        {
            var problem = new ProblemDetails
            {
                Status = ex.StatusCode,
                Title = "A handled error occurred",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (InternalValidationException ex)
        {
            var problem = new ValidationProblemDetails(ex.Summary)
            {
                Status = 400,
                Title = "Request validation failed",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (Exception ex)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "Unexpected server error",
                Instance = context.Request.Path,
            };

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
