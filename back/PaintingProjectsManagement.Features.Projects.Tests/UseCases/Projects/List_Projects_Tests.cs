namespace PaintingProjectsManagement.Features.Projects.Tests;

public class List_Projects_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test projects for different users with various states
        var rodrigoUnfinishedProject1 = new Project("rodrigo.basniak", "Rodrigo Unfinished Project 1", "https://example.com/pic1.jpg", DateTime.UtcNow.AddDays(-10));
        var rodrigoUnfinishedProject2 = new Project("rodrigo.basniak", "Rodrigo Unfinished Project 2", "https://example.com/pic2.jpg", DateTime.UtcNow.AddDays(-5));
        var rodrigoFinishedProject1 = new Project("rodrigo.basniak", "Rodrigo Finished Project 1", "https://example.com/pic3.jpg", DateTime.UtcNow.AddDays(-20));
        var rodrigoFinishedProject2 = new Project("rodrigo.basniak", "Rodrigo Finished Project 2", "https://example.com/pic4.jpg", DateTime.UtcNow.AddDays(-15));
        
        var ricardoUnfinishedProject1 = new Project("ricardo.smarzaro", "Ricardo Unfinished Project 1", "https://example.com/pic5.jpg", DateTime.UtcNow.AddDays(-8));
        var ricardoFinishedProject1 = new Project("ricardo.smarzaro", "Ricardo Finished Project 1", "https://example.com/pic6.jpg", DateTime.UtcNow.AddDays(-25));
        var ricardoFinishedProject2 = new Project("ricardo.smarzaro", "Ricardo Finished Project 2", "https://example.com/pic7.jpg", DateTime.UtcNow.AddDays(-30));

        // Set end dates for finished projects
        rodrigoFinishedProject1.UpdateDetails(rodrigoFinishedProject1.Name, rodrigoFinishedProject1.PictureUrl, rodrigoFinishedProject1.StartDate, DateTime.UtcNow.AddDays(-5));
        rodrigoFinishedProject2.UpdateDetails(rodrigoFinishedProject2.Name, rodrigoFinishedProject2.PictureUrl, rodrigoFinishedProject2.StartDate, DateTime.UtcNow.AddDays(-3));
        ricardoFinishedProject1.UpdateDetails(ricardoFinishedProject1.Name, ricardoFinishedProject1.PictureUrl, ricardoFinishedProject1.StartDate, DateTime.UtcNow.AddDays(-10));
        ricardoFinishedProject2.UpdateDetails(ricardoFinishedProject2.Name, ricardoFinishedProject2.PictureUrl, ricardoFinishedProject2.StartDate, DateTime.UtcNow.AddDays(-8));

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Project>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            // Add projects in mixed order to test sorting
            await context.AddAsync(ricardoFinishedProject2);
            await context.AddAsync(rodrigoUnfinishedProject2);
            await context.AddAsync(rodrigoFinishedProject1);
            await context.AddAsync(ricardoUnfinishedProject1);
            await context.AddAsync(rodrigoUnfinishedProject1);
            await context.AddAsync(ricardoFinishedProject1);
            await context.AddAsync(rodrigoFinishedProject2);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var rodrigoProjects = context.Set<Project>().Where(x => x.TenantId == "RODRIGO.BASNIAK").ToList();
            rodrigoProjects.Count.ShouldBe(4);

            var ricardoProjects = context.Set<Project>().Where(x => x.TenantId == "RICARDO.SMARZARO").ToList();
            ricardoProjects.Count.ShouldBe(3);
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Projects()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Their_Own_Projects()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(4);

        // Verify the projects belong to the correct user
        var projectNames = response.Data.Select(x => x.Name).ToList();
        projectNames.ShouldContain("Rodrigo Unfinished Project 1");
        projectNames.ShouldContain("Rodrigo Unfinished Project 2");
        projectNames.ShouldContain("Rodrigo Finished Project 1");
        projectNames.ShouldContain("Rodrigo Finished Project 2");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_See_Projects_From_Other_Users()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects", "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // Verify the projects belong to the correct user
        var projectNames = response.Data.Select(x => x.Name).ToList();
        projectNames.ShouldContain("Ricardo Unfinished Project 1");
        projectNames.ShouldContain("Ricardo Finished Project 1");
        projectNames.ShouldContain("Ricardo Finished Project 2");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Projects_Are_Returned_With_Correct_Properties()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(4);

        // Verify project properties are correctly mapped
        var unfinishedProject1 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Unfinished Project 1");
        unfinishedProject1.ShouldNotBeNull();
        unfinishedProject1.PictureUrl.ShouldBe("https://example.com/pic1.jpg");
        unfinishedProject1.EndDate.ShouldBeNull(); // Unfinished project

        var finishedProject1 = response.Data.FirstOrDefault(x => x.Name == "Rodrigo Finished Project 1");
        finishedProject1.ShouldNotBeNull();
        finishedProject1.PictureUrl.ShouldBe("https://example.com/pic3.jpg");
        finishedProject1.EndDate.ShouldNotBeNull(); // Finished project
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Projects_Are_Ordered_Correctly()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(4);

        // Verify ordering: unfinished projects first, then finished projects by end date (most recent first), then alphabetically
        var projects = response.Data.ToList();
        
        // First two should be unfinished projects (EndDate == null)
        projects[0].EndDate.ShouldBeNull();
        projects[1].EndDate.ShouldBeNull();
        
        // Next two should be finished projects ordered by EndDate (most recent first)
        projects[2].EndDate.ShouldNotBeNull();
        projects[3].EndDate.ShouldNotBeNull();
        
        // Finished projects should be ordered by EndDate descending (most recent first)
        projects[2].EndDate!.Value.ShouldBeGreaterThan(projects[3].EndDate!.Value);
        
        // Within each group (unfinished/finished), projects should be ordered alphabetically by name
        var unfinishedProjects = projects.Where(p => p.EndDate == null).ToList();
        var finishedProjects = projects.Where(p => p.EndDate != null).ToList();
        
        // Check alphabetical ordering within unfinished projects
        unfinishedProjects[0].Name.ShouldBe("Rodrigo Unfinished Project 1");
        unfinishedProjects[1].Name.ShouldBe("Rodrigo Unfinished Project 2");
        
        // Check alphabetical ordering within finished projects
        finishedProjects[0].Name.ShouldBe("Rodrigo Finished Project 2");
        finishedProjects[1].Name.ShouldBe("Rodrigo Finished Project 1");
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Project_Ids_Are_Correctly_Mapped()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ProjectHeader>>("api/projects", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(4);

        // Verify all projects have valid GUIDs
        foreach (var project in response.Data)
        {
            project.Id.ShouldNotBe(Guid.Empty);
        }

        // Verify projects have unique IDs
        var projectIds = response.Data.Select(x => x.Id).ToList();
        projectIds.Count.ShouldBe(projectIds.Distinct().Count());
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 