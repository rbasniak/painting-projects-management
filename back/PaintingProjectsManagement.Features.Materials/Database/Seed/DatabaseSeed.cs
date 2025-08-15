using PaintingProjectsManagement.Features;
using PaintingProjectsManagement.Features.Materials;
using rbkApiModules.Commons.Relational;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed : DatabaseSeedManager<MaterialsDatabase>, IDatabaseSeeder
{
    public DatabaseSeed()
    {
        AddSeed("2025-07-19 16:48: Development materials seed", new SeedInfo<MaterialsDatabase>(DevelopmentMaterialsSeed));
    }

    private void DevelopmentMaterialsSeed(MaterialsDatabase context, IServiceProvider provider)
    {
        context.Set<Material>().Add(new Material("rodrigo.basniak", "6x3 magnet", new Quantity(1, PackageContentUnit.Each), new Money(6, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "6x4 magnet", new Quantity(1, PackageContentUnit.Each), new Money(7, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "8x4 magnet", new Quantity(1, PackageContentUnit.Each), new Money(8, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "8x3 magnet", new Quantity(1, PackageContentUnit.Each), new Money(9, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x3 magnet", new Quantity(1, PackageContentUnit.Each), new Money(10, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x2 magnet", new Quantity(1, PackageContentUnit.Each), new Money(11, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "5x1 magnet", new Quantity(1, PackageContentUnit.Each), new Money(12, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "4x3 magnet", new Quantity(1, PackageContentUnit.Each), new Money(13, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "3x2 magnet", new Quantity(1, PackageContentUnit.Each), new Money(14, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "3x1 magnet", new Quantity(1, PackageContentUnit.Each), new Money(15, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "10x5 magnet", new Quantity(1, PackageContentUnit.Each), new Money(16, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 1mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.0, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 3mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.1, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 5mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.2, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 10mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.3, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 15mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.4, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Tamiya masking tape 20mm", new Quantity(100, PackageContentUnit.Meter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Green Stuff World Gloss Black Primer", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Green Stuff World Gloss Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Gloss Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Matt Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Ultra Matt Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "Vallejo Satin Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", "The Army Painter Satin Varnish", new Quantity(200, PackageContentUnit.Milliliter), new Money(1.5, "USD")));

        context.SaveChanges();
    }
}
