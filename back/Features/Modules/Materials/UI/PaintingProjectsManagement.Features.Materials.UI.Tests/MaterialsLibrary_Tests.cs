namespace PaintingProjectsManagement.Features.Materials.UI.Tests;

[HumanFriendlyDisplayName]
public class MaterialsLibrary_Tests : PlaywrightTestBase
{
    [Test]
    public async Task User_Can_Navigate_To_Materials_Library_Page()
    {
        // Prepare
        await InitializeAsync();
        await AuthenticateAsync("rodrigo.basniak", "trustno1");

        // Act
        await NavigateToAsync("/materials/library");

        // Assert the response
        // Wait for the page to load and check for the "Materials Library" heading
        var heading = await Page.WaitForSelectorAsync("text=Materials Library", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });

        heading.ShouldNotBeNull();

        // Verify the page URL
        var currentUrl = Page.Url;
        currentUrl.ShouldContain("/materials/library");
    }

    [Test]
    public async Task Materials_Library_Page_Displays_Add_Button()
    {
        // Prepare
        await InitializeAsync();
        await AuthenticateAsync("rodrigo.basniak", "trustno1");

        // Act
        await NavigateToAsync("/materials/library");

        // Assert the response
        // Wait for the Add button to be visible
        var addButton = await Page.WaitForSelectorAsync("button:has-text('Add')", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });

        addButton.ShouldNotBeNull();

        var isVisible = await addButton.IsVisibleAsync();
        isVisible.ShouldBeTrue();
    }
}
