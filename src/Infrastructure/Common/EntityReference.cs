namespace PaintingProjectsManagement.Infrastructure.Common;

public record EntityReference(Guid Id, string Name);

public record EntityReference<T>(T Id, string Name);
