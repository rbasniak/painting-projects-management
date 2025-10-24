namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Create_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test material for duplicate name validation tests
        var existingMaterial = new Material(
            "rodrigo.basniak",
            "Existing Material",
            new Quantity(1, PackageContentUnit.Each),
            new Money(10.0, "USD")
        );

        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            await context.AddAsync(existingMaterial);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
            savedMaterial.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Material()
    {
        var testStartTime = DateTime.UtcNow;

        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Test Material",
            PackageContentAmount = 1,
            PackageContentUnit = (int)PackageContentUnit.Each,
            PackagePriceAmount = 19,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Test Material").ToList();
        materials.ShouldBeEmpty();

        // Assert the messages
        TestingServer.ShouldNotHaveCreatedDomainEvents(testStartTime);
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments(0)]
    [Arguments(-1)]
    public async Task User_Cannot_Create_Material_When_PackageContentAmount_Is_Not_Positive(double packageAmount)
    {
        var testStartTime = DateTime.UtcNow;

        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Invalid Amount",
            PackageContentAmount = packageAmount,
            PackageContentUnit = (int)PackageContentUnit.Each,
            PackagePriceAmount = 19,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Package amount must be greater than zero.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Invalid Amount").ToList();
        materials.ShouldBeEmpty();

        // Assert the messages
        TestingServer.ShouldNotHaveCreatedDomainEvents(testStartTime);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Material_When_Name_Already_Exists()
    {
        var testStartTime = DateTime.UtcNow;

        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Existing Material", // This name was created in Seed test
            PackageContentAmount = 1,
            PackageContentUnit = (int)PackageContentUnit.Each,
            PackagePriceAmount = 15,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A material with this name already exists.");

        // Assert the database
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Existing Material").ToList();
        materials.Count.ShouldBe(1); // Only the original one from Seed

        // Assert the messages
        TestingServer.ShouldNotHaveCreatedDomainEvents(testStartTime);
    }

    [Test, NotInParallel(Order = 13)]
    public async Task User_Can_Create_Material_With_Same_Name_As_Another_User()
    {
        var testStartTime = DateTime.UtcNow;

        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "Existing Material", // This name was created by rodrigo.basniak in Seed test
            PackageContentAmount = 1,
            PackageContentUnit = (int)PackageContentUnit.Each,
            PackagePriceAmount = 25,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Existing Material");
        result.PackagePrice.Amount.ShouldBe(25);
        result.PackagePrice.CurrencyCode.ShouldBe("USD");
        //result.PackagetContent.Amount.ShouldBe(1);
        //EnumAssertionExtensions.ShouldBeEquivalentTo(result.PackagetContent.Unit, PackageContentUnit.Each);

        // Assert the database - should have two materials with the same name but different users
        var materials = TestingServer.CreateContext().Set<Material>().Where(x => x.Name == "Existing Material").ToList();
        materials.Count.ShouldBe(2); // One from rodrigo.basniak (Seed) and one from ricardo.smarzaro

        var rbMaterial = materials.FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Existing Material");
        var rsMaterial = materials.FirstOrDefault(x => x.TenantId == "RICARDO.SMARZARO" && x.Name == "Existing Material");

        rbMaterial.ShouldNotBeNull();
        rbMaterial.Id.ShouldNotBe(rsMaterial.Id);
        rbMaterial.UnitPriceAmount.ShouldBe(10.0); // From Seed test
        rbMaterial.UnitPriceUnit.ShouldBe(PackageContentUnit.Each); // From Seed test

        rsMaterial.ShouldNotBeNull();
        rsMaterial.UnitPriceAmount.ShouldBe(25);
        rsMaterial.UnitPriceUnit.ShouldBe(PackageContentUnit.Each);

        // Assert the messages
        TestingServer.ShouldHaveCreatedDomainEvents(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialCreated)] = 1,
        }, out var events);
    }

    [Test, NotInParallel(Order = 14)]
    public async Task User_Can_Create_Material()
    {
        var testStartTime = DateTime.UtcNow;

        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "8x4 magnet for test",
            PackageContentAmount = 1,
            PackageContentUnit = (int)PackageContentUnit.Each,
            PackagePriceAmount = 19,
            PackagePriceCurrency = "USD",
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("8x4 magnet for test");
        result.PackagePrice.Amount.ShouldBe(19);

        // Assert the database
        var entity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("8x4 magnet for test");
        entity.UnitPriceAmount.ShouldBe(19);
        entity.UnitPriceUnit.ShouldBe(PackageContentUnit.Each);

        // Assert the messages
        TestingServer.ShouldHaveCreatedDomainEvents(testStartTime, new Dictionary<Type, int>
        {
            [typeof(MaterialCreated)] = 1,
        }, out var events);
    } 
}
 