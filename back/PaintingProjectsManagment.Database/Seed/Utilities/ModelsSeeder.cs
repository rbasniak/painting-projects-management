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
            .Order()
            .ToArray();

        var allCategories = context.Set<ModelCategory>().ToList();

        var allPreviews = new List<string>();
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

                        allPreviews.Add(file);

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

                        var info = ExtractInfoFromPath(preview);

                        var category = allCategories.FirstOrDefault(x => x.Name == info.CategoryName);

                        if (category == null)
                        {
                            category = new ModelCategory("rodrigo.basniak", info.CategoryName);

                            allCategories.Add(category);

                            context.Add(category);
                        }

                        var model = new Model(
                            tenant: "rodrigo.basniak",
                            name: info.Name,
                            category: category,
                            characters: info.Characters,
                            franchise: info.Franchise,
                            type: type,
                            artist: info.Artist,
                            tags: info.Tags,
                            baseSize: BaseSize.Medium,
                            figureSize: FigureSize.Normal,
                            numberOfFigures: 1,
                            sizeInMb: totalSizeInMb,
                            identity: $"[{type}] {Path.GetRelativePath(path, file)}");

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

    private static (string CategoryName, string Name, string Artist, string Franchise, string[] Characters, string[] Tags) ExtractInfoFromPath(string filename)
    {
        string name = "";
        string artist = "";
        string franchise = "";
        string categoryName = "";
        List<string> characters = [];
        List<string> tags = [];

        var file = new FileInfo(filename);

        if (filename.Contains("Animated Series"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Animated Series";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Pink and Brain"))
            {
                characters = ["Pink and Brain"];
            }
            else if (filename.Contains("Dungeons and Dragons\\Heroes"))
            {
                name = "Dungeons and Dragons";
                characters = ["Dungeons and Dragons"];
            }
            else if (filename.Contains("coyote-and-sonic"))
            {
                name = "Coyote and Sonic Diorama";
                characters = ["Sonic", "Coyote"];
                franchise = "Looney Tunes";
            }
            else if (filename.Contains("Looney Tunes\\dc"))
            {
                name = "Looney Tunes DC Comics Diorama";
                characters = ["Looney Tunes"];
                franchise = "Looney Tunes";
            }
            else if (filename.Contains("Looney Tunes\\diorama"))
            {
                name = "Looney Tunes Diorama";
                characters = ["Looney Tunes"];
                franchise = "Looney Tunes";
            }
            else if (filename.Contains("Pink Panther\\fanart"))
            {
                name = "Pink Panther";
                characters = ["Pink Panther"];
                franchise = "Pink Panther";
            }
            else if (filename.Contains("The Phantom"))
            {
                franchise = "The Phantom";
            }
            else if (filename.Contains("TMNT\\Diorama"))
            {
                name = "TMNT Diorama";
                characters = ["Donatello", "Leonardo", "Michelangelo", "Raphael"];
                franchise = "TMNT";
            }
        }
        else if (filename.Contains("DC Comics"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = "DC Comics";
            categoryName = "Comics";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Harley Queen, Poison Ivy and Catwoman"))
            {
                characters = ["Harley Quinn", "Poison Ivy", "Cat Woman"];
            }
        }
        else if (filename.Contains("Disney"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Name;
            categoryName = "Disney";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Alice"))
            {
                franchise = "Alice in Wonderland";
            }
            else if (filename.Contains("Belle"))
            {
                franchise = "Belle and the Beast";
            }
            else if (filename.Contains("Hades"))
            {
                franchise = "Hercules";
            }
            else if (filename.Contains("Kida"))
            {
                franchise = "Atlantis";
            }
            else if (filename.Contains("Tinkerbell"))
            {
                franchise = "Peter Pan";
            }
            else if (filename.Contains("Stitch"))
            {
                franchise = "Lilo & Stitch";
            }
            else if (filename.Contains("Gravity Falls"))
            {
                franchise = "Gravity Falls";
            }
            else if (filename.Contains("Frozen"))
            {
                name = "Elsa and Olaf Diorama";
                franchise = "Frozen";
                characters = ["Elsa", "Olaf"];
            }
        }
        else if (filename.Contains("Marvel Comics"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Comics";
            characters = BreakName(name);
            tags = ["Marvel Comics"];

            if (filename.Contains("Daredevil"))
            {
                franchise = "Daredevil";
            }
            else if (filename.Contains("Elektra"))
            {
                franchise = "Daredevil";
            }
            else if (filename.Contains("Kingpin"))
            {
                franchise = "Daredevil";
            }
            else if (filename.Contains("Iron Fist"))
            {
                franchise = "Defenders";
            }
            else if (filename.Contains("Luke Cage"))
            {
                franchise = "Defenders";
            }
            else if (filename.Contains("Moon Knight"))
            {
                franchise = "Moon Knight";
            }
            else if (filename.Contains("Punisher"))
            {
                franchise = "The Punisher";
            }
            else if (filename.Contains("Others\\Black Knight"))
            {
                franchise = "Avengers";
            }
            else if (filename.Contains("Others\\Cloak and Dagger"))
            {
                franchise = "Spider-Man";
            }
            else if (filename.Contains("Others\\Ghost Rider"))
            {
                franchise = "Defenders";
            }
            else if (filename.Contains("Others\\Namor"))
            {
                franchise = "Fantastic Four";
            }
            else if (filename.Contains("Others\\The Watcher"))
            {
                franchise = "Avengers";
            }
            else if (filename.Contains("Spiderverse"))
            {
                franchise = "Spider-Man";
            }
            else if (filename.Contains("Avengers\\Galactus"))
            {
                franchise = "Fantastic Four";
            }
            else if (filename.Contains("Scarlet Witch and Dr Strange"))
            {
                characters = ["Scarlet Witch", "Doctor Strange"];
            }
            else if (filename.Contains("Fantastic Four\\Fantastic Four"))
            {
                characters = ["Sue Storm", "Reed Richards", "The Thing", "Johny Storm"];
            }
            else if (filename.Contains("Stan Lee"))
            {
                characters = ["Spider-Man"];
            }
            else if (filename.Contains("Apocalypse and Four Horseman"))
            {
                characters = ["Archangel", "Apocalypse", "Famine", "Pestilence", "War"];
            }
            else if (filename.Contains("Brotherhood of Evil Mutants"))
            {
                characters = ["Avalanche", "Pyro", "Blob", "Toad", "Sabretooth", "Mystique"];
            }
            else if (filename.Contains("Magneto Family"))
            {
                characters = ["Scarlet Witch", "Magneto", "Mercury"];
            }
        }
        else if (filename.Contains("Miscelaneous"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Miscelaneous";
            characters = BreakName(name);
            tags = [];
        } 
        else if (filename.StartsWith("Fantasy\\"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Mithology";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Afrodite") || filename.Contains("Apolo") || filename.Contains("Ares") ||
                filename.Contains("Artemis") || filename.Contains("Athena") || filename.Contains("Hades") ||
                filename.Contains("Hercules") || filename.Contains("Medusa") || filename.Contains("Persephone") ||
                filename.Contains("Perseus") || filename.Contains("Poseidon") || filename.Contains("Zeus") ||
                filename.Contains("Centaur") || filename.Contains("Leonidas"))
            {
                tags = ["Greek mithology"];
            }
            else if (filename.Contains("Anubis"))
            {
                tags = ["Egyptian mithology"];
            }
            else if (filename.Contains("Valkyrie"))
            {
                tags = ["Nordic mithology"];
            }
        }
        else if (filename.Contains("Movies"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Movies";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Ghostbusters\\Ghostbusters"))
            {
                name = "Ghostbusters Diorama";
                characters = ["Dr Peter Venkman", "Dr. Raymond Stantz", "Dr Egon Spengler", "Dr Winston Zeddemore"];
            }
            else if (filename.Contains("Gremlims"))
            {
                name = "Gremlims Diorama";
                franchise = "Gremlims";
            }
            else if (filename.Contains("Indiana Jones"))
            {
                franchise = "Indiana Jones";
            }
            else if (filename.Contains("Neo, Trinity and Morpheus"))
            {
                characters = ["Neo", "Trinity", "Morpheus"];
            }
            else if (filename.Contains("Movies\\Predator"))
            {
                franchise = "Predator";
            }
            else if (filename.Contains("Movies\\Robocop"))
            {
                franchise = "Robocop";
            }
            else if (filename.Contains("Dagobah"))
            {
                name = "Dagobah Diorama";
                characters = ["Yoda", "Luke Skywalker"];
            }
            else if (filename.Contains("Ewoks"))
            {
                name = "Ewoks Diorama";
                characters = ["Ewok"];
            }
            else if (filename.Contains("Movies\\The Crow"))
            {
                franchise = "The Crow";
            }
        }
        else if (filename.Contains("Other Comics"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Comics";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Hellboy"))
            {
                franchise = "Hellboy";
            }
            else if (filename.Contains("Aiolia"))
            {
                name = "Aiolia de Leão";
                characters = [name];
            }
            else if (filename.Contains("Andromeda"))
            {
                name = "Shun de Andromeda";
                characters = [name];
            }
            else if (filename.Contains("Camus"))
            {
                name = "Camus de Aquário";
                characters = [name];
            }
            else if (filename.Contains("Aries"))
            {
                name = "Mu de Aries";
                characters = [name];
            }
            else if (filename.Contains("Athena"))
            {
                name = "Deusa Athena";
                characters = [name];
            }
            else if (filename.Contains("Deahmask"))
            {
                name = "Máscara da Morte";
                characters = [name];
            }
            else if (filename.Contains("Shura"))
            {
                name = "Shura de Capricórnio";
                characters = [name];
            }
            else if (filename.Contains("Cisne"))
            {
                name = "Hioga de Cisne";
                characters = [name];
            }
            else if (filename.Contains("Libra"))
            {
                name = "Dohko de Libra";
                characters = [name];
            }
            else if (filename.Contains("Shiryu"))
            {
                name = "Shiryu de Dragão";
                characters = [name];
            }
            else if (filename.Contains("Marin"))
            {
                name = "Marin de Águia";
                characters = [name];
            }
            else if (filename.Contains("Saga"))
            {
                name = "Saga de Gêmeos";
                characters = [name];
            }
            else if (filename.Contains("Aioria"))
            {
                name = "Aiolia de Leão";
                characters = [name];
            }
            else if (filename.Contains("Scorpio"))
            {
                name = "Milo de Escorpião";
                characters = [name];
            }
            else if (filename.Contains("Peagasus"))
            {
                name = "Seiya de Pegasus";
                characters = [name];
            }
            else if (filename.Contains("Phoenix"))
            {
                name = "Ikki de Phoenix";
                characters = [name];
            }
            else if (filename.Contains("Albafica"))
            {
                name = "Albafica de Peixes";
                characters = [name];
            }
            else if (filename.Contains("Aphrodite"))
            {
                name = "Afrodite de Peixes";
                characters = [name];
            }
            else if (filename.Contains("Sisifo"))
            {
                name = "Sisifo de Sagitário";
                characters = [name];
            }
            else if (filename.Contains("Aiolos"))
            {
                name = "Aiolos de Sagitário";
                characters = [name];
            }
            else if (filename.Contains("Kanon"))
            {
                name = "Kanon de Cavalo Marinho";
                characters = [name];
            }
            else if (filename.Contains("Shaina"))
            {
                name = "Shina de Cobra";
                characters = [name];
            }
            else if (filename.Contains("Aldebaran"))
            {
                name = "Aldebaran de Touro";
                characters = [name];
            }
            else if (filename.Contains("Shaka"))
            {
                name = "Shaka de Virgem";
                characters = [name];
            }
        }
        else if (filename.Contains("TV Shows"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "TV Shows";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Buffy"))
            {
                franchise = "Buffy the Vampire Slayer";
            }
            else if (filename.Contains("Japanese"))
            {
                franchise = file.Directory.Name;
            }
            else if (filename.Contains("Supernatural"))
            {
                franchise = "Supernatural";
                name = "Supernatural Diorama";
                characters = ["Sam Whinchester", "Dean Whinchester"];
            }
        }
        else if (filename.Contains("Videogames"))
        {
            name = file.Directory.Name;
            artist = NormalizeArtist(Path.GetFileNameWithoutExtension(file.FullName));
            franchise = file.Directory.Parent.Name;
            categoryName = "Videogames";
            characters = BreakName(name);
            tags = [];

            if (filename.Contains("Assassins Creed"))
            {
                franchise = "Assassin's Creed";
            }
            else if (filename.Contains("Cuphead"))
            {
                franchise = "Cuphead";
            }
            else if (filename.Contains("Final Fantasy\\Diorama"))
            {
                name = "Final Fantasy Diorama";
                characters = ["Aerith", "Tifa", "Cloud Strife"];
            }
            else if (filename.Contains("Helldivers"))
            {
                franchise = "Helldivers";
                name = "Helldiver";
                characters = [name];
            }
            else if (filename.Contains("Zelda and the Guardians"))
            {
                characters= ["Zelda"];
            }  
        }

        if (franchise == "TMNT")
        {
            franchise = "Teenage Mutant Ninja Turtles";
        }
        if (franchise == "Mario")
        {
            franchise = "Super Mario Bros";
        }

        return (categoryName, name, artist, franchise, characters.ToArray(), tags.ToArray());
    }

    private static List<string> BreakName(string name)
    {
        if (name.ToLower().Contains(" and "))
        {
            return name.Split(" and ").ToList();
        }
        else if (name.ToLower().Contains(" & "))
        {
            return name.Split(" & ").ToList();
        }
        else if (name.ToLower().Contains(" vs "))
        {
            return name.Split(" vs ").ToList();
        }
        else if (name.ToLower().Contains(" vs. "))
        {
            return name.Split(" vs. ").ToList();
        }
        else
        {
            return [name];
        }
    }

    private static string NormalizeArtist(string artist)
    {
        switch (artist.ToLower())
        {
            case "3d-mind": return "3D Mind";
            case "3d-verse": return "3dverse";
            case "3dmoon": return "3Dmoonn";
            case "3dmoonn": return "3Dmoonn";
            case "3dsqulpts": return "3DSQULPTS";
            case "3dverse": return "3dverse";
            case "3dxm": return "3DXM";
            case "abe3d": return "Abe3D";
            case "action-studios-3d": return "Action Studios 3D";
            case "aiolos-1": return "Unknown";
            case "aioria-1": return "Unknown";
            case "aioria-2": return "Unknown";
            case "akshay-jay": return "Akshay Jay";
            case "albafica-1": return "Unknown";
            case "aldebaran-1": return "Unknown";
            case "alhaitham": return "Unknown";
            case "anderson-bastos": return "Anderson Bastos";
            case "angeleluna": return "Unknown";
            case "aphrodite-1": return "Unknown";
            case "aphrodite-2": return "Unknown";
            case "appendix": return "AppendixSTL";
            case "appendix-stl": return "AppendixSTL";
            case "arkevz": return "Unknown";
            case "arm-lamp": return "Unknown";
            case "artifex3d": return "Artifexd 3D";
            case "artistation": return "Artistation";
            case "artstation": return "Artistation";
            case "as": return "Unknown";
            case "atv": return "ATV Studios";
            case "axe": return "Unknown";
            case "b3dserk": return "B3DSERK";
            case "b3dserk-1": return "B3DSERK";
            case "b3dserk-2": return "B3DSERK";
            case "barruz": return "Unknown";
            case "bee-figures": return "Bee Figures";
            case "black-owl": return "Unknown";
            case "blue-spray": return "Unknown";
            case "bluespray": return "Unknown";
            case "book-holder": return "Unknown";
            case "boom-comics": return "Unknown";
            case "brunoart3d": return "BrunoArt3D";
            case "bulkamancer": return "Bulkmancer";
            case "bulkmancer": return "Bulkmancer";
            case "c27collectibles": return "C27 Collectibles";
            case "ca-studios": return "CA 3D STUDIOS";
            case "ca-studios-1": return "CA 3D STUDIOS";
            case "ca-studios-2": return "CA 3D STUDIOS";
            case "ca-studios-v1": return "CA 3D STUDIOS";
            case "ca-studios-v2": return "CA 3D STUDIOS";
            case "camus-1": return "Unknown";
            case "camus-2": return "Unknown";
            case "camus-3": return "Unknown";
            case "cardoso-3d": return "CA 3D STUDIOS";
            case "cardoso3d": return "CA 3D STUDIOS";
            case "carvalho": return "Unknown";
            case "cg-pyro": return "Unknown";
            case "chase-morello": return "Unknown";
            case "chibi": return "Unknown";
            case "chuya": return "Unknown";
            case "cisne-1": return "Unknown";
            case "cisne-2": return "Unknown";
            case "cisne-3": return "Unknown";
            case "cisne-4": return "Unknown";
            case "clay-cyanide": return "Unknown";
            case "claycyanide": return "Unknown";
            case "coyote-and-sonic": return "Unknown";
            case "creative-geek": return "Unknown";
            case "cw-studios": return "Unknown";
            case "damaia": return "Unknown";
            case "dario": return "Unknown";
            case "dc": return "Unknown";
            case "deathmask": return "Unknown";
            case "diluc": return "Unknown";
            case "diorama": return "Unknown";
            case "dlmkal": return "Unknown";
            case "dpaula": return "DPaula3D";
            case "dpaula3d": return "DPaula3D";
            case "dragon-collector": return "Unknown";
            case "dragun-studios": return "Unknown";
            case "dtr": return "Unknown";
            case "eddy-workshop": return "Eddy Workshop";
            case "eddythecross": return "Unknown";
            case "edythecross": return "Unknown";
            case "ehnovais": return "Unknown";
            case "emanuel-sko": return "Unknown";
            case "empty-forge": return "Unknown";
            case "eric-souza": return "Unknown";
            case "es": return "ES Monster";
            case "es-monster": return "ES Monster";
            case "esmonster": return "ES Monster";
            case "exclusive": return "Unknown";
            case "fanart": return "Unknown";
            case "fanart-1": return "Unknown";
            case "fanart-2": return "Unknown";
            case "fanart-3": return "Unknown";
            case "fanart-4": return "Unknown";
            case "fanart-5": return "Unknown";
            case "fanart-6": return "Unknown";
            case "fanart-7": return "Unknown";
            case "fandom": return "Unknown";
            case "fandom-1": return "Unknown";
            case "fandom-2": return "Unknown";
            case "fandom-3": return "Unknown";
            case "fandom-4": return "Unknown";
            case "fandom-5": return "Unknown";
            case "finger": return "Unknown";
            case "fracisquez": return "Unknown";
            case "francis-q": return "Unknown";
            case "frinted": return "Unknown";
            case "gambody": return "Gambody";
            case "geeksculpt": return "Unknown";
            case "gemini-1": return "Unknown";
            case "genesis": return "Unknown";
            case "god-of-light": return "Unknown";
            case "gom3d": return "Unknown";
            case "gui-gs": return "Unknown";
            case "guimimocas": return "Unknown";
            case "h3ll-creator": return "h3llcreator";
            case "h3llcreator": return "h3llcreator";
            case "hagen-1": return "Unknown";
            case "heroicas": return "Heroicas";
            case "heroicas-1": return "Heroicas";
            case "heroicas-2": return "Heroicas";
            case "hex3d": return "Unknown";
            case "hq-golden": return "Unknown";
            case "ikki-1": return "Unknown";
            case "ikki-2": return "Unknown";
            case "ikki-3": return "Unknown";
            case "immortal": return "Unknown";
            case "iron-studios": return "Iron Studios";
            case "jp-studio": return "Unknown";
            case "kabuki": return "Unknown";
            case "kafka": return "Unknown";
            case "kanon": return "Unknown";
            case "kauan": return "Unknown";
            case "kc-studio": return "Unknown";
            case "kugo": return "Unknown";
            case "kuton": return "Unknown";
            case "la-figures": return "LA Figures";
            case "lt3d": return "Unknown";
            case "lucas-perez": return "Lucas Perez";
            case "lucas-somariva": return "Unknown";
            case "lucasperez": return "Lucas Perez";
            case "luciano-berutti": return "Unknown";
            case "luftmensch": return "luftmensch";
            case "luftsmech": return "luftmensch";
            case "malix3d": return "Sanix3D";
            case "manoel-sko": return "Manuel Sko";
            case "marco-art": return "Unknown";
            case "marcoart": return "Unknown";
            case "marin-1": return "Unknown";
            case "mcm3d": return "Unknown";
            case "megha-l": return "Unknown";
            case "michael-b": return "Unknown";
            case "mighty-jax": return "Unknown";
            case "milo-1": return "Unknown";
            case "milo-2": return "Unknown";
            case "milo-3": return "Unknown";
            case "mocdan": return "Mocdan Collectibles";
            case "mocdan-collectibles": return "Mocdan Collectibles";
            case "moxomor": return "Unknown";
            case "nacho-cg": return "NachoCG";
            case "nachocg": return "NachoCG";
            case "nachocg3d": return "NachoCG";
            case "nanami": return "Unknown";
            case "neko": return "Unknown";
            case "nerikson": return "Nerikson";
            case "nomnom": return "Nomnom";
            case "nr-studios": return "Unknown";
            case "nympha": return "Unknown";
            case "oxo3d": return "Oxo3D";
            case "oz": return "Unknown";
            case "oz-1": return "Unknown";
            case "oz-2": return "Unknown";
            case "panarello": return "Panarello";
            case "patrickart": return "Unknown";
            case "peach-figure": return "Peach Figures";
            case "peach-figures": return "Peach Figures";
            case "pen-holder": return "Unknown";
            case "perez": return "Lucas Perez";
            case "pg-gasta": return "Unknown";
            case "pink-studio": return "Unknown";
            case "polymind": return "Polymind";
            case "pop-totem": return "Unknown";
            case "prime-1": return "Prime1";
            case "prime1": return "Prime1";
            case "prime1-cyborg": return "Prime1";
            case "prime1-normal": return "Prime1";
            case "private": return "Unknown";
            case "raven-eye": return "Unknown";
            case "risco": return "Unknown";
            case "rks3d": return "Unknown";
            case "sacrifice": return "Unknown";
            case "salares": return "Unknown";
            case "sanix": return "Sanix3D";
            case "sanix3d": return "Sanix3D";
            case "sanix3d-1": return "Sanix3D";
            case "sanix3d-2": return "Sanix3D";
            case "saori": return "Unknown";
            case "sasha": return "Unknown";
            case "sasha-2": return "Unknown";
            case "sasha-3": return "Unknown";
            case "sculpix": return "Unknown";
            case "seiya-1": return "Unknown";
            case "seiya-2": return "Unknown";
            case "shaina": return "Unknown";
            case "shaina-1": return "Unknown";
            case "shaka-1": return "Unknown";
            case "shaka-2": return "Unknown";
            case "shiryu-1": return "Unknown";
            case "shiryu-2": return "Unknown";
            case "shiryu-3": return "Unknown";
            case "shun-1": return "Unknown";
            case "shura-1": return "Unknown";
            case "sideshow": return "Sideshow";
            case "sigmaarts": return "Unknown";
            case "soarez-3d": return "Unknown";
            case "soarezstl": return "Unknown";
            case "stalyn-quito": return "Unknown";
            case "statix": return "Sanix3D";
            case "studio4": return "Unknown";
            case "sunray": return "Unknown";
            case "sword": return "Unknown";
            case "takuni": return "Takuni Figures";
            case "tanuki": return "Takuni Figures";
            case "titan-jacky": return "Unknown";
            case "toon-studios": return "Unknown";
            case "torrida": return "Torrida Minis";
            case "trick-or-threat-3d": return "Unknown";
            case "trick-or-treat": return "Unknown";
            case "u3d": return "Unknown";
            case "u3dprintshop": return "Unknown";
            case "ubi-collections": return "Unknown";
            case "ultimate": return "Unknown";
            case "universe": return "Universe Studios";
            case "unknown": return "Unknown";
            case "valentines": return "Unknown";
            case "vengeance": return "Vengeance Studios";
            case "vengeance-studios": return "Vengeance Studios";
            case "venomized": return "Unknown";
            case "vs3d": return "Unknown";
            case "vx-labs": return "VxLabs";
            case "wall-mounted": return "Unknown";
            case "wicked": return "Wicked3D";
            case "wicked-1": return "Wicked3D";
            case "wicked-2": return "Wicked3D";
            case "wicked-3": return "Wicked3D";
            case "willart": return "Unknown";
            case "xm": return "XM Studios";
            case "xm-studios": return "XM Studios";
            case "yan-h": return "YanH";
            case "yosh": return "Yosh Studios";
            case "yosh-studios": return "Yosh Studios";
            case "z-sculptors": return "Unknown";
            case "zez": return "Zez Studios";
            case "zez-1": return "Zez Studios";
            case "zez-2": return "Zez Studios";
            case "zez-studios": return "Zez Studios";
            case "zez-studios-2": return "Zez Studios";

            default:
                return "Unknown";
        }
    }

    public static void GenerateSqlSeedFile(DbContext context)
    {
        var basePath = new DirectoryInfo(".");

        var seedDirectory = Path.Combine(basePath.Parent.FullName, "PaintingProjectsManagment.Database", "Seed");
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

        var random = new Random();

        string[] images = [
            "uploads\\sample\\placeholder0.png",
            "uploads\\sample\\placeholder1.png",
            "uploads\\sample\\placeholder2.png",
            "uploads\\sample\\placeholder3.png",
            "uploads\\sample\\placeholder4.png",
            "uploads\\sample\\placeholder5.png",
            "uploads\\sample\\placeholder6.png",
            "uploads\\sample\\placeholder7.png",
            "uploads\\sample\\placeholder8.png",
            "uploads\\sample\\placeholder9.png",
        ];

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
                sql += $"$${images[random.Next(0, 10)]}$$, ";
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
