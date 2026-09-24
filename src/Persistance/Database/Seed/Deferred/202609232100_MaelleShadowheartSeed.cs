using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Inventory;
using PaintingProjectsManagement.Features.Inventory.Integration;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Commons.Relational;

namespace PaintingProjectsManagment.Database;

public sealed class MaelleShadowheartSeed : IDeferredSeedStep
{
    private const string TenantId = "RODRIGO.BASNIAK";
    private const string SeedFileName = "maelle_shadowheart_seed.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Id => "2026-09-23 21h00m: Seed Maelle and Shadowheart projects";

    public EnvironmentUsage EnvironmentUsage => EnvironmentUsage.Production
        | EnvironmentUsage.Development
        | EnvironmentUsage.Staging
        | EnvironmentUsage.Testing;

    public Type DbContextType => typeof(DatabaseContext);

    public async Task ExecuteAsync(DbContext context, IServiceProvider serviceProvider)
    {
        var seedFile = FindSeedFile();
        var projects = JsonSerializer.Deserialize<SeedProject[]>(
            await File.ReadAllTextAsync(seedFile),
            JsonOptions)
            ?? Array.Empty<SeedProject>();
        var paintColors = await context.Set<PaintColor>()
            .Include(x => x.Line)
            .ThenInclude(x => x.Brand)
            .ToListAsync();

        foreach (var data in projects)
        {
            var alreadySeeded = await context.Set<Project>()
                .AnyAsync(x => x.TenantId == TenantId && x.Name == data.Name);
            if (alreadySeeded)
            {
                continue;
            }

            var project = new Project(
                TenantId,
                data.Name,
                data.StartDate,
                modelId: null);

            project.UpdateCoverPicture(data.PictureUrl);
            foreach (var reference in data.References)
            {
                project.AddReferencePicture(reference);
            }

            context.Add(project);

            foreach (var groupData in data.Groups)
            {
                var group = new ColorGroup(project, groupData.Name);
                context.Add(group);
                project.AddColorGroup(group);

                foreach (var sectionData in groupData.Sections)
                {
                    group.AddSection(
                        (ColorZone)sectionData.Zone,
                        sectionData.ReferenceColor);

                    var section = group.Sections.Single(x => x.Zone == (ColorZone)sectionData.Zone);
                    if (sectionData.UsedColor is not null)
                    {
                        section.SetPickedColor(ResolvePaintColorId(sectionData.UsedColor, paintColors));
                    }

                    var suggestedColors = sectionData.SuggestedColors
                        .Deserialize<ColorMatchResult[]>(JsonOptions) ?? Array.Empty<ColorMatchResult>();
                    foreach (var suggestedColor in suggestedColors)
                    {
                        suggestedColor.PaintColorId = ResolvePaintColorId(
                            new SeedPaint
                            {
                                Name = suggestedColor.Name,
                                LineName = suggestedColor.LineName,
                                BrandName = suggestedColor.BrandName
                            },
                            paintColors);
                    }

                    if (suggestedColors.Length > 0)
                    {
                        section.UpdateSuggestedColors(suggestedColors);
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static Guid ResolvePaintColorId(SeedPaint paint, IReadOnlyCollection<PaintColor> paintColors)
    {
        var match = paintColors.SingleOrDefault(x =>
            x.Name == paint.Name
            && x.Line.Name == paint.LineName
            && x.Line.Brand.Name == paint.BrandName);

        return match?.Id
            ?? throw new InvalidOperationException(
                $"Could not resolve paint '{paint.Name}' ({paint.BrandName} / {paint.LineName}) while seeding projects.");
    }

    private static string FindSeedFile()
    {
        var root = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (root is not null)
        {
            var candidate = Path.Combine(root.FullName, "Persistance", "Database", "Seed", SeedFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            root = root.Parent;
        }

        throw new FileNotFoundException($"Could not find seed file: {SeedFileName}");
    }

    private sealed class SeedProject
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public string[] References { get; set; } = Array.Empty<string>();
        public SeedGroup[] Groups { get; set; } = Array.Empty<SeedGroup>();
    }

    private sealed class SeedGroup
    {
        public string Name { get; set; } = string.Empty;
        public SeedSection[] Sections { get; set; } = Array.Empty<SeedSection>();
    }

    private sealed class SeedSection
    {
        public int Zone { get; set; }
        public string ReferenceColor { get; set; } = string.Empty;
        public SeedPaint? UsedColor { get; set; }
        public JsonElement SuggestedColors { get; set; }
    }

    private sealed class SeedPaint
    {
        public string Name { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
    }
}
