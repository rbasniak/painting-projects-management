using PaintingProjectsManagement.Features.Models.Tests;

namespace PaintingProjectsManagement.Features.Models.Tests;

public class Create_Model_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1Category1;
    private static Guid _tenant1Category2;
    private static Guid _tenant2Category;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test model category for the tests
        var tenant1TestCategory1 = new ModelCategory("rodrigo.basniak", "Tenant 1 Test Category");
        var tenant1TestCategory2 = new ModelCategory("rodrigo.basniak", "Tenant 1 Other Test Category");
        var tenant2TestCategory = new ModelCategory("ricardo.smarzaro", "Tenant 2 Other Tenant Test Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(tenant1TestCategory1);
            await context.SaveChangesAsync();
            _tenant1Category1 = tenant1TestCategory1.Id;

            await context.AddAsync(tenant1TestCategory2);
            await context.SaveChangesAsync();
            _tenant1Category2 = tenant1TestCategory2.Id;

            await context.AddAsync(tenant2TestCategory);
            await context.SaveChangesAsync();
            _tenant2Category = tenant2TestCategory.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }
     

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 