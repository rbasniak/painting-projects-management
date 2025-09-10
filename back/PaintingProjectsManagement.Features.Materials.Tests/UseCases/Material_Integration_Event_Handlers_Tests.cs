using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using PaintingProjectsManagement.Features.Projects;
using PaintingProjectsManagement.Testing.Core;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Material_Integration_Event_Handlers_Tests
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
    public async Task MaterialCreatedConsumer_CreatesReadOnlyMaterial_WhenMaterialCreatedEventReceived()
    {
        // Arrange
        var materialId = Guid.NewGuid();
        var integrationEvent = new MaterialCreatedV1(
            materialId,
            "Test Material",
            100.0,
            "Milliliters",
            25.50,
            "USD"
        );

        // Act - Publish integration event directly to outbox
        await TestingServer.PublishIntegrationEventAsync(integrationEvent, "RODRIGO.BASNIAK", TestUser);
        
        // Wait for integration events to be processed
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was created
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Test Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0.255); // 25.50 / 100
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
        readOnlyMaterial.UpdatedUtc.ShouldNotBe(default(DateTime));
    }

    [Test, NotInParallel(Order = 3)]
    public async Task MaterialUpdatedConsumer_UpdatesReadOnlyMaterial_WhenMaterialUpdatedEventReceived()
    {
        // Arrange - First create a read-only material
        var materialId = Guid.NewGuid();
        var createEvent = new MaterialCreatedV1(
            materialId,
            "Original Material",
            100.0,
            "Milliliters",
            25.50,
            "USD"
        );

        await TestingServer.PublishIntegrationEventAsync(createEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Act - Publish update event
        var updateEvent = new MaterialUpdatedV1(
            materialId,
            "Updated Material",
            200.0,
            "Milliliters",
            35.75,
            "USD"
        );

        await TestingServer.PublishIntegrationEventAsync(updateEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was updated
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Updated Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0.17875); // 35.75 / 200
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task MaterialUpdatedConsumer_CreatesReadOnlyMaterial_WhenMaterialUpdatedEventReceivedForNonExistentMaterial()
    {
        // Arrange
        var materialId = Guid.NewGuid();
        var updateEvent = new MaterialUpdatedV1(
            materialId,
            "New Material",
            150.0,
            "Milliliters",
            30.00,
            "USD"
        );

        // Act - Publish update event for non-existent material
        await TestingServer.PublishIntegrationEventAsync(updateEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was created
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("New Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0.2); // 30.00 / 150
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task MaterialDeletedConsumer_DeletesReadOnlyMaterial_WhenMaterialDeletedEventReceived()
    {
        // Arrange - First create a read-only material
        var materialId = Guid.NewGuid();
        var createEvent = new MaterialCreatedV1(
            materialId,
            "Material To Delete",
            100.0,
            "Milliliters",
            25.50,
            "USD"
        );

        await TestingServer.PublishIntegrationEventAsync(createEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Verify material exists
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
            readOnlyMaterial.ShouldNotBeNull();
        }

        // Act - Publish delete event
        var deleteEvent = new MaterialDeletedV1(materialId);
        await TestingServer.PublishIntegrationEventAsync(deleteEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was deleted
        using (var context = TestingServer.CreateContext())
        {
            var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
                .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
            readOnlyMaterial.ShouldBeNull();
        }
    }

    [Test, NotInParallel(Order = 6)]
    public async Task MaterialDeletedConsumer_HandlesGracefully_WhenMaterialDoesNotExist()
    {
        // Arrange
        var materialId = Guid.NewGuid();
        var deleteEvent = new MaterialDeletedV1(materialId);

        // Act - Publish delete event for non-existent material
        await TestingServer.PublishIntegrationEventAsync(deleteEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Should not throw exception and complete successfully
        // (The handler should handle the case where the material doesn't exist gracefully)
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        readOnlyMaterial.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task MaterialCreatedConsumer_HandlesZeroPackageContent_WhenCalculatingUnitPrice()
    {
        // Arrange
        var materialId = Guid.NewGuid();
        var integrationEvent = new MaterialCreatedV1(
            materialId,
            "Test Material",
            0.0, // Zero package content
            "Milliliters",
            25.50,
            "USD"
        );

        // Act - Publish integration event with zero package content
        await TestingServer.PublishIntegrationEventAsync(integrationEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was created with zero unit price
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Test Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0); // Should be 0 when package content is 0
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    [Test, NotInParallel(Order = 8)]
    public async Task MaterialUpdatedConsumer_HandlesZeroPackageContent_WhenCalculatingUnitPrice()
    {
        // Arrange - First create a read-only material
        var materialId = Guid.NewGuid();
        var createEvent = new MaterialCreatedV1(
            materialId,
            "Original Material",
            100.0,
            "Milliliters",
            25.50,
            "USD"
        );

        await TestingServer.PublishIntegrationEventAsync(createEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Act - Publish update event with zero package content
        var updateEvent = new MaterialUpdatedV1(
            materialId,
            "Updated Material",
            0.0, // Zero package content
            "Milliliters",
            35.75,
            "USD"
        );

        await TestingServer.PublishIntegrationEventAsync(updateEvent, "RODRIGO.BASNIAK", TestUser);
        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify read-only material was updated with zero unit price
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldNotBeNull();
        readOnlyMaterial.Name.ShouldBe("Updated Material");
        readOnlyMaterial.PricePerUnit.ShouldBe(0); // Should be 0 when package content is 0
        readOnlyMaterial.Unit.ShouldBe("Milliliters");
    }

    [Test, NotInParallel(Order = 9)]
    public async Task MaterialIntegrationEventHandlers_ProcessEventsInCorrectOrder()
    {
        // Arrange
        var materialId = Guid.NewGuid();

        // Act - Publish events in sequence
        var createEvent = new MaterialCreatedV1(materialId, "Material", 100.0, "Milliliters", 25.50, "USD");
        await TestingServer.PublishIntegrationEventAsync(createEvent, "RODRIGO.BASNIAK", TestUser);

        var updateEvent = new MaterialUpdatedV1(materialId, "Updated Material", 200.0, "Milliliters", 35.75, "USD");
        await TestingServer.PublishIntegrationEventAsync(updateEvent, "RODRIGO.BASNIAK", TestUser);

        var deleteEvent = new MaterialDeletedV1(materialId);
        await TestingServer.PublishIntegrationEventAsync(deleteEvent, "RODRIGO.BASNIAK", TestUser);

        await TestingServer.WaitForAllIntegrationEventsProcessedAsync();

        // Assert - Verify final state (material should be deleted)
        using var context = TestingServer.CreateContext();
        var readOnlyMaterial = await context.Set<ReadOnlyMaterial>()
            .FirstOrDefaultAsync(m => m.Tenant == "RODRIGO.BASNIAK" && m.Id == materialId);
        
        readOnlyMaterial.ShouldBeNull();
    } 
}
