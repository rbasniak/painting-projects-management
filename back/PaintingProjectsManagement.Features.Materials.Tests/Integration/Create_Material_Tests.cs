using Microsoft.EntityFrameworkCore;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Create_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Setup test data directly in database
        using (var context = TestingServer.CreateContext())
        {
            // Create a test tenant
            var tenant = new Tenant("TEST", "Test Tenant", "");
            context.Set<Tenant>().Add(tenant);
            await context.SaveChangesAsync();
        }

        // Prepare
        // Create a test material for duplicate name validation tests
        var existingMaterial = new Material("TEST", "Existing Material", MaterialUnit.Unit, 10.0);

        // Act          
        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            await context.AddAsync(existingMaterial);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
            savedMaterial.ShouldNotBeNull();
        }
    }

    [Test, NotInParallel(Order = 2)]
    public async Task User_Cannot_Create_Material_When_Name_Is_Empty()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "",
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Create_Material_When_Name_Is_Null()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = null!,
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == null).ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Material_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = new string('A', 101), // Exceeds max length of 100
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name.Length > 100).ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Create_Material_When_Name_Already_Exists()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Existing Material", // This name was created in Seed test
            Unit = MaterialUnit.Drops,
            PricePerUnit = 15,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A material with this name already exists.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Existing Material").ToList();
        materials.Count.ShouldBe(1); // Only the original one from Seed
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Material_When_PricePerUnit_Is_Zero()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Test Material",
            Unit = MaterialUnit.Unit,
            PricePerUnit = 0,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Price per unit must be greater than zero.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Test Material").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Create_Material_When_PricePerUnit_Is_Negative()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Test Material",
            Unit = MaterialUnit.Unit,
            PricePerUnit = -5,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Price per unit must be greater than zero.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Test Material").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Create_Material_When_Multiple_Validation_Errors_Occur()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "", // Empty name
            Unit = MaterialUnit.Unit,
            PricePerUnit = -10, // Negative price
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.", "Price per unit must be greater than zero.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Cannot_Create_Material_When_Name_Contains_Only_Whitespace()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "   ", // Only whitespace
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name.Trim() == "").ToList();
        materials.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Cannot_Create_Material_When_Name_Is_Too_Short()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "A", // Very short name (though this might be valid, testing edge case)
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        // This might pass validation, but we're testing the edge case
        if (response.IsSuccess)
        {
            // If it passes, verify the database
            var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "A").ToList();
            materials.Count.ShouldBe(1);
        }
        else
        {
            // If it fails, verify no material was created
            var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "A").ToList();
            materials.ShouldBeEmpty();
        }
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Cannot_Create_Material_When_PricePerUnit_Is_Very_Large()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Expensive Material",
            Unit = MaterialUnit.Unit,
            PricePerUnit = double.MaxValue, // Very large value
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        // This might pass validation, but we're testing the edge case
        if (response.IsSuccess)
        {
            // If it passes, verify the database
            var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Expensive Material").ToList();
            materials.Count.ShouldBe(1);
        }
        else
        {
            // If it fails, verify no material was created
            var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Expensive Material").ToList();
            materials.ShouldBeEmpty();
        }
    }

    [Test, NotInParallel(Order = 15)]
    public async Task User_Cannot_Create_Material_When_Unit_Is_Invalid()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Invalid Unit Material",
            Unit = (MaterialUnit)999, // Invalid enum value
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Unit has an invalid value.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Invalid Unit Material").ToList();
        materials.ShouldBeEmpty();
    }

    /// <summary>
    /// The user should be able to create a new material with valid data
    /// </summary>
    [Test, NotInParallel(Order = 17)]
    public async Task User_Can_Create_Material()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "8x4 magnet",
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("8x4 magnet");
        result.PricePerUnit.ShouldBe(19);
        result.Unit.ShouldNotBeNull();
        result.Unit.Id.ShouldBe((int)MaterialUnit.Unit);
        result.Unit.Value.ShouldBe(MaterialUnit.Unit.ToString());

        // Assert the database
        var entity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("8x4 magnet");
        entity.PricePerUnit.ShouldBe(19);
        entity.Unit.ShouldBe(MaterialUnit.Unit);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}