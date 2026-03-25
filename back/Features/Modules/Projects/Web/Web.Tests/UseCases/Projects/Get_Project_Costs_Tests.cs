using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagement.Features.Projects.Tests;

[HumanFriendlyDisplayName]
public class Get_Project_Costs_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _testProjectId;
    private const string Tenant = "rodrigo.basniak";

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        var project = new Project(Tenant, "Cost Test Project", DateTime.UtcNow.AddDays(-7), null);
        project.AddExecutionWindow(ProjectStepDefinition.Painting, DateTime.UtcNow.AddDays(-2), 2.5);
        project.AddExecutionWindow(ProjectStepDefinition.Printing, DateTime.UtcNow.AddDays(-1), 3.0);

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Project>().ExecuteDeleteAsync();
            await context.Set<Material>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            var paintId = Guid.CreateVersion7();
            var material = new Materials.Material(
                Tenant,
                "Paint Bottle",
                MaterialCategory.Paints,
                new Materials.Quantity(10, PackageContentUnit.Gram),
                new Materials.Money(25, "DKK"));

            var paint = new Material
            {
                Id = material.Id,
                Tenant = Tenant.ToUpperInvariant(),
                Name = "Paint Bottle",
                CategoryId = 1,
                CategoryName = "Paints",
                PricePerUnit = new Money(2.5, "DKK"),
                Unit = MaterialUnit.Gram,
                UpdatedUtc = DateTime.UtcNow
            };

            await context.AddAsync(material);
            await context.AddAsync(paint);
            await context.AddAsync(project);
            await context.SaveChangesAsync();

            _testProjectId = project.Id;

            context.Add(new MaterialForProject(project.Id, material.Id, 4, MaterialUnit.Gram));
            await context.SaveChangesAsync();
        }

        await TestingServer.CacheCredentialsAsync(Tenant, "trustno1", Tenant);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Project_Costs()
    {
        var response = await TestingServer.GetAsync<ProjectCostDetails>($"projects/{_testProjectId}/costs");

        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_Get_Project_Costs_On_Demand()
    {
        var response = await TestingServer.GetAsync<ProjectCostDetails>($"projects/{_testProjectId}/costs", Tenant);

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(_testProjectId);
        response.Data.Electricity.TotalCost.Currency.ShouldBe("DKK");
        response.Data.Electricity.TotalCost.Amount.ShouldBe(1.08d, 0.0001d);

        response.Data.Labor.ShouldContainKey(ProjectStepDefinition.Painting.ToString());
        response.Data.Labor[ProjectStepDefinition.Painting.ToString()].SpentHours.ShouldBe(2.5d, 0.0001d);
        response.Data.Labor[ProjectStepDefinition.Painting.ToString()].TotalCost.Amount.ShouldBe(375d, 0.0001d);

        response.Data.Materials.ShouldContainKey("Paints");
        var paintCost = response.Data.Materials["Paints"].Single();
        paintCost.Description.ShouldBe("Paint Bottle");
        paintCost.Quantity.ShouldBe(4d, 0.0001d);
        paintCost.TotalCost.Amount.ShouldBe(10d, 0.0001d);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Can_Request_Project_Costs_In_Another_Currency()
    {
        var response = await TestingServer.GetAsync<ProjectCostDetails>($"projects/{_testProjectId}/costs?currency=usd", Tenant);

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Electricity.TotalCost.Currency.ShouldBe("USD");
        response.Data.Labor[ProjectStepDefinition.Painting.ToString()].TotalCost.Currency.ShouldBe("USD");
        response.Data.Materials["Paints"].Single().TotalCost.Currency.ShouldBe("USD");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Project_Details_Remain_Available_Without_Cost_Recalculation()
    {
        var response = await TestingServer.GetAsync<ProjectDetails>($"projects/{_testProjectId}", Tenant);

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(_testProjectId);
        response.Data.Name.ShouldBe("Cost Test Project");
        response.Data.CostBreakdown.Id.ShouldBe(Guid.Empty);
        response.Data.CostBreakdown.Electricity.TotalCost.Amount.ShouldBe(0d, 0.0001d);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
