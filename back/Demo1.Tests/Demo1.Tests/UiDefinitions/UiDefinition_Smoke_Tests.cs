using Demo1.Tests;

namespace rbkApiModules.Identity.Tests.Claims;

public class UiDefinition_Smoke_Tests
{
    [ClassDataSource<Demo1TestingServer>(Shared = SharedType.PerClass)]
    public required Demo1TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Global_Admin_Can_Create_Claim()
    {
        var response = await TestingServer.GetAsync("api/ui-definitions");

        response.ShouldBeSuccess();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.Context.Database.EnsureDeletedAsync();
    }
}