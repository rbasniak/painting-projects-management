using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials.Integrations;
public static partial class GetMaterialsForProject
{
    public sealed class Validator : AbstractValidator<Abstractions.GetMaterialsForProject.Request>
    {
        // Module to module communication, is validation really necessary here?
    }

    public sealed class Handler : ICommandHandler<Abstractions.GetMaterialsForProject.Request, IReadOnlyCollection<ReadOnlyMaterial>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ReadOnlyMaterial>> HandleAsync(Abstractions.GetMaterialsForProject.Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(x => request.MaterialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
                
            return materials.Select(x => x.MapFromModel()).AsReadOnly();
        }
    }
}
