using PaintingProjectsManagement.Features.Projects;
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
        var testStartTime = DateTime.UtcNow;

        // Arrange
        var materialName = "End-to-End Test Material";
        var packageContent = new Quantity(100.0, PackageContentUnit.Mililiter);
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
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialCreatedConsumer)] = 1,
        });

        // Assert - Verify read-only material was created in the Projects module
        using var context = TestingServer.CreateContext();

        var readOnlyMaterial = await context.Set<Material>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialDetails.Id);

        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe(materialName);
        readOnlyMaterial.PricePerUnit.Amount.ShouldBe(0.255); // 25.50 / 100
        readOnlyMaterial.Unit.ShouldBe(MaterialUnit.Mililiter);
        readOnlyMaterial.UpdatedUtc.ShouldNotBe(default(DateTime));
    }

    [Test, NotInParallel(Order = 3)]
    public async Task CompleteEventFlow_UpdateMaterial_ShouldUpdateReadOnlyMaterialInProjectsModule()
    {
        var testStartTime = DateTime.UtcNow;

        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Original Material");

        // Act - Update material through API (triggers domain event → integration event → consumer)
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = "Updated Material",
            PackageContentAmount = 200.0,
            PackageContentUnit = (int)PackageContentUnit.Mililiter,
            PackagePriceAmount = 35.75,
            PackagePriceCurrency = "USD"
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialUpdatedConsumer)] = 3,
        });

        // Assert - Verify read-only material was updated in the Projects module
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<Material>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);

        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Updated Material");
        readOnlyMaterial.PricePerUnit.Amount.ShouldBe(0.17875); // 35.75 / 200
        readOnlyMaterial.Unit.ShouldBe(MaterialUnit.Mililiter);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task CompleteEventFlow_DeleteMaterial_ShouldDeleteReadOnlyMaterialInProjectsModule()
    {
        var testStartTime = DateTime.UtcNow;

        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Material To Delete");

        // Verify material exists in read-only store
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
            readOnlyMaterial.ShouldBeNull();
        }

        // Act - Delete material through API (triggers domain event → integration event → consumer)
        var response = await TestingServer.DeleteAsync($"api/materials/{material.Id}", TestUser);
        response.ShouldBeSuccess();

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialDeletedConsumer)] = 1,
        });

        // Assert - Verify read-only material was deleted from the Projects module
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);
            readOnlyMaterial.ShouldBeNull();
        }
    }

    [Test, NotInParallel(Order = 5)]
    public async Task CompleteEventFlow_MultipleOperations_ShouldMaintainConsistency()
    {
        var testStartTime = DateTime.UtcNow;

        // Arrange
        var material1 = await CreateTestMaterialAsync("Material 1");
        var material2 = await CreateTestMaterialAsync("Material 2");

        // Act - Perform multiple operations
        // Update material 1
        var updateRequest1 = new UpdateMaterial.Request
        {
            Id = material1.Id,
            Name = "Updated Material 1",
            PackageContentAmount = 150.0,
            PackageContentUnit = (int)PackageContentUnit.Mililiter,
            PackagePriceAmount = 30.00,
            PackagePriceCurrency = "USD"
        };
        await TestingServer.PutAsync($"api/materials", updateRequest1, TestUser);

        // Delete material 2
        await TestingServer.DeleteAsync($"api/materials/{material2.Id}", TestUser);

        // Create material 3
        var createRequest = new CreateMaterial.Request
        {
            Name = "Material 3",
            PackageContentAmount = 75.0,
            PackageContentUnit = (int)PackageContentUnit.Mililiter,
            PackagePriceAmount = 15.00,
            PackagePriceCurrency = "USD"
        };
        var createResponse = await TestingServer.PostAsync<MaterialDetails>("api/materials", createRequest, TestUser);
        createResponse.ShouldBeSuccess(out var material3);

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialUpdatedConsumer)] = 3,
            [typeof(MaterialCreatedConsumer)] = 3,
            [typeof(MaterialDeletedConsumer)] = 1,
        });

        // Assert - Verify all operations were processed correctly
        using var context = TestingServer.CreateContext();

        // Material 1 should be updated
        var readOnlyMaterial1 = await context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material1.Id);
        readOnlyMaterial1.ShouldNotBeNull();
        readOnlyMaterial1.Name.ShouldBe("Updated Material 1");
        readOnlyMaterial1.PricePerUnit.Amount.ShouldBe(0.2); // 30.00 / 150

        // Material 2 should be deleted
        var readOnlyMaterial2 = await context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material2.Id);
        readOnlyMaterial2.ShouldBeNull();

        // Material 3 should be created
        var readOnlyMaterial3 = await context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material3.Id);
        readOnlyMaterial3.ShouldNotBeNull();
        readOnlyMaterial3.Name.ShouldBe("Material 3");
        readOnlyMaterial3.PricePerUnit.Amount.ShouldBe(0.2); // 15.00 / 75
    }

    [Test, NotInParallel(Order = 6)]
    public async Task CompleteEventFlow_EventOrdering_ShouldProcessEventsInCorrectSequence()
    {
        var testStartTime = DateTime.UtcNow;

        // Arrange
        var material = await CreateTestMaterialAsync("Sequential Test Material");

        // Act - Perform rapid updates to test event ordering
        var update1 = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = "Update 1",
            PackageContentAmount = material.PackageContent.Amount,
            PackageContentUnit = material.PackageContent.Unit.Id,
            PackagePriceAmount = 40.00,
            PackagePriceCurrency = "USD"
        };
        var updateResponse1 = await TestingServer.PutAsync($"api/materials", update1, TestUser);
        updateResponse1.ShouldBeSuccess();

        var update2 = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = "Update 2",
            PackageContentAmount = 300.0,
            PackageContentUnit = (int)PackageContentUnit.Mililiter,
            PackagePriceAmount = update1.PackagePriceAmount,
            PackagePriceCurrency = update1.PackagePriceCurrency
        };
        var updateResponse2 = await TestingServer.PutAsync($"api/materials", update2, TestUser);
        updateResponse2.ShouldBeSuccess();

        // Wait for all events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        var temp = TestingServer.CreateContext().Set<DomainOutboxMessage>().Where(x => x.ProcessedUtc != null && x.ProcessedUtc > testStartTime).ToList();

        await TestingServer.WaitForAllIntegrationEventsProcessedAsync(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialUpdatedConsumer)] = 1,
            [typeof(MaterialUpdatedConsumer)] = 4,
        });

        // Assert - Verify final state reflects the last update
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<Material>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == material.Id);

        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Update 2");
        readOnlyMaterial.PricePerUnit.Amount.ShouldBe(40.0 / 300.0); // 40.00 / 300
        readOnlyMaterial.Unit.ShouldBe(MaterialUnit.Mililiter);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }

    #region Helper Methods

    private async Task<MaterialDetails> CreateTestMaterialAsync(string name)
    {
        var request = new CreateMaterial.Request
        {
            Name = name,
            PackageContentAmount = 100.0,
            PackageContentUnit = (int)PackageContentUnit.Mililiter,
            PackagePriceAmount = 25.50,
            PackagePriceCurrency = "USD"
        };

        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, TestUser);
        response.ShouldBeSuccess(out var materialDetails);
        return materialDetails;
    }

    #endregion
}
