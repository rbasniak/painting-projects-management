namespace PaintingProjectsManagement.Features.Projects.Tests;

public class Update_Project_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test projects for different users
        var existingProject = new Project("rodrigo.basniak", "Existing Project",  DateTime.UtcNow, null);
        var anotherUserProject = new Project("ricardo.smarzaro", "Another User Project", DateTime.UtcNow, null);
        var duplicateNameProject = new Project("rodrigo.basniak", "Duplicate Name Project", DateTime.UtcNow, null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingProject);
            await context.AddAsync(anotherUserProject);
            await context.AddAsync(duplicateNameProject);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
            savedProject.ShouldNotBeNull();

            var savedAnotherUserProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Another User Project");
            savedAnotherUserProject.ShouldNotBeNull();

            var savedDuplicateNameProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Duplicate Name Project");
            savedDuplicateNameProject.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Project()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Project_That_Does_Not_Exist()
    {
        // Prepare
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateProject.Request
        {
            Id = nonExistentId,
            Name = "Updated Project",
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == "Updated Project").ToList();
        projects.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Update_Project_That_Belongs_To_Another_User()
    {
        // Prepare
        var anotherUserProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Another User Project");
        anotherUserProject.TenantId.ShouldBe("RICARDO.SMARZARO", "Project should belong to another user");
        anotherUserProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = anotherUserProject.Id,
            Name = "Updated Project",
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act - Try to update as rodrigo.basniak (different user)
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - project should remain unchanged
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == anotherUserProject.Id);
        unchangedEntity.ShouldNotBeNull("Project should still exist in database");
        unchangedEntity.Name.ShouldBe("Another User Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 5)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Project_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = name!,
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Project_When_Name_Already_Exists()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        var duplicateNameProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Duplicate Name Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");
        duplicateNameProject.ShouldNotBeNull("Duplicate name project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Duplicate Name Project", // This name already exists for the same user
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A project with this name already exists.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Update_Project_When_Base64Image_Is_Invalid()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            Base64Image = "invalid-base64-image",
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Invalid base64 image format. Must be a valid base64 encoded image with proper header.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Cannot_Update_Project_When_EndDate_Is_In_Future()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            EndDate = DateTime.UtcNow.AddDays(1), // Future date
        };

        // Act
        var response = await TestingServer.PutAsync("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "End date cannot be in the future.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Project"); // Name should remain unchanged
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Can_Update_Project_With_Same_Name_As_Itself()
    {
        // Prepare
        var temp = TestingServer.CreateContext().Set<Project>().ToList();

        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Existing Project", // Same name as itself
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync<ProjectHeader>("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldBe(existingProject.Id);
        result.Name.ShouldBe("Existing Project");

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Existing Project");
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Can_Update_Project()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Updated Project",
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
            EndDate = DateTime.UtcNow,
        };

        // Act
        var response = await TestingServer.PutAsync<ProjectHeader>("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldBe(existingProject.Id);
        result.Name.ShouldBe("Updated Project");
        result.EndDate.ShouldNotBeNull();

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Updated Project");
        updatedEntity.EndDate.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Can_Update_Project_With_Same_Name_As_Another_User()
    {
        // Prepare
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Updated Project");
        var anotherUserProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Another User Project");
        existingProject.ShouldNotBeNull("Project should exist from previous test");
        anotherUserProject.ShouldNotBeNull("Another user project should exist from seed");

        var updateRequest = new UpdateProject.Request
        {
            Id = existingProject.Id,
            Name = "Another User Project", // This name belongs to another user
            Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PutAsync<ProjectHeader>("api/projects", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldBe(existingProject.Id);
        result.Name.ShouldBe("Another User Project");

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Another User Project");

        // Verify the other user's project is still intact
        var otherUserProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == anotherUserProject.Id);
        otherUserProject.ShouldNotBeNull();
        otherUserProject.Name.ShouldBe("Another User Project");
    }


    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 