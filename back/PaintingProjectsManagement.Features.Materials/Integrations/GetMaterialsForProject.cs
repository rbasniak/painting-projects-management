using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials.Integrations;
public static partial class GetMaterialsForProject
{
    public sealed class Validator : AbstractValidator<Abstractions.GetMaterialsForProject.Request>
    {
        // Module to module communication, is validation really necessary here?
    }

    public sealed class Handler(DbContext _context) : ITypedQueryHandler<Abstractions.GetMaterialsForProject.Request, IReadOnlyCollection<ReadOnlyMaterial>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>> HandleAsync(Abstractions.GetMaterialsForProject.Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(m => m.TenantId == request.Identity.Tenant)
                .Where(x => request.MaterialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var results = materials.Select(x => x.MapFromModel()).AsReadOnly();
            return QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>.Success(results);
        }
    }
}
