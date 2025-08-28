using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace PaintingProjectsManagement.Features.Models;

public static class ModelsSeeder
{
    public static void SeedFromDisk(DbContext context)
    {
        var path = @"D:\Printing and Painting\3D Models\Figures";

        IReadOnlyCollection<Model> figures = [];    
       
        LoadLibrary(context, path, ModelType.Figure);
    }

    private static void LoadLibrary(DbContext context, string path, ModelType type)
    {
        var results = new List<Model>();

        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(x => !x.Contains("\\_summary\\"))
            .Where(x => !x.Contains("\\_next\\"))
            .Where(x => !x.Contains("\\_store\\"))
            .ToArray();

        var allModels = new List<string>();

        var allCategories = context.Set<ModelCategory>().ToList();

        try
        {
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

                        string franchise = new DirectoryInfo(modelFolderPath).Parent.Name;
                        string artist = folderName;
                        string categoryName = new DirectoryInfo(modelFolderPath).Parent.Parent.Name;
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
                            else if (parts[2].ToLower().Contains(" vs "))
                            {
                                characters = parts[2].Split(" vs ");
                            }
                            else if (parts[2].ToLower().Contains(" vs. "))
                            {
                                characters = parts[2].Split(" vs ");
                            }
                            else
                            {
                                characters = [parts[2]];
                            }
                        }
                        else
                        {
                            Debugger.Break();
                        }

                        var category = allCategories.FirstOrDefault(x => x.Name == categoryName);

                        if (category == null)
                        {
                            category = new ModelCategory("rodrigo.basniak", categoryName);

                            allCategories.Add(category);

                            context.Add(category);
                        }

                        var model = new Model(
                            tenant: "rodrigo.basniak",
                            name: character,
                            category: category,
                            characters: characters,
                            franchise: franchise,
                            type: type,
                            artist: artist,
                            tags: [],
                            baseSize: BaseSize.Unknown,
                            figureSize: FigureSize.Unknown,
                            numberOfFigures: 0,
                            sizeInMb: totalSizeInMb);

                        results.Add(model);

                        var basePath = new DirectoryInfo(".");

                        var wwwroot = Path.Combine(basePath.FullName, "wwwroot");

                        var destination = Path.Combine(wwwroot, "uploads", "rodrigo.basniak", "models", Path.GetFileNameWithoutExtension(file) + Guid.NewGuid().ToString("N") + Path.GetExtension(file));

                        var destinationPAth = Path.GetDirectoryName(destination);
                        Directory.CreateDirectory(destinationPAth);

                        File.Copy(file, destination, true);

                        var url = Path.GetRelativePath(wwwroot, destination);

                        model.AddPicture(url);
                        model.UpdateCoverPicture(url);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
            throw;
        }

        context.AddRange(results);
        context.SaveChanges();
    }

    private static string GetFranchise(string franchise)
    {
        return "Unknown";
    }
    
    public static void GenerateSqlSeedFile(DbContext context)
    {
        var basePath = new DirectoryInfo(".");

        var seedDirectory = Path.Combine(basePath.Parent.FullName, "PaintingProjectsManagment.Database",  "Seed");
        Directory.CreateDirectory(seedDirectory);
        
        var sqlFilePath = Path.Combine(seedDirectory, "models_seed.sql");
        
        var sqlBuilder = new StringBuilder();
        
        // Get all categories and models
        var categories = context.Set<ModelCategory>().ToList();
        var models = context.Set<Model>().ToList();
        
        // Generate SQL for categories
        foreach (var category in categories)
        {
            sqlBuilder.AppendLine($@"INSERT INTO public.""models.categories""(""Id"", ""Name"", ""TenantId"") VALUES ('{category.Id}', '{category.Name.Replace("'", "''")}', '{category.TenantId}');");
        }
        
        // Generate SQL for models
        foreach (var model in models)
        {
            var charactersJson = System.Text.Json.JsonSerializer.Serialize(model.Characters);
            var tagsJson = System.Text.Json.JsonSerializer.Serialize(model.Tags);
            var picturesJson = System.Text.Json.JsonSerializer.Serialize(model.Pictures);
            
            // Build the SQL string manually to avoid C# string interpolation issues with backslashes
            var sql = $@"INSERT INTO public.""models.models""(""Id"", ""Name"", ""Franchise"", ""Characters"", ""CategoryId"", ""Type"", ""Artist"", ""Tags"", ""CoverPicture"", ""Pictures"", ""Score"", ""BaseSize"", ""FigureSize"", ""NumberOfFigures"", ""SizeInMb"", ""MustHave"", ""TenantId"") VALUES ('{model.Id}', '{model.Name.Replace("'", "''")}', '{model.Franchise.Replace("'", "''")}', '{charactersJson.Replace("'", "''")}', '{model.CategoryId}', {(int)model.Type}, '{model.Artist.Replace("'", "''")}', '{tagsJson.Replace("'", "''")}', ";
            
            // Handle CoverPicture with dollar-quoted string
            if (model.CoverPicture != null)
            {
                sql += $"$${model.CoverPicture}$$, ";
            }
            else
            {
                sql += "NULL, ";
            }
            
            // Handle Pictures with dollar-quoted string
            sql += $"$${picturesJson}$$, ";
            
            // Add the rest of the values
            sql += $"{model.Score.Value}, {(int)model.BaseSize}, {(int)model.FigureSize}, {model.NumberOfFigures}, {model.SizeInMb}, {(model.MustHave ? "true" : "false")}, '{model.TenantId}');";
            
            sqlBuilder.AppendLine(sql);
        }
        
        File.WriteAllText(sqlFilePath, sqlBuilder.ToString());
    }
}
