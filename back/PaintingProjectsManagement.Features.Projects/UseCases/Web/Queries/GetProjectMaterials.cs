using rbkApiModules.Commons.Core.Abstractions;

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
        .Produces<ProjectMaterialDetails[]>(StatusCodes.Status200OK)
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
            var project = await _context.Set<Project>()
                .Include(x => x.Materials)
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            if (!project.Materials.Any())
            {
                return QueryResponse.Success(Array.Empty<ProjectMaterialDetails>());
            }

            var materialIds = project.Materials.Select(m => m.MaterialId).ToArray();

            var materials = await _context.Set<Material>()
                .Where(m => m.Tenant == request.Identity.Tenant && materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            var results = project.Materials
                .Select(pm =>
                {
                    if (!materials.TryGetValue(pm.MaterialId, out var material))
                    {
                        return null;
                    }

                    var pricePerUnit = material.PricePerUnit;
                    var quantity = pm.Quantity;

                    return new ProjectMaterialDetails
                    {
                        MaterialId = pm.MaterialId,
                        MaterialName = material.Name,
                        CategoryName = material.CategoryName,
                        PricePerUnitFormatted = $"{pricePerUnit.Amount:F2} {pricePerUnit.Currency}/{GetUnitDisplayName(quantity.Unit)}",
                        Quantity = quantity.Value,
                        QuantityFormatted = $"{quantity.Value:F2} {GetUnitDisplayName(quantity.Unit)}",
                        Unit = quantity.Unit,
                        UnitDisplayName = GetUnitDisplayName(quantity.Unit)
                    };
                })
                .Where(x => x != null)
                .Cast<ProjectMaterialDetails>()
                .ToList();

            return QueryResponse.Success(results.ToArray());
        }

        private static string GetUnitDisplayName(MaterialUnit unit)
        {
            return unit switch
            {
                MaterialUnit.Drop => "drops",
                MaterialUnit.Unit => "units",
                MaterialUnit.Centimeter => "centimeters",
                MaterialUnit.Meter => "meters",
                MaterialUnit.Gram => "grams",
                MaterialUnit.Kilogram => "kilograms",
                MaterialUnit.Liter => "liters",
                MaterialUnit.Mililiter => "milliliters",
                MaterialUnit.Spray => "sprays",
                _ => unit.ToString().ToLower()
            };
        }
    }
}