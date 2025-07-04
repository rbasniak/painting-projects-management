using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagment.Database;

namespace PaintingProjectsManagement.Features.Materials.Tests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<ApiTestFixture>
{
    protected readonly ApiTestFixture Fixture;

    protected IntegrationTestBase(ApiTestFixture fixture)
    {
        Fixture = fixture;
    }
    
    protected async Task<DatabaseContext> CreateScopedDbContextAsync()
    {
        var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        return dbContext;
    }
    
    protected static class TestData
    {
        public static readonly string ValidMaterialName = "Test Material";
        public static readonly MaterialUnit ValidMaterialUnit = MaterialUnit.Unit;
        public static readonly double ValidPricePerUnit = 10.50;
    }
}