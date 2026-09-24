namespace PaintingProjectsManagement.Features.Projects;

internal static class ArchivedProjectValidation
{
    public const string ReadOnlyMessage = "Archived projects are read-only and cannot be edited.";

    public static async Task<bool> IsEditableProjectAsync(
        DbContext context,
        string tenantId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await context.Set<Project>()
            .AsNoTracking()
            .Select(x => new { x.Id, x.TenantId, x.EndDate })
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId, cancellationToken);

        return project is null || project.EndDate is null;
    }

    public static async Task<bool> IsEditableByColorGroupAsync(
        DbContext context,
        string tenantId,
        Guid colorGroupId,
        CancellationToken cancellationToken)
    {
        var projectData = await context.Set<ColorGroup>()
            .AsNoTracking()
            .Where(x => x.Id == colorGroupId && x.Project.TenantId == tenantId)
            .Select(x => new { x.Project.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        return projectData is null || projectData.EndDate is null;
    }

    public static async Task<bool> IsEditableBySectionAsync(
        DbContext context,
        string tenantId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var projectData = await context.Set<ColorSection>()
            .AsNoTracking()
            .Where(x => x.Id == sectionId && x.ColorGroup.Project.TenantId == tenantId)
            .Select(x => new { x.ColorGroup.Project.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        return projectData is null || projectData.EndDate is null;
    }
}
