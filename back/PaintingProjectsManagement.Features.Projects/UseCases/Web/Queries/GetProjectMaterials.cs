namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects/{projectId}/execution/materials", async (Guid projectId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { ProjectId = projectId }, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<ProjectMaterialDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Project Materials")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
        public Guid ProjectId { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(p => p.Id == id && p.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var projectMaterials = await _context.Set<MaterialForProject>()
                .Where(m => m.ProjectId == request.ProjectId)
                .Join(
                    _context.Set<Material>().Where(m => m.Tenant == request.Identity.Tenant),
                    pm => pm.MaterialId,
                    m => m.Id,
                    (pm, m) => new { ProjectMaterial = pm, Material = m }
                )
                .ToListAsync(cancellationToken);

            var result = projectMaterials.Select(x =>
            {
                var pm = x.ProjectMaterial;
                var m = x.Material;
                return new ProjectMaterialDetails
                {
                    MaterialId = m.Id,
                    MaterialName = m.Name,
                    CategoryName = m.CategoryName,
                    PricePerUnit = $"{m.PricePerUnit.Amount:F2} {m.PricePerUnit.CurrencyCode}/{m.Unit}",
                    Quantity = $"{pm.Quantity.Value} {pm.Quantity.Unit.ToString().ToLower()}",
                    Unit = m.Unit
                };
            }).ToList().AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}
