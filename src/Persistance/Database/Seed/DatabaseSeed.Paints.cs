using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Inventory;
using System.Text.RegularExpressions;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{
    private void PaintCatalogSeed(DatabaseContext context, IServiceProvider provider)
    {
        string[] sourceFiles = [
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Army_Painter.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/AK.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/AKRC.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Acrilex.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/AppleBarrel.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Army_Painter.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Arteza.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Citadel_Colour.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/GreenStuffWorld.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Humbrol.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Italeri.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Liquitex.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Mig.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/MrHobby.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Revell.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Scale75.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Tamiya.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/TomColor.md",
            "https://github.com/Arcturus5404/miniature-paints/blob/main/paints/Vallejo.md"
        ];

        using var httpClient = new HttpClient();
        var processedPaints = new HashSet<string>();
        var processedPaintNames = new HashSet<string>();

        foreach (var sourceUrl in sourceFiles)
        {
            // Extract brand name from filename
            var fileName = Path.GetFileNameWithoutExtension(new Uri(sourceUrl).LocalPath);
            var brandName = fileName.Replace("_", " ");

            // Convert GitHub URL to raw content URL
            var rawUrl = sourceUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");

            // Download content
            var content = httpClient.GetStringAsync(rawUrl).GetAwaiter().GetResult();

            // Get or create brand
            var brand = context.Set<PaintBrand>().FirstOrDefault(b => b.Name == brandName);
            if (brand == null)
            {
                brand = new PaintBrand(brandName);
                context.Set<PaintBrand>().Add(brand);
                context.SaveChanges();
            }

            // Parse markdown table
            var lines = content.Split('\n');
            var inTable = false;
            var headerProcessed = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Detect table start
                if (trimmedLine.StartsWith("|") && !inTable)
                {
                    inTable = true;
                    continue;
                }

                // Skip separator line (|---|---|---|)
                if (inTable && trimmedLine.Contains("---"))
                {
                    headerProcessed = true;
                    continue;
                }

                // Parse data rows
                if (inTable && headerProcessed && trimmedLine.StartsWith("|"))
                {
                    var columns = trimmedLine.Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .ToArray();

                    if (columns.Length >= 3)
                    {
                        var paintName = columns[0];
                        var lineName = columns[2];
                        var hexColorRaw = columns.Last();

                        // Extract hex color from markdown format: ![#A32F26](https://placehold.co/15x15/A32F26/A32F26.png) `#A32F26`
                        var hexMatch = Regex.Match(hexColorRaw, @"`#([0-9A-Fa-f]{6})`");
                        if (!hexMatch.Success)
                        {
                            // Try alternative format without backticks
                            hexMatch = Regex.Match(hexColorRaw, @"#([0-9A-Fa-f]{6})");
                        }

                        if (!hexMatch.Success || hexMatch.Groups.Count < 2)
                        {
                            continue; // Skip if no valid hex color found
                        }

                        var hexColor = "#" + hexMatch.Groups[1].Value.ToUpper();

                        // Create a unique key to check for duplicates
                        var cacheKey = $"{brandName}|{lineName}|{hexColor}";
                        if (processedPaints.Contains(cacheKey))
                        {
                            // TODO: remove unique index from hex value in the database
                            continue; // Skip duplicate entry
                        }

                        processedPaints.Add(cacheKey);

                        // Get or create line
                        var paintLine = context.Set<PaintLine>()
                            .FirstOrDefault(l => l.BrandId == brand.Id && l.Name == lineName);

                        if (paintLine == null)
                        {
                            paintLine = new PaintLine(brand, lineName);
                            context.Set<PaintLine>().Add(paintLine);
                            context.SaveChanges();
                        }

                        // Create a unique key for paint name within the line
                        var paintNameKey = $"{paintLine.Id}|{paintName}";
                        if (processedPaintNames.Contains(paintNameKey))
                        {
                            continue; // Skip duplicate paint name in the same line
                        }

                        // Check if paint already exists in database
                        var existingPaint = context.Set<PaintColor>()
                            .FirstOrDefault(p => p.LineId == paintLine.Id && p.Name == paintName);

                        if (existingPaint == null)
                        {
                            var paint = new PaintColor(paintLine, paintName, hexColor, 17, PaintType.Opaque);
                            context.Set<PaintColor>().Add(paint);
                            processedPaintNames.Add(paintNameKey);
                        }
                        else
                        {
                            processedPaintNames.Add(paintNameKey);
                        }
                    }
                }

                // Exit table when we hit a non-table line
                if (inTable && headerProcessed && !trimmedLine.StartsWith("|"))
                {
                    inTable = false;
                    headerProcessed = false;
                }
            }

            context.SaveChanges();
        }
    }

    private void UserPaintsSeed(DatabaseContext context, IServiceProvider provider)
    {
        var apFanatics = context.Set<PaintColor>().Include(x => x.Line).Where(x => x.Line.Name.ToLower() == "warpaints fanatic");
        var apAir = context.Set<PaintColor>().Include(x => x.Line).Where(x => x.Line.Name.ToLower() == "warpaints air");
        var apSpeed = context.Set<PaintColor>().Include(x => x.Line).Where(x => x.Line.Name.ToLower() == "speedpaint set");

        foreach (var paint in apFanatics)
        {
            context.Add(new UserPaint("RODRIGO.BASNIAK", paint.Id));
        }

        foreach (var paint in apAir)
        {
            context.Add(new UserPaint("RODRIGO.BASNIAK", paint.Id));
        }

        foreach (var paint in apSpeed)
        {
            context.Add(new UserPaint("RODRIGO.BASNIAK", paint.Id));
        }

        context.SaveChanges();
    }
}