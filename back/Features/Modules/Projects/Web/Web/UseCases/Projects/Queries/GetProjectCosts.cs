using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectCosts : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects/{projectId}/costs", async (Guid projectId, string? currency, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request
            {
                ProjectId = projectId,
                Currency = currency
            }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectCostDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Project Costs")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
        public Guid ProjectId { get; set; }
        public string? Currency { get; set; }
    }

    internal class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        x => x.Id == id && x.TenantId == request.Identity.Tenant,
                        cancellationToken))
                .WithMessage("Id references a non-existent record.");
        }
    }

    public class Handler(
        DbContext context,
        IProjectCostCalculator projectCostCalculator,
        ILogger<Handler> logger
    ) : IQueryHandler<Request>
    {
        private const string DefaultCurrency = "DKK";

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .AsNoTracking()
                .FirstAsync(
                    x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant,
                    cancellationToken);

            try
            {
                var selectedCurrency = string.IsNullOrWhiteSpace(request.Currency)
                    ? DefaultCurrency
                    : request.Currency.Trim().ToUpperInvariant();

                var projectCostBreakdown = await projectCostCalculator.CalculateCostAsync(project.Id, selectedCurrency, cancellationToken);
                return QueryResponse.Success(ProjectCostDetails.FromModel(projectCostBreakdown));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to calculate project cost for project {ProjectId}. Returning empty cost breakdown.", project.Id);

                // #region agent log
                try
                {
                    System.IO.File.AppendAllText(
                        "/opt/cursor/logs/debug.log",
                        JsonSerializer.Serialize(new
                        {
                            hypothesisId = "H1",
                            location = "GetProjectCosts.cs:Handler",
                            message = ex.GetType().FullName,
                            data = new
                            {
                                ex.Message,
                                inner = ex.InnerException?.Message,
                                projectId = project.Id,
                                request.Currency
                            },
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        }) + "\n");
                }
                catch
                {
                    /* ignore debug log I/O errors */
                }
                // #endregion

                return QueryResponse.Success(new ProjectCostDetails
                {
                    Id = project.Id,
                    Electricity = ProjectCostDetails.Empty.Electricity,
                    Labor = ProjectCostDetails.Empty.Labor,
                    Materials = ProjectCostDetails.Empty.Materials
                });
            }
        }
    }
}
