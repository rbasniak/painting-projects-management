namespace PaintingProjectsManagement.Features.Materials.Abstractions;

public static partial class GetMaterialsForProject
{
    public sealed class Request : IQuery
    {
        public Guid[] MaterialIds { get; set; } = Array.Empty<Guid>();
    }
}

