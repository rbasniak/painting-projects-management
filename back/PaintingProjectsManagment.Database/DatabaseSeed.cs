using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Paints;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagment.Database;

public class DatabaseSeed : DatabaseSeedManager<DatabaseContext>, IDatabaseSeeder
{
    public DatabaseSeed()
    {
        AddSeed("2025-07-19 16:00: Users seed", new SeedInfo<DatabaseContext>(UsersSeed));
        AddSeed("2025-07-19 16:48: Development materials seed", new SeedInfo<DatabaseContext>(DevelopmentMaterialsSeed));
        // AddSeed("2025-07-20: Army Painter Seed", new SeedInfo<DatabaseContext>(ArmyPainterFanaticsSeed));
    }

    private void UsersSeed(DatabaseContext context, IServiceProvider provider)
    {
        var tenant1 = context.Add(new Tenant("rodrigo.basniak", "Rodrigo Basniak")).Entity;
        var tenant2 = context.Add(new Tenant("ricardo.smarzaro", "Ricardo Smarzaro")).Entity;

        context.SaveChanges();

        var user1 = context.Add(new User("rodrigo.basniak", "rodrigo.basniak", "rbasniak@gmail.com", "trustno1", AvatarGenerator.GenerateSvgAvatar("R B"), "Rodrigo", AuthenticationMode.Credentials)).Entity;
        var user2 = context.Add(new User("ricardo.smarzaro", "ricardo.smarzaro", "smarza@gmail.com", "zemiko987", AvatarGenerator.GenerateSvgAvatar("R S"), "Ricardo", AuthenticationMode.Credentials)).Entity;

        user1.Confirm();
        user2.Confirm();

        context.SaveChanges();
    }

