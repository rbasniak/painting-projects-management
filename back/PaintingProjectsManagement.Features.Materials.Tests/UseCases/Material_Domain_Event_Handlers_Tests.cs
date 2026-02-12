using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

[HumanFriendlyDisplayName]
public class Material_Domain_Event_Handlers_Tests
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
    public async Task MaterialCreatedHandler_PublishesIntegrationEvent_WhenMaterialCreated()
    {
        // Arrange
        var materialName = "Test Material 1";
        var packageContent = new Quantity(100.0, PackageContentUnit.Mililiter);
        var packagePrice = new Money(25.50, "USD");

        // Act - Create material (this will raise MaterialCreated domain event)
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

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialCreatedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialCreatedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == materialDetails.Id &&
            envelope.Event.Name == materialName &&
            envelope.Event.PackageContentAmount == packageContent.Amount &&
            envelope.Event.PackageContentUnit == packageContent.Unit.ToString() &&
            envelope.Event.PackagePriceAmount == packagePrice.Amount &&
            envelope.Event.PackagePriceCurrency == packagePrice.CurrencyCode &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task MaterialUpdatedHandler_PublishesIntegrationEvent_WhenMaterialNameChanged()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Original Name 1");

        // Act - Update material name (this will raise MaterialNameChanged domain event)
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = "Updated Name",
            PackageContentAmount = material.PackageContent.Amount,
            PackageContentUnit = material.PackageContent.Unit.Id,
            PackagePriceAmount = material.PackagePrice.Amount,
            PackagePriceCurrency = material.PackagePrice.CurrencyCode
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialUpdatedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialUpdatedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == material.Id &&
            envelope.Event.Name == "Updated Name" &&
            envelope.Event.PackageContentAmount == material.PackageContent.Amount &&
            envelope.Event.PackageContentUnit == material.PackageContent.Unit.Value &&
            envelope.Event.PackagePriceAmount == material.PackagePrice.Amount &&
            envelope.Event.PackagePriceCurrency == material.PackagePrice.CurrencyCode &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task MaterialUpdatedHandler_PublishesIntegrationEvent_WhenMaterialPackageContentChanged()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Test Material 2");

        // Act - Update package content (this will raise MaterialPackageContentChanged domain event)
        var newPackageContent = new Quantity(200.0, PackageContentUnit.Mililiter);
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = material.Name,
            PackageContentAmount = newPackageContent.Amount,
            PackageContentUnit = (int)newPackageContent.Unit,
            PackagePriceAmount = material.PackagePrice.Amount,
            PackagePriceCurrency = material.PackagePrice.CurrencyCode
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialUpdatedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialUpdatedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == material.Id &&
            envelope.Event.Name == material.Name &&
            envelope.Event.PackageContentAmount == newPackageContent.Amount &&
            envelope.Event.PackageContentUnit == newPackageContent.Unit.ToString() &&
            envelope.Event.PackagePriceAmount == material.PackagePrice.Amount &&
            envelope.Event.PackagePriceCurrency == material.PackagePrice.CurrencyCode &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task MaterialUpdatedHandler_PublishesIntegrationEvent_WhenMaterialPackagePriceChanged()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Test Material 3");

        // Act - Update package price (this will raise MaterialPackagePriceChanged domain event)
        var newPackagePrice = new Money(35.75, "USD");
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = material.Name,
            PackageContentAmount = material.PackageContent.Amount,
            PackageContentUnit = material.PackageContent.Unit.Id,
            PackagePriceAmount = newPackagePrice.Amount,
            PackagePriceCurrency = newPackagePrice.CurrencyCode
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialUpdatedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialUpdatedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == material.Id &&
            envelope.Event.Name == material.Name &&
            envelope.Event.PackageContentAmount == material.PackageContent.Amount &&
            envelope.Event.PackageContentUnit == material.PackageContent.Unit.Value &&
            envelope.Event.PackagePriceAmount == newPackagePrice.Amount &&
            envelope.Event.PackagePriceCurrency == newPackagePrice.CurrencyCode &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task MaterialUpdatedHandler_PublishesIntegrationEvent_WhenMultiplePropertiesChanged()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Original Name 2");

        // Act - Update multiple properties (this will raise multiple domain events)
        var newPackageContent = new Quantity(150.0, PackageContentUnit.Mililiter);
        var newPackagePrice = new Money(30.00, "USD");
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = "Updated Name 2",
            PackageContentAmount = newPackageContent.Amount,
            PackageContentUnit = (int)newPackageContent.Unit,
            PackagePriceAmount = newPackagePrice.Amount,
            PackagePriceCurrency = newPackagePrice.CurrencyCode
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialUpdatedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialUpdatedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == material.Id &&
            envelope.Event.Name == "Updated Name 2" &&
            envelope.Event.PackageContentAmount == newPackageContent.Amount &&
            envelope.Event.PackageContentUnit == newPackageContent.Unit.ToString() &&
            envelope.Event.PackagePriceAmount == newPackagePrice.Amount &&
            envelope.Event.PackagePriceCurrency == newPackagePrice.CurrencyCode &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task MaterialDeletedHandler_PublishesIntegrationEvent_WhenMaterialDeleted()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Test Material For Deletion");

        // Act - Delete material (this will raise MaterialDeleted domain event)
        var response = await TestingServer.DeleteAsync($"api/materials/{material.Id}", TestUser);
        response.ShouldBeSuccess();

        // Wait for domain events to be processed
        await TestingServer.WaitForAllDomainEventsProcessedAsync();

        // Assert - Verify MaterialDeletedV1 integration event was published to outbox
        await TestingServer.AssertOutboxMessageAfterAsync<MaterialDeletedV1>(TestContext.Current.Execution.TestStart.Value.UtcDateTime, envelope =>
            envelope.Event.MaterialId == material.Id &&
            envelope.TenantId == "RODRIGO.BASNIAK" &&
            envelope.Username == TestUser);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task MaterialUpdatedHandler_DoesNotPublishIntegrationEvent_WhenNoPropertiesChanged()
    {
        // Arrange - Create a material first
        var material = await CreateTestMaterialAsync("Test Material");

        // Act - Update with same values (should not raise any domain events)
        var updateRequest = new UpdateMaterial.Request
        {
            Id = material.Id,
            Name = material.Name,
            PackageContentAmount = material.PackageContent.Amount,
            PackageContentUnit = material.PackageContent.Unit.Id,
            PackagePriceAmount = material.PackagePrice.Amount,
            PackagePriceCurrency = material.PackagePrice.CurrencyCode
        };

        var response = await TestingServer.PutAsync($"api/materials", updateRequest, TestUser);
        response.ShouldBeSuccess();

        // Wait a bit to ensure no events are processed
        await Task.Delay(2000);

        // Assert - Verify no MaterialUpdatedV1 integration event was published to outbox
        using var context = TestingServer.CreateContext();
        var outboxMessages = await context.Set<IntegrationOutboxMessage>()
            .Where(x => x.Name == "materials.material-updated.v1")
            .ToListAsync();

        outboxMessages.ShouldBeEmpty();
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
