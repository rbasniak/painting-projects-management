namespace PaintingProjectsManagement.Features.Materials.Abstractions;

public static partial class GetMaterialsForProject 
{
    public sealed class Request : AuthenticatedRequest, IQuery<IReadOnlyCollection<ReadOnlyMaterial>>
    {
        public Guid[] MaterialIds { get; set; } = Array.Empty<Guid>();
    }
}

