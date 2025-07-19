using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials.Integrations;
public static partial class GetMaterialsForProject
{
    public sealed class Validator : AbstractValidator<Abstractions.GetMaterialsForProject.Request>
    {
        public Validator()
        {
            RuleFor(x => x.Identity.IsAuthenticated)
                .Must(x => x == true)
                .WithMessage("This must be used in an authenticated context.");
        }
    }

    public sealed class Handler(DbContext _context) : ITypedQueryHandler<Abstractions.GetMaterialsForProject.Request, IReadOnlyCollection<ReadOnlyMaterial>>
    {
        public async Task<QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>> HandleAsync(Abstractions.GetMaterialsForProject.Request request, CancellationToken cancellationToken)
        {
            var temp = await _context.Set<Material>().ToListAsync();
            var materials = await _context.Set<Material>()
                .Where(m => m.TenantId == request.Identity.Tenant)
                .Where(x => request.MaterialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var results = materials.Select(x => x.MapFromModel()).AsReadOnly();

            return QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>.Success(results);
        }
    }
}
