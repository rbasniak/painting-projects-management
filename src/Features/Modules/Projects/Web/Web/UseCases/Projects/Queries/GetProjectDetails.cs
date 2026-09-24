using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectDetails : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects/{projectId}", async (Guid projectId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request
            {
                Id = projectId
            }, cancellationToken);
            
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Project Details")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
        public Guid Id { get; set; }
    }

    internal class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Id)
                .MustAsync(async (request, id, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        x => x.Id == id && x.TenantId == request.Identity.Tenant,
                        cancellationToken))
                .WithMessage("Id references a non-existent record.");
        }
    }

    public class Handler(DbContext context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .Include(x => x.Steps)
                .Include(x => x.References)
                .Include(x => x.Pictures)
                .Include(x => x.ColorGroups)
                    .ThenInclude(x => x.Sections)
                .FirstAsync(
                    x => x.Id == request.Id && x.TenantId == request.Identity.Tenant,
                    cancellationToken);

            return QueryResponse.Success(ProjectDetails.FromModel(project));
        }
    }
}
