namespace PaintingProjectsManagement.Features.Inventory.Integrations.Tests;

[HumanFriendlyDisplayName]
public class FindColorMatches_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test paint brands, lines, colors, and user paints
        var citadelBrand = new PaintBrand("Citadel");
        var vallejoBrand = new PaintBrand("Vallejo");
        var armyPainterBrand = new PaintBrand("Army Painter");

        var citadelBaseLine = new PaintLine(citadelBrand, "Base");
        var vallejoModelColorLine = new PaintLine(vallejoBrand, "Model Color");
        var armyPainterWarpaints = new PaintLine(armyPainterBrand, "Warpaints");

        // Create paints with different colors for rodrigo.basniak
        var rodrigoRedPaint = new PaintColor(citadelBaseLine, "Mephiston Red", "#FF0000", 12.0, PaintType.Opaque, "RED-001");
        var rodrigoBlueGreenPaint = new PaintColor(vallejoModelColorLine, "Blue Green", "#0080FF", 17.0, PaintType.Opaque, "BG-001");
        var rodrigoDarkRedPaint = new PaintColor(citadelBaseLine, "Khorne Red", "#AA0000", 12.0, PaintType.Opaque, "RED-002");
        var rodrigoOrangePaint = new PaintColor(armyPainterWarpaints, "Pure Red", "#FF4500", 18.0, PaintType.Opaque, "OR-001");
        var rodrigoYellowPaint = new PaintColor(vallejoModelColorLine, "Yellow", "#FFFF00", 17.0, PaintType.Opaque, "YEL-001");
        var rodrigoGreenPaint = new PaintColor(citadelBaseLine, "Caliban Green", "#00FF00", 12.0, PaintType.Opaque, "GRN-001");

        // Create paints with different colors for ricardo.smarzaro
        var ricardoPurplePaint = new PaintColor(vallejoModelColorLine, "Purple", "#800080", 17.0, PaintType.Metallic, "PUR-001");
        var ricardoCyanPaint = new PaintColor(armyPainterWarpaints, "Cyan", "#00FFFF", 18.0, PaintType.Opaque, "CYA-001");
        var ricardoMagentaPaint = new PaintColor(citadelBaseLine, "Magenta", "#FF00FF", 12.0, PaintType.Opaque, "MAG-001");

        using (var context = TestingServer.CreateContext())
        {
            // Clean up existing data
            await context.Set<UserPaint>().ExecuteDeleteAsync();
            await context.Set<PaintColor>().ExecuteDeleteAsync();
            await context.Set<PaintLine>().ExecuteDeleteAsync();
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            // Add brands, lines, and colors
            await context.AddAsync(citadelBrand);
            await context.AddAsync(vallejoBrand);
            await context.AddAsync(armyPainterBrand);
            await context.SaveChangesAsync();

            await context.AddAsync(citadelBaseLine);
            await context.AddAsync(vallejoModelColorLine);
            await context.AddAsync(armyPainterWarpaints);
            await context.SaveChangesAsync();

            await context.AddAsync(rodrigoRedPaint);
            await context.AddAsync(rodrigoBlueGreenPaint);
            await context.AddAsync(rodrigoDarkRedPaint);
            await context.AddAsync(rodrigoOrangePaint);
            await context.AddAsync(rodrigoYellowPaint);
            await context.AddAsync(rodrigoGreenPaint);
            await context.AddAsync(ricardoPurplePaint);
            await context.AddAsync(ricardoCyanPaint);
            await context.AddAsync(ricardoMagentaPaint);
            await context.SaveChangesAsync();

            // Add user paints for rodrigo.basniak
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoRedPaint.Id));
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoBlueGreenPaint.Id));
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoDarkRedPaint.Id));
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoOrangePaint.Id));
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoYellowPaint.Id));
            await context.AddAsync(new UserPaint("rodrigo.basniak", rodrigoGreenPaint.Id));

            // Add user paints for ricardo.smarzaro
            await context.AddAsync(new UserPaint("ricardo.smarzaro", ricardoPurplePaint.Id));
            await context.AddAsync(new UserPaint("ricardo.smarzaro", ricardoCyanPaint.Id));
            await context.AddAsync(new UserPaint("ricardo.smarzaro", ricardoMagentaPaint.Id));

            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var rodrigoPaints = context.Set<UserPaint>().Where(x => x.Username.ToLower() == "rodrigo.basniak").ToList();
            rodrigoPaints.Count.ShouldBe(6);

            var ricardoPaints = context.Set<UserPaint>().Where(x => x.Username.ToLower() == "ricardo.smarzaro").ToList();
            ricardoPaints.Count.ShouldBe(3);
        }
    }

    private async Task<QueryResponse<IReadOnlyCollection<ColorMatchResult>>> ExecuteQuery(string tenantId, string referenceColor, int maxResults)
    {
        using var scope = TestingServer.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
        
        // Create query and set identity using reflection
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = referenceColor,
            MaxResults = maxResults
        };

        // Manually set identity because we're bypassing http context
        query.SetIdentity(tenantId, tenantId, []);

        var result = await dispatcher.SendAsync(query, CancellationToken.None);

        return result;
    }

    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_Find_Color_Matches_From_Their_Own_Inventory()
    {
        // Act
        var response = await ExecuteQuery("rodrigo.basniak", "#FF0000", 5);

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBeLessThanOrEqualTo(5);

        // The closest match should be Mephiston Red (exact match)
        var closestMatch = response.Data.First();
        closestMatch.Name.ShouldBe("Mephiston Red");
        closestMatch.HexColor.ShouldBe("#FF0000");
        closestMatch.Distance.ShouldBe(0.0);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Color_Matches_Are_Ordered_By_Distance()
    {
        // Act
        var response = await ExecuteQuery("rodrigo.basniak", "#FF0000", 10);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(6);

        var matches = response.Data.ToList();

        // Verify ordering by distance (ascending)
        for (int i = 0; i < matches.Count - 1; i++)
        {
            matches[i].Distance.ShouldBeLessThanOrEqualTo(matches[i + 1].Distance);
        }

        // The first match should be the exact match (Mephiston Red)
        matches[0].Name.ShouldBe("Mephiston Red");
        matches[0].Distance.ShouldBe(0.0);

        // The second match should be Pure Red (orange-ish red)
        matches[1].Name.ShouldBe("Pure Red");

        // The third match should be Khorne Red (darker red)
        matches[2].Name.ShouldBe("Khorne Red");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task MaxResults_Limits_The_Number_Of_Results()
    {
        // Act
        var response = await ExecuteQuery("rodrigo.basniak", "#FF0000", 3);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Only_Sees_Their_Own_Inventory()
    {
        // Act
        var response = await ExecuteQuery("ricardo.smarzaro", "#FF0000", 10);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // Verify none of Rodrigo's paints are returned
        var paintNames = response.Data.Select(x => x.Name).ToList();
        paintNames.ShouldNotContain("Mephiston Red");
        paintNames.ShouldNotContain("Khorne Red");
        paintNames.ShouldNotContain("Pure Red");
        paintNames.ShouldNotContain("Yellow");
        paintNames.ShouldNotContain("Caliban Green");
        paintNames.ShouldNotContain("Blue Green");

        // Verify Ricardo's paints are returned
        paintNames.ShouldContain("Purple");
        paintNames.ShouldContain("Cyan");
        paintNames.ShouldContain("Magenta");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Returns_Complete_Paint_Information()
    {
        // Act
        var response = await ExecuteQuery("rodrigo.basniak", "#FF0000", 1);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);

        var match = response.Data.First();
        match.PaintColorId.ShouldNotBe(Guid.Empty);
        match.Name.ShouldBe("Mephiston Red");
        match.HexColor.ShouldBe("#FF0000");
        match.BrandName.ShouldBe("Citadel");
        match.LineName.ShouldBe("Base");
        match.Distance.ShouldBe(0.0);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Invalid_Hex_Color_Returns_Validation_Error()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "INVALID",
            MaxResults = 5
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Empty_Reference_Color_Returns_Validation_Error()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "",
            MaxResults = 5
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 9)]
    public async Task MaxResults_Must_Be_Greater_Than_Zero()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "#FF0000",
            MaxResults = 0
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 10)]
    public async Task MaxResults_Must_Not_Exceed_100()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "#FF0000",
            MaxResults = 101
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 11)]
    public async Task Short_Hex_Color_Format_Returns_Validation_Error()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "#FF00",
            MaxResults = 5
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 12)]
    public async Task Hex_Color_Without_Hash_Returns_Validation_Error()
    {
        // Arrange
        var validator = new FindColorMatches.Validator();
        var query = new FindColorMatchesQuery
        {
            ReferenceColor = "FF0000",
            MaxResults = 5
        };

        // Act
        var validationResult = await validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 13)]
    public async Task Returns_Empty_List_When_User_Has_No_Paints()
    {
        // Act
        var response = await ExecuteQuery("new.user", "#FF0000", 5);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(0);
    }

    [Test, NotInParallel(Order = 14)]
    public async Task Finds_Closest_Matches_For_Different_Color()
    {
        // Act - Search for a blue-green color similar to Blue Green (#0080FF)
        var response = await ExecuteQuery("rodrigo.basniak", "#0080F0", 3);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // The closest match should be Blue Green
        var closestMatch = response.Data.First();
        closestMatch.Name.ShouldBe("Blue Green");
        closestMatch.HexColor.ShouldBe("#0080FF");
    }

    [Test, NotInParallel(Order = 15)]
    public async Task Lowercase_Hex_Color_Is_Accepted()
    {
        // Act - Use lowercase hex color
        var response = await ExecuteQuery("rodrigo.basniak", "#ff0000", 5);

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBeGreaterThan(0);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }

    // Helper class for test identity - duck typing to match IIdentity interface
    private class TestIdentity
    {
        public TestIdentity(string tenantId)
        {
            Tenant = tenantId;
            Id = tenantId;
        }

        public string? Tenant { get; }
        public string? Id { get; }
    }
}