    private void DevelopmentMaterialsSeed(DatabaseContext context, IServiceProvider provider)
    {
        context.Set<Material>().Add(new Material("rodrigo.basniak", "6x3 magnet", MaterialUnit.Unit, 6));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "6x4 magnet", MaterialUnit.Unit, 7));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "8x4 magnet", MaterialUnit.Unit, 8));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "8x3 magnet", MaterialUnit.Unit, 9));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x3 magnet", MaterialUnit.Unit, 10));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x2 magnet", MaterialUnit.Unit, 11));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x1 magnet", MaterialUnit.Unit, 12));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "4x3 magnet", MaterialUnit.Unit, 13));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "3x2 magnet", MaterialUnit.Unit, 14));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "3x1 magnet", MaterialUnit.Unit, 15));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "10x5 magnet", MaterialUnit.Unit, 16));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 1mm", MaterialUnit.Centimeters, 1.0));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 3mm", MaterialUnit.Centimeters, 1.1));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 5mm", MaterialUnit.Centimeters, 1.2));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 10mm", MaterialUnit.Centimeters, 1.3));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 15mm", MaterialUnit.Centimeters, 1.4));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 20mm", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Green Stuff World Gloss Black Primer", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Green Stuff World Gloss Varnish", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Gloss Varnish", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Matt Varnish", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Ultra Matt Varnish", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Satin Varnish", MaterialUnit.Centimeters, 1.5));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "The Army Painter Satin Varnish", MaterialUnit.Centimeters, 1.5));

        context.SaveChanges();
    }

    private void ArmyPainterFanaticsSeed(DatabaseContext context, IServiceProvider provider)
    {
        var armyPainter = new PaintBrand(Guid.CreateVersion7(), "TheArmyPainter");
        context.Set<PaintBrand>().Add(armyPainter);

        var apFanatics = new PaintLine(armyPainter, Guid.CreateVersion7(), "Warpaints Fanatics");
        context.Set<PaintLine>().Add(apFanatics);

        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Matt Black", "#000000", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Deep Grey", "#595d66", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Uniform Grey", "#6f737c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ash Grey", "#8d9194", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Company Grey", "#c0c1c6", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Brigade Grey", "#d5d5d7", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Castle Grey", "#9c9992", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Gargoyle Grey", "#b3ac9c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Great Hall Grey", "#c6bdae", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Worn Stone", "#cec7b7", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Brainmatter Beige", "#e9e6d7", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Matt White", "#fdfdfd", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Night Sky", "#374757", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Thunderous Blue", "#365478", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Stratos Blue", "#33628c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wolf Grey", "#5479a4", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Runic Cobalt", "#7c95b3", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Frost Blue", "#a5c0dd", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Triumphant Navy", "#1b2a53", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Gothic Blue", "#29386f", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ultramarine Blue", "#274486", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Alpha Blue", "#6979b5", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Baron Blue", "#8697cd", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Augur Blue", "#b0c2ea", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Imperial Navy", "#003261", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Regal Blue", "#004a83", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Royal Blue", "#0055a8", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Crystal Blue", "#006dbf", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Arctic Gem", "#008ece", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Bright Sapphire", "#93ccea", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Deep Ocean Blue", "#213341", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Abyssal Blue", "#00506b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tidal Blue", "#006a94", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Phalanx Blue", "#0086ac", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Shieldwall Blue", "#009ec5", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Aegis Aqua", "#3fadd0", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Deep Azure", "#016877", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Hydra Turqouise", "#018c93", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Turqouise Siren", "#25a9ae", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Aquamarine", "#59bec6", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Neptune Glow", "#7dccd1", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Marine Mist", "#9cd4d1", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Scarab Green", "#1e3b41", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Temple Gate Teal", "#005c4d", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pharoah Guard", "#00755a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Talisman Teal", "#00a585", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Aqua Alchemy", "#21b59b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Amulet Aqua", "#57cdb5", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Emerald Forest", "#52ad34", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Leafy Green", "#78bb0c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Rainforest", "#88d000", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Electric Lime", "#acd701", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Vivid Volt", "#d1e23c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Afterglow", "#e9ef8d", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Evergreen Fog", "#3d655c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Medieval Forest", "#457b6b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Patagon Pine", "#64917c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Autumn Sage", "#75ab93", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Forest Faun", "#a4c9aa", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Mossy Green", "#b8d8b9", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Woodland Camo", "#4f6347", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Army Green", "#61784a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Camoflauge Green", "#728251", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Olive Drab", "#7e9056", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Necrotic Flesh", "#a1ac82", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Grotesque Green", "#b6c09b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Angel Green", "#1f3c2a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Guardian Green", "#205f42", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Greenskin", "#097239", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Eternal Hunt", "#308c37", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wild Green", "#44a244", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ferocious Green", "#75c06b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tundra Taupe", "#53533b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Prairie Ochre", "#6f6740", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Desert Yellow", "#88743f", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wasteland Clay", "#a88a4c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Burnt Turf", "#c19c4e", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Barren Dune", "#dbba6b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dusty Skull", "#928670", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tomb King Tan", "#aa9a80", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Skeleton Bone", "#c1b696", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ancient Stone", "#d4c9a9", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Boney Spikes", "#e9d9bf", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pale Sand", "#f4ecd7", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Demigod Flames", "#d07733", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Fiendish Yellow", "#e39737", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Daemonic Yellow", "#ffca00", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Warped Yellow", "#f9d134", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Space Dust", "#f8e080", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ice Yellow", "#fdebab", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Molten Lava", "#e84d31", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Burning Ore", "#f35c31", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Lava Orange", "#fa7310", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Flickering Flame", "#ff8b2c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Glowing Inferno", "#ffa837", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Inner Light", "#feca52", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Resplendent Red", "#972427", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Angelic Red", "#cd323a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Legendary Red", "#e6382f", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Sacred Scarlet", "#ec563e", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Violent Vermillion", "#f0745a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Raging Rouge", "#f28c7c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Brigadine Brown", "#443f3c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Bootstrap Brown", "#645250", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Leather Brown", "#806055", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Paratrooper Tan", "#9c7b6c", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Command Khaki", "#b99889", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Urban Buff", "#d6b5a6", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Basalisk Red", "#702738", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wyvern Fury", "#923d48", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dragon Red", "#94404b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pure Red", "#a81719", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Blood Chalice", "#b33f42", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Raging Rose", "#b05453", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Oak Brown", "#524743", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tree Ancient", "#615049", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dryad Brown", "#6e5248", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Fur Brown", "#7f5651", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ruddy Umber", "#875448", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Buffed Hide", "#a6776f", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Terrestial Titan", "#494753", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Alien Purple", "#665591", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Cultist Purple", "#7e69b1", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Hexed Violet", "#8877ad", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Violet Coven", "#9985af", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Kraken Lavender", "#b7acbe", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wicked Pink", "#a91269", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Impish Rouge", "#be4586", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pixie Pink", "#c5629b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Weird Elixer", "#b77a9e", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pink Potion", "#ca99b6", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Doomfire Drab", "#c8afc0", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Mulled Berry", "#684852", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Moldy Wine", "#86485d", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Elder Flower", "#a0627e", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Forbidden Fruit", "#b37d92", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Figgy Pink", "#c39bab", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Wilted Rose", "#c5abb8", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Diabolic Plum", "#5c3b6b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Magecast Magenta", "#814b99", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Warlock Magenta", "#9453a2", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Spellbound Fuchia", "#b1559a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Enchanted Pink", "#ab5f9b", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Diviner Light", "#c890bc", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Moonstone Skin", "#ad6f66", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Agate Skin", "#c67f75", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Barbarian Flesh", "#c98873", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Ruby Skin", "#ce9f8a", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Opal Skin", "#e5c2b7", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Pearl Skin", "#dbc9c0", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Obsidian Skin", "#56504f", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Onyx Skin", "#6c5851", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Mocca Skin", "#897164", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Amber Skin", "#a78d72", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dorado Skin", "#c4a786", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Quartz Skin", "#d3baa0", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Carnelian Skin", "#644c4d", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tigers Eye Skin", "#8d5958", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Topaz Skin", "#a46058", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Jasper Skin", "#9c6963", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tourmaline Skin", "#a87a6e", 17, 3.5, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Leopard Stone Skin", "#b99590", 17, 3.5, PaintType.Opaque));

        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dark Tone", "#252525", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Strong Tone", "#3a2a1d", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Soft Tone", "#72563e", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Light Tone", "#a06c1f", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Sepia Tone", "#93551f", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Rust Tone", "#573618", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dark Red Tone", "#44212b", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Red Tone", "#5c202c", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Orange Tone", "#904115", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Green Tone", "#2c4831", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Military Shade", "#3b401d", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Blue Tone", "#1e3e5e", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dark Blue Tone", "#293647", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Purple Tone", "#443256", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Magenta Tone", "#8e0e5b", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Strong Skin Shade", "#432724", 17, 3.5, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dark Skin Shade", "#423337", 17, 3.5, PaintType.Wash));

        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Rough Iron", "#362d25", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Red Copper", "#683d3f", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Weapon Bronze", "#b16716", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "True Copper", "#936143", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Evil Chrome", "#7e5341", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "True Brass", "#8d8479", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Tainted Gold", "#6b6644", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Greedy Gold", "#ac7725", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Bright Gold", "#ae8b20", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Mithril", "#bbbdc0", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Shining Silver", "#94969a", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Plate Metal", "#6a6b6f", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Gun Metal", "#505155", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Cobalt Metal", "#31464a", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Death Metal", "#2a2a2a", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Dark Emerald", "#415129", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Glittering Green", "#2f7e4d", 17, 3.5, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(Guid.CreateVersion7(), apFanatics, "Gemstone Red", "#851f26", 17, 3.5, PaintType.Metallic));

        context.SaveChanges();
    }
}
