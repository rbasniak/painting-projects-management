using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace PaintingProjectsManagement.Features.Models;

public static class ModelsSeeder
{
    private static List<ModelCategory> _categories = [];
    public static void SeedFromDisk(DbContext context)
    {
        var path = @"D:\Printing and Painting\3D Models\Figures";

        IReadOnlyCollection<Model> figures = [];    
        if (Environment.MachineName == "RB-DESKTOP1")
        {
            figures = LoadLibrary(path, ModelType.Figure);
            File.WriteAllText("models.seeddata", JsonSerializer.Serialize(figures));
        }

        context.AddRange(figures);
        context.SaveChanges();
    }

    private static IReadOnlyList<Model> LoadLibrary(string path, ModelType type)
    {
        var results = new List<Model>();

        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(x => !x.Contains("\\_summary\\"))
            .Where(x => !x.Contains("\\_next\\"))
            .Where(x => !x.Contains("\\_store\\"))
            .ToArray();

        var allModels = new List<string>();
        foreach (var file in files)
        {
            if (file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".jpeg"))
            {
                var preview = Path.GetRelativePath(path, file);
                var folderPath = Path.GetDirectoryName(file);
                var folderName = Path.GetFileNameWithoutExtension(file);
                var modelFolderPath = Path.Combine(folderPath, folderName);

                if (Directory.Exists(modelFolderPath))
                {
                    if (file.ToLower().Contains("\\stl\\"))
                    {
                        continue;
                    }

                    var imageFiles = Directory.GetFiles(modelFolderPath, "*.*", SearchOption.AllDirectories)
                        .Where(f => f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg") || f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".gif") || f.EndsWith(".webp"))
                        .Select(f => Path.GetRelativePath(path, f).ToLower())
                        .ToArray();

                    var stlFiles = Directory.GetFiles(modelFolderPath, "*.stl", SearchOption.AllDirectories)
                        .Select(f => Path.GetRelativePath(path, f).ToLower())
                        .ToArray();

                    // Calculate total size of all STL files in megabytes
                    var totalSizeInBytes = 0L;
                    foreach (var stlFile in stlFiles)
                    {
                        var fullPath = Path.Combine(path, stlFile);
                        if (File.Exists(fullPath))
                        {
                            var fileInfo = new FileInfo(fullPath);
                            totalSizeInBytes += fileInfo.Length;
                        }
                    }
                    var totalSizeInMb = (int)(totalSizeInBytes / (1024 * 1024)); // Convert bytes to MB

                    var character = folderPath.Split(Path.DirectorySeparatorChar).Last();

                    var parts = preview.Split('\\');

                    string franchise = "Unknown";
                    string artist = "Unknown";
                    string categoryName = "Unknown";
                    string[] characters = [];

                    if (parts.Length == 3)
                    {
                        categoryName = parts[0];
                        artist = parts[2].Split('.').First();
                        franchise = parts[1];
                    }
                    else if (parts.Length == 4)
                    {
                        categoryName = parts[0];
                        artist = parts[3].Split('.').First();
                        franchise = parts[1];

                        if (parts[2].ToLower().Contains(" and "))
                        {
                            characters = parts[2].Split(" and ");
                        }
                        else if (parts[2].ToLower().Contains(" & "))
                        {
                            characters = parts[2].Split(" & ");
                        }
                        else if (parts[2].ToLower().Contains(" vs ") )
                        {
                            characters = parts[2].Split(" vs ");
                        }
                        else if (parts[2].ToLower().Contains(" vs. "))
                        {
                            characters = parts[2].Split(" vs ");
                        }
                        else
                        {
                            characters = [ parts[2] ];
                        }
                    }
                    else
                    {
                        Debugger.Break();
                    }

                    var category = _categories.FirstOrDefault(x => x.Name == categoryName);

                    if (category == null)
                    {
                        category = new ModelCategory("rodrigo.basniak", categoryName);
                        
                        _categories.Add(category);
                    }

                    allModels.Add(character); // TODO: check unique names

                    results.Add(new Model(
                        tenant: "rodrigo.basniak",
                        name: character,
                        category: category,
                        franchise: GetFranchise(franchise),
                        type: type,
                        artist: artist,
                        tags: [],
                        baseSize: BaseSize.Unknown,
                        figureSize: FigureSize.Unknown,
                        numberOfFigures: -1,
                        size: totalSizeInMb));
                }
            }
        }

        return results.AsReadOnly();
    }

    private static string GetFranchise(string franchise)
    {
        return "Unknown";
    }
}
