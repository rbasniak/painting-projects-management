namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Update_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand and lines
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var existingLine = new PaintLine(brand, "Existing Line");
            var duplicateNameLine = new PaintLine(brand, "Duplicate Name Line");

            await context.AddAsync(existingLine);
            await context.AddAsync(duplicateNameLine);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedLine = context.Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
            savedLine.ShouldNotBeNull();

            var savedDuplicateLine = context.Set<PaintLine>().FirstOrDefault(x => x.Name == "Duplicate Name Line");
            savedDuplicateLine.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Paint_Line()
    {
        // Prepare
        var existingLine = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
        existingLine.ShouldNotBeNull("Line should exist from seed");

        var updateRequest = new UpdatePaintLine.Request
        {
            Id = existingLine.Id,
            Name = "Updated Line"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/lines", updateRequest);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == existingLine.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Line");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Paint_Line_That_Does_Not_Exist()
    {
        // Prepare
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdatePaintLine.Request
        {
            Id = nonExistentId,
            Name = "Updated Line"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/lines", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Updated Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Paint_Line_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var existingLine = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
        existingLine.ShouldNotBeNull("Line should exist from seed");

        var updateRequest = new UpdatePaintLine.Request
        {
            Id = existingLine.Id,
            Name = name!
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/lines", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == existingLine.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Line");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Paint_Line_When_Name_Already_Exists_In_Same_Brand()
    {
        // Prepare
        var existingLine = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
        existingLine.ShouldNotBeNull("Line should exist from seed");

        var updateRequest = new UpdatePaintLine.Request
        {
            Id = existingLine.Id,
            Name = "Duplicate Name Line" // This name already exists in same brand
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/lines", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint line with this name already exists for this brand.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == existingLine.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Line");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Paint_Line_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var existingLine = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
        existingLine.ShouldNotBeNull("Line should exist from seed");

        var updateRequest = new UpdatePaintLine.Request
        {
            Id = existingLine.Id,
            Name = new string('A', 101) // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/lines", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == existingLine.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Line");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Update_Paint_Line()
    {
        // Prepare
        var existingLine = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Existing Line");
        existingLine.ShouldNotBeNull("Line should exist from seed");

        var updateRequest = new UpdatePaintLine.Request
        {
            Id = existingLine.Id,
            Name = "Updated Line"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("/api/paints/lines", updateRequest, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingLine.Id);
        result.Name.ShouldBe("Updated Line");

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == existingLine.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Updated Line");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
