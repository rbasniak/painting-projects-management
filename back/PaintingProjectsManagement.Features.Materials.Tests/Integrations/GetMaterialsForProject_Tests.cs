using Microsoft.EntityFrameworkCore;
using rbkApiModules.Testing.Core;
using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class GetMaterialsForProject_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Material rodrigoMaterial1 = null!;
    private static Material rodrigoMaterial2 = null!;
    private static Material ricardoMaterial1 = null!;
    private static Material ricardoMaterial2 = null!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test materials for different users
        rodrigoMaterial1 = new Material("rodrigo.basniak", "Rodrigo Material 1", MaterialUnit.Unit, 10.0);
        rodrigoMaterial2 = new Material("rodrigo.basniak", "Rodrigo Material 2", MaterialUnit.Drops, 5.0);
        ricardoMaterial1 = new Material("ricardo.smarzaro", "Ricardo Material 1", MaterialUnit.Unit, 15.0);
        ricardoMaterial2 = new Material("ricardo.smarzaro", "Ricardo Material 2", MaterialUnit.Drops, 8.0);

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Material>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(rodrigoMaterial1);
            await context.AddAsync(rodrigoMaterial2);
            await context.AddAsync(ricardoMaterial1);
            await context.AddAsync(ricardoMaterial2);
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

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Materials_For_Project()
    {
        // Prepare
        var request = new GetMaterialsForProject.Request
        {
            MaterialIds = new[] { rodrigoMaterial1.Id, rodrigoMaterial2.Id },
        };

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeFalse();
        response.ShouldHaveMessages("This must be used in an authenticated context.");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_Get_Their_Own_Materials_By_Ids()
    {
        // Prepare
        var request = new GetMaterialsForProject.Request
        {
            MaterialIds = new[] { rodrigoMaterial1.Id, rodrigoMaterial2.Id },
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(2);

        // Verify the materials belong to the correct user
        var materialNames = response.Data.Select(x => x.Name).ToList();
        materialNames.ShouldContain("Rodrigo Material 1");
        materialNames.ShouldContain("Rodrigo Material 2");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Get_Materials_From_Other_Users()
    {
        // Prepare - Try to get materials from another user
        var request = new Abstractions.GetMaterialsForProject.Request
        {
            MaterialIds = new[] { ricardoMaterial1.Id, ricardoMaterial2.Id }
        };
        
        // Set the identity for rodrigo user (trying to access ricardo's materials)
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(0); // Should return empty list, not the other user's materials
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Get_Mixed_Own_And_Other_Users_Materials_Returns_Only_Own()
    {
        // Prepare - Mix of own materials and other user's materials
        var request = new GetMaterialsForProject.Request
        {
            MaterialIds = new[] { rodrigoMaterial1.Id, ricardoMaterial1.Id, rodrigoMaterial2.Id }
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(2); // Only rodrigo's materials

        // Verify only rodrigo's materials are returned
        var materialNames = response.Data.Select(x => x.Name).ToList();
        materialNames.ShouldContain("Rodrigo Material 1");
        materialNames.ShouldContain("Rodrigo Material 2");
        materialNames.ShouldNotContain("Ricardo Material 1");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_With_Empty_MaterialIds_Returns_Empty_List()
    {
        // Prepare
        var request = new Abstractions.GetMaterialsForProject.Request
        {
            MaterialIds = Array.Empty<Guid>()
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(0);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_With_NonExistent_MaterialIds_Returns_Empty_List()
    {
        // Prepare
        var request = new GetMaterialsForProject.Request
        {
            MaterialIds = new[] { Guid.NewGuid(), Guid.NewGuid() }
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(0);
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Materials_Are_Returned_With_Correct_Properties()
    {
        // Prepare
        var request = new GetMaterialsForProject.Request
        {
            MaterialIds = new[] { rodrigoMaterial1.Id, rodrigoMaterial2.Id }
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(2);

        // Verify material properties are correctly mapped
        var material1 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Material 1");
        material1.ShouldNotBeNull();
        material1.Unit.ShouldBe(MaterialUnit.Unit);
        material1.PricePerUnit.ShouldBe(10.0);

        var material2 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Material 2");
        material2.ShouldNotBeNull();
        material2.Unit.ShouldBe(MaterialUnit.Drops);
        material2.PricePerUnit.ShouldBe(5.0);
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Can_Get_Single_Material()
    {
        // Prepare
        var request = new Abstractions.GetMaterialsForProject.Request
        {
            MaterialIds = new[] { rodrigoMaterial1.Id }
        };
        
        // Set the identity for rodrigo user
        request.SetIdentity("rodrigo.basniak", "rodrigo.basniak", new[] { "User" });

        // Act
        var response = await TestingServer.Dispatcher.SendAsync(request, default);

        // Assert the response
        response.IsValid.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);

        // Verify the correct material is returned
        var material = response.Data.First();
        material.Name.ShouldBe("Rodrigo Material 1");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 