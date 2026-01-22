using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Inventory;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed : DatabaseSeedManager<DatabaseContext>, IDatabaseSeeder
{
    public DatabaseSeed()
    {
        AddSeed("2025-07-19 16:00: Users seed", new SeedInfo<DatabaseContext>(UsersSeed));
        AddSeed("2025-07-20 08:15: Development models seed", new SeedInfo<DatabaseContext>(DevelopmentModelsSeed, EnvironmentUsage.Development));
        AddSeed("2025-07-22 23:15: Admin claims seed", new SeedInfo<DatabaseContext>(AdminClaimsSeed));
        AddSeed("2025-08-11 23:15: Example materials", new SeedInfo<DatabaseContext>(MaterialsSeed, EnvironmentUsage.Development));
        AddSeed("2025-11-11 22:30: Example projects", new SeedInfo<DatabaseContext>(ProjectsSeed, EnvironmentUsage.Development));
        AddSeed("2025-12-05 12:00: Update materials tenant", new SeedInfo<DatabaseContext>(UpdateMaterialsTenant, EnvironmentUsage.Development));
        AddSeed("2025-07-20: Army Painter Seed", new SeedInfo<DatabaseContext>(ArmyPainterFanaticsSeed));
    }

    private void UpdateMaterialsTenant(DatabaseContext context, IServiceProvider provider)
    {
        context.Database.ExecuteSqlRaw("update public.\"projects.projections.materials\" set \"Tenant\" = 'RODRIGO.BASNIAK'");
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

    private void ArmyPainterFanaticsSeed(DatabaseContext context, IServiceProvider provider)
    {
        var armyPainter = new PaintBrand("TheArmyPainter");
        context.Set<PaintBrand>().Add(armyPainter);

        var apFanatics = new PaintLine(armyPainter, "Warpaints Fanatics");
        context.Set<PaintLine>().Add(apFanatics);

        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Matt Black", "#000000", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Deep Grey", "#595d66", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Uniform Grey", "#6f737c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ash Grey", "#8d9194", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Company Grey", "#c0c1c6", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Brigade Grey", "#d5d5d7", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Castle Grey", "#9c9992", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Gargoyle Grey", "#b3ac9c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Great Hall Grey", "#c6bdae", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Worn Stone", "#cec7b7", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Brainmatter Beige", "#e9e6d7", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Matt White", "#fdfdfd", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Night Sky", "#374757", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Thunderous Blue", "#365478", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Stratos Blue", "#33628c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wolf Grey", "#5479a4", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Runic Cobalt", "#7c95b3", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Frost Blue", "#a5c0dd", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Triumphant Navy", "#1b2a53", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Gothic Blue", "#29386f", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ultramarine Blue", "#274486", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Alpha Blue", "#6979b5", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Baron Blue", "#8697cd", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Augur Blue", "#b0c2ea", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Imperial Navy", "#003261", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Regal Blue", "#004a83", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Royal Blue", "#0055a8", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Crystal Blue", "#006dbf", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Arctic Gem", "#008ece", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Bright Sapphire", "#93ccea", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Deep Ocean Blue", "#213341", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Abyssal Blue", "#00506b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tidal Blue", "#006a94", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Phalanx Blue", "#0086ac", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Shieldwall Blue", "#009ec5", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Aegis Aqua", "#3fadd0", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Deep Azure", "#016877", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Hydra Turqouise", "#018c93", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Turqouise Siren", "#25a9ae", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Aquamarine", "#59bec6", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Neptune Glow", "#7dccd1", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Marine Mist", "#9cd4d1", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Scarab Green", "#1e3b41", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Temple Gate Teal", "#005c4d", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pharoah Guard", "#00755a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Talisman Teal", "#00a585", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Aqua Alchemy", "#21b59b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Amulet Aqua", "#57cdb5", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Emerald Forest", "#52ad34", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Leafy Green", "#78bb0c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Rainforest", "#88d000", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Electric Lime", "#acd701", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Vivid Volt", "#d1e23c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Afterglow", "#e9ef8d", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Evergreen Fog", "#3d655c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Medieval Forest", "#457b6b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Patagon Pine", "#64917c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Autumn Sage", "#75ab93", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Forest Faun", "#a4c9aa", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Mossy Green", "#b8d8b9", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Woodland Camo", "#4f6347", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Army Green", "#61784a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Camoflauge Green", "#728251", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Olive Drab", "#7e9056", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Necrotic Flesh", "#a1ac82", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Grotesque Green", "#b6c09b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Angel Green", "#1f3c2a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Guardian Green", "#205f42", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Greenskin", "#097239", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Eternal Hunt", "#308c37", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wild Green", "#44a244", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ferocious Green", "#75c06b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tundra Taupe", "#53533b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Prairie Ochre", "#6f6740", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Desert Yellow", "#88743f", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wasteland Clay", "#a88a4c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Burnt Turf", "#c19c4e", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Barren Dune", "#dbba6b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dusty Skull", "#928670", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tomb King Tan", "#aa9a80", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Skeleton Bone", "#c1b696", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ancient Stone", "#d4c9a9", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Boney Spikes", "#e9d9bf", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pale Sand", "#f4ecd7", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Demigod Flames", "#d07733", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Fiendish Yellow", "#e39737", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Daemonic Yellow", "#ffca00", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Warped Yellow", "#f9d134", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Space Dust", "#f8e080", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ice Yellow", "#fdebab", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Molten Lava", "#e84d31", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Burning Ore", "#f35c31", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Lava Orange", "#fa7310", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Flickering Flame", "#ff8b2c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Glowing Inferno", "#ffa837", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Inner Light", "#feca52", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Resplendent Red", "#972427", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Angelic Red", "#cd323a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Legendary Red", "#e6382f", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Sacred Scarlet", "#ec563e", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Violent Vermillion", "#f0745a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Raging Rouge", "#f28c7c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Brigadine Brown", "#443f3c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Bootstrap Brown", "#645250", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Leather Brown", "#806055", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Paratrooper Tan", "#9c7b6c", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Command Khaki", "#b99889", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Urban Buff", "#d6b5a6", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Basalisk Red", "#702738", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wyvern Fury", "#923d48", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dragon Red", "#94404b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pure Red", "#a81719", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Blood Chalice", "#b33f42", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Raging Rose", "#b05453", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Oak Brown", "#524743", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tree Ancient", "#615049", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dryad Brown", "#6e5248", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Fur Brown", "#7f5651", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ruddy Umber", "#875448", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Buffed Hide", "#a6776f", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Terrestial Titan", "#494753", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Alien Purple", "#665591", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Cultist Purple", "#7e69b1", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Hexed Violet", "#8877ad", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Violet Coven", "#9985af", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Kraken Lavender", "#b7acbe", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wicked Pink", "#a91269", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Impish Rouge", "#be4586", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pixie Pink", "#c5629b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Weird Elixer", "#b77a9e", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pink Potion", "#ca99b6", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Doomfire Drab", "#c8afc0", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Mulled Berry", "#684852", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Moldy Wine", "#86485d", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Elder Flower", "#a0627e", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Forbidden Fruit", "#b37d92", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Figgy Pink", "#c39bab", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Wilted Rose", "#c5abb8", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Diabolic Plum", "#5c3b6b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Magecast Magenta", "#814b99", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Warlock Magenta", "#9453a2", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Spellbound Fuchia", "#b1559a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Enchanted Pink", "#ab5f9b", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Diviner Light", "#c890bc", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Moonstone Skin", "#ad6f66", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Agate Skin", "#c67f75", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Barbarian Flesh", "#c98873", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Ruby Skin", "#ce9f8a", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Opal Skin", "#e5c2b7", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Pearl Skin", "#dbc9c0", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Obsidian Skin", "#56504f", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Onyx Skin", "#6c5851", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Mocca Skin", "#897164", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Amber Skin", "#a78d72", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dorado Skin", "#c4a786", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Quartz Skin", "#d3baa0", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Carnelian Skin", "#644c4d", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tigers Eye Skin", "#8d5958", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Topaz Skin", "#a46058", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Jasper Skin", "#9c6963", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tourmaline Skin", "#a87a6e", 17, PaintType.Opaque));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Leopard Stone Skin", "#b99590", 17, PaintType.Opaque));

        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dark Tone", "#252525", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Strong Tone", "#3a2a1d", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Soft Tone", "#72563e", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Light Tone", "#a06c1f", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Sepia Tone", "#93551f", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Rust Tone", "#573618", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dark Red Tone", "#44212b", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Red Tone", "#5c202c", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Orange Tone", "#904115", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Green Tone", "#2c4831", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Military Shade", "#3b401d", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Blue Tone", "#1e3e5e", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dark Blue Tone", "#293647", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Purple Tone", "#443256", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Magenta Tone", "#8e0e5b", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Strong Skin Shade", "#432724", 17, PaintType.Wash));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dark Skin Shade", "#423337", 17, PaintType.Wash));

        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Rough Iron", "#362d25", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Red Copper", "#683d3f", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Weapon Bronze", "#b16716", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "True Copper", "#936143", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Evil Chrome", "#7e5341", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "True Brass", "#8d8479", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Tainted Gold", "#6b6644", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Greedy Gold", "#ac7725", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Bright Gold", "#ae8b20", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Mithril", "#bbbdc0", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Shining Silver", "#94969a", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Plate Metal", "#6a6b6f", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Gun Metal", "#505155", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Cobalt Metal", "#31464a", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Death Metal", "#2a2a2a", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Dark Emerald", "#415129", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Glittering Green", "#2f7e4d", 17, PaintType.Metallic));
        context.Set<PaintColor>().Add(new PaintColor(apFanatics, "Gemstone Red", "#851f26", 17, PaintType.Metallic));

        context.SaveChanges();
    }

    private void AdminClaimsSeed(DatabaseContext context, IServiceProvider provider)
    {
        var superuser = context.Set<User>().First(x => x.Username == "superuser");

        var claim = new Claim(ApplicationClaims.MANAGE_PAINTS.Name, ApplicationClaims.MANAGE_PAINTS.Description);

        context.Add(claim);

        var userClaim = new UserToClaim(superuser, claim, ClaimAccessType.Allow);

        context.Add(userClaim);

        context.SaveChanges();
    }
}
