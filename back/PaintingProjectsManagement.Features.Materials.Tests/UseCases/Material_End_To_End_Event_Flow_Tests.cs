using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using PaintingProjectsManagement.Features.Projects;
using PaintingProjectsManagement.Testing.Core;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Material_End_To_End_Event_Flow_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    private static readonly string TestUser = "rodrigo.basniak";

    [Test, NotInParallel(Order = 2)]
    public async Task CompleteEventFlow_CreateMaterial_ShouldCreateReadOnlyMaterialInProjectsModule()
    {
        // Arrange
        var materialName = "End-to-End Test Material";
        var packageContent = new Quantity(100.0, PackageContentUnit.Milliliter);
        var packagePrice = new Money(25.50, "USD");

        // Act - Create material through API (triggers domain event → integration event → consumer)
        var request = new CreateMaterial.Request
        {
            Name = materialName,
            PackageContentAmount = packageContent.Amount,
            PackageContentUnit = (int)packageContent.Unit,
            PackagePriceAmount = packagePrice.Amount,
            PackagePriceCurrency = packagePrice.CurrencyCode
        };

        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, TestUser);
        response.ShouldBeSuccess(out var materialDetails);

        // Wait for all events to be processed (domain events → integration events → consumers)
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsPublishedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was created in the Projects module
        using var context = TestingServer.CreateContext();

        var temp = context.Set<ReadOnlyMaterial>().ToList();

        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialDetails.Id);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe(materialName);
        readOnlyMaterial.PricePerUnit.ShouldBe(0.255); // 25.50 / 100
        readOnlyMaterial.Unit.ShouldBe(packageContent.Unit.ToString());
        readOnlyMaterial.UpdatedUtc.ShouldNotBe(default(DateTime));
    }

    [Test, NotInParallel(Order = 3)]
    public async Task CompleteEventFlow_UpdateMaterial_ShouldUpdateReadOnlyMaterialInProjectsModule()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Original Material");

        // Act - Update material through API (triggers domain event → integration event → consumer)
        var updateRequest = new UpdateMaterial.Request
        {
            Name = "Updated Material",
            PackageContentAmount = 200.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 35.75,
            PackagePriceCurrency = "USD"
        };

        var response = await TestingServer.PutAsync($"api/materials/{material.Id}", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was updated in the Projects module
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Updated Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0.17875); // 35.75 / 200
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task CompleteEventFlow_DeleteMaterial_ShouldDeleteReadOnlyMaterialInProjectsModule()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Material To Delete");

        // Verify material exists in read-only store
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
            readOnlyMaterial.ShouldNotBeNull();
        }

        // Act - Delete material through API (triggers domain event → integration event → consumer)
        var response = await TestingServer.DeleteAsync($"api/materials/{material.Id}", TestUser);
        response.ShouldBeSuccess();

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was deleted from the Projects module
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
            readOnlyMaterial.ShouldBeNull();
        }
    }

    [Test, NotInParallel(Order = 5)]
    public async Task CompleteEventFlow_MultipleOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var material1 = await CreateTestMaterialAsync("Material 1");
        var material2 = await CreateTestMaterialAsync("Material 2");

        // Act - Perform multiple operations
        // Update material 1
        var updateRequest1 = new UpdateMaterial.Request
        {
            Name = "Updated Material 1",
            PackageContentAmount = 150.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 30.00,
            PackagePriceCurrency = "USD"
        };
        await TestingServer.PutAsync($"api/materials/{material1.Id}", updateRequest1, TestUser);

        // Delete material 2
        await TestingServer.DeleteAsync($"api/materials/{material2.Id}", TestUser);

        // Create material 3
        var createRequest = new CreateMaterial.Request
        {
            Name = "Material 3",
            PackageContentAmount = 75.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 15.00,
            PackagePriceCurrency = "USD"
        };
        var createResponse = await TestingServer.PostAsync<MaterialDetails>("api/materials", createRequest, TestUser);
        createResponse.ShouldBeSuccess(out var material3);

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify all operations were processed correctly
        using var context = TestingServer.CreateContext();
        
        // Material 1 should be updated
        var readOnlyMaterial1 = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material1.Id);
        readOnlyMaterial1.ShouldNotBeNull();
        readOnlyMaterial1.Name.ShouldBe("Updated Material 1");
        readOnlyMaterial1.PricePerUnit.ShouldBe(0.2); // 30.00 / 150

        // Material 2 should be deleted
        var readOnlyMaterial2 = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material2.Id);
        readOnlyMaterial2.ShouldBeNull();

        // Material 3 should be created
        var readOnlyMaterial3 = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material3.Id);
        readOnlyMaterial3.ShouldNotBeNull();
        readOnlyMaterial3.Name.ShouldBe("Material 3");
        readOnlyMaterial3.PricePerUnit.ShouldBe(0.2); // 15.00 / 75
    }

    [Test, NotInParallel(Order = 6)]
    public async Task CompleteEventFlow_EventOrdering_ShouldProcessEventsInCorrectSequence()
    {
        // Arrange
        var material = await CreateTestMaterialAsync("Sequential Test Material");

        // Act - Perform rapid updates to test event ordering
        var update1 = new UpdateMaterial.Request
        {
            Name = "Update 1",
            PackageContentAmount = 200.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 40.00,
            PackagePriceCurrency = "USD"
        };
        await TestingServer.PutAsync($"api/materials/{material.Id}", update1, TestUser);

        var update2 = new UpdateMaterial.Request
        {
            Name = "Update 2",
            PackageContentAmount = 300.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 60.00,
            PackagePriceCurrency = "USD"
        };
        await TestingServer.PutAsync($"api/materials/{material.Id}", update2, TestUser);

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify final state reflects the last update
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Update 2");
        readOnlyMaterial.PricePerUnit.ShouldBe(0.2); // 60.00 / 300
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    #region Helper Methods

    private async Task<MaterialDetails> CreateTestMaterialAsync(string name)
    {
        var request = new CreateMaterial.Request
        {
            Name = name,
            PackageContentAmount = 100.0,
            PackageContentUnit = (int)PackageContentUnit.Milliliter,
            PackagePriceAmount = 25.50,
            PackagePriceCurrency = "USD"
        };

        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, TestUser);
        response.ShouldBeSuccess(out var materialDetails);
        return materialDetails;
    }

    #endregion
}
