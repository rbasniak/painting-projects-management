namespace PaintingProjectsManagement.Features.Projects.Tests;

public class Create_Project_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;


    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Project()
    {
         
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 