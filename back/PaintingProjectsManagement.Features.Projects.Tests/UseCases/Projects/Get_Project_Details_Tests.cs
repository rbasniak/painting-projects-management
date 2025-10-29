using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagement.Features.Projects.Tests;

public class Get_Project_Details_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _testProjectId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test project with all related data
        var project = new Project("rodrigo.basniak", "Test Project", DateTime.UtcNow.AddDays(-10), null);
        
        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Project>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            
            await context.AddAsync(project);
            await context.SaveChangesAsync();
            
            _testProjectId = project.Id;
        }

        // Login with the user that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Project_Details()
    {
        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_Get_Their_Own_Project_Details()
    {
        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(_testProjectId);
        response.Data.Name.ShouldBe("Test Project");
        response.Data.StartDate.ShouldNotBeNull();
        response.Data.EndDate.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Project_Details_Returns_Empty_Collections_When_No_Related_Data()
    {
        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Verify empty collections
        response.Data.References.ShouldBeEmpty();
        response.Data.Pictures.ShouldBeEmpty();
        response.Data.Groups.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Project_Details_With_Materials_Returns_Correct_Data()
    {
        // Prepare
        Guid materialId;
        using (var context = TestingServer.CreateContext())
        {
            var newMaterial1 = new Material("rodrigo.basniak", "Test Material 1", new Materials.Quantity(9, PackageContentUnit.Gram), new Materials.Money(10.5, "BRL"));
            await context.AddAsync(newMaterial1);
            await context.SaveChangesAsync();
            materialId = newMaterial1.Id;
        }

        // Arrange - Add materials to the project
        using (var context = TestingServer.CreateContext())
        {
            var materialForProject = new MaterialForProject(_testProjectId, materialId, 7, PackageContentUnit.Each);
            await context.AddAsync(materialForProject);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Project_Details_With_References_Returns_Correct_Data()
    {
        // Arrange - Add references to the project
        Guid referenceId;
        using (var context = TestingServer.CreateContext())
        {
            var project = await context.Set<Project>().FirstAsync(x => x.Id == _testProjectId);
            var newReference = new ProjectReference(project.Id, "https://example.com/reference1.jpg");
            context.Add(newReference);
            await context.SaveChangesAsync();
            referenceId = newReference.Id;
        }

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.References.ShouldNotBeEmpty();
        response.Data.References.Length.ShouldBe(1);

        var reference = response.Data.References[0];
        reference.Url.ShouldBe("https://example.com/reference1.jpg");
        reference.Id.ShouldNotBe(Guid.Empty);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Project_Details_With_Pictures_Returns_Correct_Data()
    {
        // Arrange - Add pictures to the project
        Guid pictureId;
        using (var context = TestingServer.CreateContext())
        {
            var project = await context.Set<Project>().FirstAsync(x => x.Id == _testProjectId);
            var newPicture = new ProjectPicture(project.Id, "https://example.com/picture1.jpg");
            context.Add(newPicture);
            await context.SaveChangesAsync();
            pictureId = newPicture.Id;
        }

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Pictures.ShouldNotBeEmpty();
        response.Data.Pictures.Length.ShouldBe(1);

        var picture = response.Data.Pictures[0];
        picture.Url.ShouldBe("https://example.com/picture1.jpg");
        picture.Id.ShouldBe(pictureId);
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Project_Details_With_Color_Groups_Returns_Correct_Data()
    {
        // Arrange - Add color groups to the project
        Guid colorGroupId;
        using (var context = TestingServer.CreateContext())
        {
            var project = await context.Set<Project>().FirstAsync(x => x.Id == _testProjectId);
            var colorGroup = new ColorGroup(project.Id, "Test Color Group");
            context.Add(colorGroup);
            await context.SaveChangesAsync();
            colorGroupId = colorGroup.Id;
        }

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Groups.ShouldNotBeEmpty();
        response.Data.Groups.Length.ShouldBe(1);

        var group = response.Data.Groups[0];
        group.Name.ShouldBe("Test Color Group");
        group.Id.ShouldBe(colorGroupId);
    }

    [Test, Skip("Not implemented"), NotInParallel(Order = 9)]
    public async Task Project_Details_With_Steps_Returns_Correct_Data()
    {
        //// Arrange - Add steps to the project
        //using (var context = TestingServer.CreateContext())
        //{
        //    var project = await context.Set<Project>().FirstAsync(x => x.Id == _testProjectId);
        //    var startDate = DateTime.UtcNow.AddDays(-5);
        //    var endDate = DateTime.UtcNow.AddDays(-3);

        //    project.Steps.AddPlanningStep(startDate, endDate, 2.5);
        //    project.Steps.AddPaintingStep(startDate, endDate, 3.0);
        //    await context.SaveChangesAsync();
        //}

        //// Act
        //var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{_testProjectId}", "rodrigo.basniak");

        //// Assert the response
        //response.ShouldBeSuccess();
        //response.Data.ShouldNotBeNull();
        //response.Data.Steps.ShouldNotBeNull();

        //// Verify planning steps
        //response.Data.Steps.Planning.ShouldNotBeEmpty();
        //response.Data.Steps.Planning.Length.ShouldBe(1);
        //var planningStep = response.Data.Steps.Planning[0];
        //planningStep.Duration.ShouldBe(2.5);
        //planningStep.StartDate.ShouldBe(DateTime.UtcNow.AddDays(-5).Date);
        //planningStep.EndDate.ShouldBe(DateTime.UtcNow.AddDays(-3).Date);

        //// Verify painting steps
        //response.Data.Steps.Painting.ShouldNotBeEmpty();
        //response.Data.Steps.Painting.Length.ShouldBe(1);
        //var paintingStep = response.Data.Steps.Painting[0];
        //paintingStep.Duration.ShouldBe(3.0);
        //paintingStep.StartDate.ShouldBe(DateTime.UtcNow.AddDays(-5).Date);
        //paintingStep.EndDate.ShouldBe(DateTime.UtcNow.AddDays(-3).Date);
    }

    [Test, NotInParallel(Order = 10)]
    public async Task Project_Details_Returns_NotFound_For_Non_Existent_Project()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{nonExistentId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Cannot_Access_Project_From_Other_User()
    {
        // Arrange - Create a project for a different user
        var otherUserProject = new Project("ricardo.smarzaro", "Other User Project", DateTime.UtcNow.AddDays(-5), null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(otherUserProject);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{otherUserProject.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, Skip("Not implemented"), NotInParallel(Order = 12)]
    public async Task Project_Details_With_All_Related_Data_Returns_Complete_Information()
    {
        //// Arrange - Create a comprehensive project with all data
        //var comprehensiveProject = new Project("rodrigo.basniak", "Comprehensive Project", "https://example.com/comprehensive.jpg", DateTime.UtcNow.AddDays(-15));

        //using (var context = TestingServer.CreateContext())
        //{
        //    await context.AddAsync(comprehensiveProject);
        //    await context.SaveChangesAsync();

        //    // Add materials
        //    var materialForProject = new MaterialForProject(comprehensiveProject.Id, _testMaterialId, 1.5);
        //    await context.AddAsync(materialForProject);

        //    // Add references and pictures
        //    var reference = new ProjectReference("https://example.com/ref.jpg");
        //    var picture = new ProjectPicture("https://example.com/pic.jpg");
        //    comprehensiveProject.AddReference(reference);
        //    comprehensiveProject.AddPicture(picture);

        //    // Add color group
        //    var colorGroup = new ColorGroup("Test Group");
        //    comprehensiveProject.AddColorGroup(colorGroup);

        //    // Add steps
        //    var startDate = DateTime.UtcNow.AddDays(-10);
        //    var endDate = DateTime.UtcNow.AddDays(-8);
        //    comprehensiveProject.Steps.AddPlanningStep(startDate, endDate, 2.0);
        //    comprehensiveProject.Steps.AddPaintingStep(startDate, endDate, 4.0);

        //    await context.SaveChangesAsync();
        //}

        //// Act
        //var response = await TestingServer.GetAsync<ProjectDetails>($"api/projects/{comprehensiveProject.Id}", "rodrigo.basniak");

        //// Assert the response
        //response.ShouldBeSuccess();
        //response.Data.ShouldNotBeNull();

        //// Verify basic properties
        //response.Data.Name.ShouldBe("Comprehensive Project");
        //response.Data.PictureUrl.ShouldBe("https://example.com/comprehensive.jpg");
        //response.Data.StartDate.ShouldNotBeNull();
        //response.Data.EndDate.ShouldBeNull();

        //// Verify materials
        //response.Data.Materials.ShouldNotBeEmpty();
        //response.Data.Materials.Length.ShouldBe(1);

        //// Verify references
        //response.Data.References.ShouldNotBeEmpty();
        //response.Data.References.Length.ShouldBe(1);

        //// Verify pictures
        //response.Data.Pictures.ShouldNotBeEmpty();
        //response.Data.Pictures.Length.ShouldBe(1);

        //// Verify color groups
        //response.Data.Groups.ShouldNotBeEmpty();
        //response.Data.Groups.Length.ShouldBe(1);

        //// Verify steps
        //response.Data.Steps.Planning.ShouldNotBeEmpty();
        //response.Data.Steps.Painting.ShouldNotBeEmpty();
        //response.Data.Steps.Preparation.ShouldBeEmpty();
        //response.Data.Steps.Supporting.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 