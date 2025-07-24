namespace PaintingProjectsManagement.Features.Paints.Brands.Tests;

public class List_PaintBrands_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _brand1Id;
    private static Guid _brand2Id;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands for the tests
        var testBrand1 = new PaintBrand("Brand A");
        var testBrand2 = new PaintBrand("Brand B");
        
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand1);
            await context.AddAsync(testBrand2);
            await context.SaveChangesAsync();
            _brand1Id = testBrand1.Id;
            _brand2Id = testBrand2.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_PaintBrands()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Can_List_PaintBrands()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        var brands = response.Data;
        brands.ShouldNotBeNull();
        brands.Count.ShouldBeGreaterThanOrEqualTo(2);

        // Verify the brands are returned in alphabetical order
        brands.ShouldBeEquivalentTo(brands.OrderBy(x => x.Name).ToList());

        // Verify our test brands are included
        var brandA = brands.FirstOrDefault(x => x.Name == "Brand A");
        brandA.ShouldNotBeNull();
        brandA.Id.ShouldBe(_brand1Id);

        var brandB = brands.FirstOrDefault(x => x.Name == "Brand B");
        brandB.ShouldNotBeNull();
        brandB.Id.ShouldBe(_brand2Id);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Can_List_PaintBrands()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        var brands = response.Data;
        brands.ShouldNotBeNull();
        brands.Count.ShouldBeGreaterThanOrEqualTo(2);

        // Verify the brands are returned in alphabetical order
        brands.ShouldBeEquivalentTo(brands.OrderBy(x => x.Name).ToList());

        // Verify our test brands are included
        var brandA = brands.FirstOrDefault(x => x.Name == "Brand A");
        brandA.ShouldNotBeNull();
        brandA.Id.ShouldBe(_brand1Id);

        var brandB = brands.FirstOrDefault(x => x.Name == "Brand B");
        brandB.ShouldNotBeNull();
        brandB.Id.ShouldBe(_brand2Id);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Different_Users_Get_Same_List_Of_PaintBrands()
    {
        // Act
        var response1 = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands", "rodrigo.basniak");
        var response2 = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands", "ricardo.smarzaro");
        var response3 = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("api/paints/brands", "superuser");

        // Assert the responses
        response1.ShouldBeSuccess();
        response2.ShouldBeSuccess();
        response3.ShouldBeSuccess();

        // Verify all users get the same list
        var brands1 = response1.Data;
        var brands2 = response2.Data;
        var brands3 = response3.Data;
        
        brands1.ShouldNotBeNull();
        brands2.ShouldNotBeNull();
        brands3.ShouldNotBeNull();

        brands1.Count.ShouldBe(brands2.Count);
        brands2.Count.ShouldBe(brands3.Count);

        // Verify the same brands are returned
        var orderedBrands1 = brands1.OrderBy(x => x.Id).ToList();
        var orderedBrands2 = brands2.OrderBy(x => x.Id).ToList();
        var orderedBrands3 = brands3.OrderBy(x => x.Id).ToList();

        for (int i = 0; i < orderedBrands1.Count; i++)
        {
            orderedBrands1[i].Id.ShouldBe(orderedBrands2[i].Id);
            orderedBrands2[i].Id.ShouldBe(orderedBrands3[i].Id);
            orderedBrands1[i].Name.ShouldBe(orderedBrands2[i].Name);
            orderedBrands2[i].Name.ShouldBe(orderedBrands3[i].Name);
        }
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 