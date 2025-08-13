using Microsoft.EntityFrameworkCore;
using rbkApiModules.Testing.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class List_Materials_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test materials for different users
        var rodrigoMaterial1 = new Material("rodrigo.basniak", "Rodrigo Material 1", new Quantity(1, PackageUnits.Each), new Money(10.0, "USD"));
        var rodrigoMaterial2 = new Material("rodrigo.basniak", "Rodrigo Material 2", new Quantity(1, PackageUnits.Each), new Money(5.0, "USD"));
        var ricardoMaterial1 = new Material("ricardo.smarzaro", "Ricardo Material 1", new Quantity(1, PackageUnits.Each), new Money(15.0, "USD"));
        var ricardoMaterial2 = new Material("ricardo.smarzaro", "Ricardo Material 2", new Quantity(1, PackageUnits.Each), new Money(8.0, "USD"));

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Material>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(rodrigoMaterial2);
            await context.AddAsync(ricardoMaterial2);
            await context.AddAsync(rodrigoMaterial1);
            await context.AddAsync(ricardoMaterial1);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var rodrigoMaterials = context.Set<Material>().Where(x => x.TenantId == "RODRIGO.BASNIAK").ToList();
            rodrigoMaterials.Count.ShouldBe(2);

            var ricardoMaterials = context.Set<Material>().Where(x => x.TenantId == "RICARDO.SMARZARO").ToList();
            ricardoMaterials.Count.ShouldBe(2);
        }
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Materials()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MaterialDetails>>("api/materials");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Their_Own_Materials()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MaterialDetails>>("api/materials", "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(2);

        // Verify the materials belong to the correct user
        var materialNames = response.Data.Select(x => x.Name).ToList();
        materialNames.ShouldContain("Ricardo Material 1");
        materialNames.ShouldContain("Ricardo Material 2");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Materials_Are_Returned_With_Correct_Properties()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MaterialDetails>>("api/materials", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(2);

        // Verify material properties are correctly mapped
        var material1 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Material 1");
        material1.ShouldNotBeNull();
        material1.PackagePrice.Amount.ShouldBe(10.0);

        var material2 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Material 2");
        material2.ShouldNotBeNull();
        material2.PackagePrice.Amount.ShouldBe(5.0);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}