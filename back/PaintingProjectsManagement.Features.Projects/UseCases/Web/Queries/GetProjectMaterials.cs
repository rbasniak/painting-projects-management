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
                        CategoryId = material.CategoryId,
                        Quantity = quantity.Value,
                        QuantityFormatted = GetFormatedQuantity(pm.Quantity),
                        Unit = quantity.Unit,
                    };
                })
                .Where(x => x is not null)
                .OrderBy(x => x!.CategoryId)
                .ThenBy(x => x!.MaterialName)
                .Cast<ProjectMaterialDetails>()
                .ToList();

            return QueryResponse.Success(results.ToArray());
        }

        private static string GetFormatedQuantity(Quantity quantity)
        {
            switch (quantity.Unit)
            {
                case MaterialUnit.Meter:
                case MaterialUnit.Kilogram:
                case MaterialUnit.Liter:
                    return $"{quantity.Value:F2} {GetUnitDisplayName(quantity.Unit)}";
                case MaterialUnit.Drop:
                case MaterialUnit.Centimeter:
                case MaterialUnit.Unit:
                case MaterialUnit.Gram:
                case MaterialUnit.Mililiter:
                case MaterialUnit.Spray:
                    return $"{quantity.Value:F0} {GetUnitDisplayName(quantity.Unit)}";
                default:
                    throw new NotImplementedException($"Unknown quantity unit: {quantity.Unit}");
            }
        }

        private static string GetUnitDisplayName(MaterialUnit unit)
        {
            return unit switch
            {
                MaterialUnit.Drop => "drops",
                MaterialUnit.Unit => "units",
                MaterialUnit.Centimeter => "cm",
                MaterialUnit.Meter => "m",
                MaterialUnit.Gram => "g",
                MaterialUnit.Kilogram => "kg",
                MaterialUnit.Liter => "l",
                MaterialUnit.Mililiter => "ml",
                MaterialUnit.Spray => "spray presses",
                _ => throw new NotImplementedException($"Unknown quantity unit: {unit}")
            };
        }
    }
}