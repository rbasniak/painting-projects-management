using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials.Integrations;
public static partial class GetMaterialsForProject
{
    public sealed class Validator : AbstractValidator<Abstractions.GetMaterialsForProject.Request>
    {
        // Module to module communication, is validation really necessary here?
    }

    public sealed class Handler(DbContext _context) : IQueryHandler<Abstractions.GetMaterialsForProject.Request, IReadOnlyCollection<ReadOnlyMaterial>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>> HandleAsync(Abstractions.GetMaterialsForProject.Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(x => request.MaterialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
                
            return QueryResponse.Success(materials.Select(x => x.MapFromModel()).AsReadOnly());
        }
    }
}
