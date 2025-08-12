using Microsoft.EntityFrameworkCore;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Update_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test materials for different users
        var existingMaterial = new Material("rodrigo.basniak", "Existing Material", new Quantity(1, PackageUnits.Each), new Money(10.0, "USD"));
        var anotherUserMaterial = new Material("ricardo.smarzaro", "Another User Material", new Quantity(1, PackageUnits.Each), new Money(5.0, "USD"));
        var duplicateNameMaterial = new Material("rodrigo.basniak", "Duplicate Name Material", new Quantity(1, PackageUnits.Each), new Money(15.0, "USD"));

        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            await context.AddAsync(existingMaterial);
            await context.AddAsync(anotherUserMaterial);
            await context.AddAsync(duplicateNameMaterial);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
            savedMaterial.ShouldNotBeNull();

            var savedAnotherUserMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Another User Material");
            savedAnotherUserMaterial.ShouldNotBeNull();

            var savedDuplicateNameMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Duplicate Name Material");
            savedDuplicateNameMaterial.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Material()
    {
        // Prepare
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Updated Material",
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 25.0,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Material"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Material_That_Does_Not_Exist()
    {
        // Prepare
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateMaterial.Request
        {
            Id = nonExistentId,
            Name = "Updated Material",
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 25.0,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Updated Material").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Update_Material_That_Belongs_To_Another_User()
    {
        // Prepare
        var anotherUserMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Another User Material");
        anotherUserMaterial.ShouldNotBeNull("Material should exist from seed");
        anotherUserMaterial.TenantId.ShouldBe("RICARDO.SMARZARO", "Material should belong to another user");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = anotherUserMaterial.Id,
            Name = "Hacked Material",
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 100.0,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == anotherUserMaterial.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Another User Material"); // Name should remain unchanged
        unchangedEntity.TenantId.ShouldBe("RICARDO.SMARZARO"); // Should still belong to the original user
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Material_When_Name_Already_Exists()
    {
        // Prepare
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Duplicate Name Material", // This name already exists for the same user
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 25.0,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A material with this name already exists.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Material"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Update_Material_With_Same_Name_As_Another_User()
    {
        // Prepare
        var duplicateNameMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Duplicate Name Material");
        duplicateNameMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = duplicateNameMaterial.Id,
            Name = "Another User Material", // This name exists for another user (ricardo.smarzaro)
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 30.0,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == duplicateNameMaterial.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Another User Material");
        updatedEntity.UnitPriceAmount.ShouldBe(15.0); // 30/2 = 15
        updatedEntity.UnitPriceUnit.ShouldBe(PackageUnits.Each);
        updatedEntity.TenantId.ShouldBe("RODRIGO.BASNIAK"); // Should still belong to the original user

        // Verify the other user's material is unchanged
        var otherUserMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.TenantId == "RICARDO.SMARZARO" && x.Name == "Another User Material");
        otherUserMaterial.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Can_Update_Material_With_Same_Name_As_Itself()
    {
        // Prepare
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Existing Material", // Same name as itself
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 50.0, // Change price
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Existing Material"); // Name remains the same
        updatedEntity.UnitPriceAmount.ShouldBe(25.0); // 50/2 = 25
        updatedEntity.UnitPriceUnit.ShouldBe(PackageUnits.Each); // Unit was updated
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Can_Update_Material()
    {
        // Prepare
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Updated Material Name",
            PackageAmount = 2,
            PackageUnit = PackageUnits.Each,
            PackagePriceAmount = 25.50,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Id.ShouldBe(existingMaterial.Id);
        updatedEntity.Name.ShouldBe("Updated Material Name");
        updatedEntity.UnitPriceAmount.ShouldBe(12.75); // 25.5/2
        updatedEntity.UnitPriceUnit.ShouldBe(PackageUnits.Each);
        updatedEntity.TenantId.ShouldBe("RODRIGO.BASNIAK"); // Should still belong to the same user
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}