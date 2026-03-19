using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section D — Suite 9: Navigation and Empty State
/// Covers: nav links work, empty states shown when collections are empty,
/// 404 handling for unknown recipe id.
/// </summary>
[NonParallelizable]
[TestFixture]
public class NavigationAndEmptyStateTests : PageTest
{
    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.Fixture.ResetDatabaseAsync();
    }

    [Test]
    public async Task NavLink_Pantry_NavigatesToPantryPage()
    {
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/MealLog");
        await Page.ClickAsync("[data-testid='nav-pantry']");
        await Page.WaitForURLAsync("**/Pantry**");
        Assert.That(Page.Url, Does.Contain("/Pantry"));
    }

    [Test]
    public async Task NavLink_Recipes_NavigatesToRecipeBrowser()
    {
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/MealLog");
        await Page.ClickAsync("[data-testid='nav-recipes']");
        await Page.WaitForURLAsync("**/Recipes**");
        Assert.That(Page.Url, Does.Contain("/Recipes"));
    }

    [Test]
    public async Task NavLink_ShoppingList_NavigatesToShoppingList()
    {
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/MealLog");
        await Page.ClickAsync("[data-testid='nav-shoppinglist']");
        await Page.WaitForURLAsync("**/ShoppingList**");
        Assert.That(Page.Url, Does.Contain("/ShoppingList"));
    }

    [Test]
    public async Task RecipeDetail_Returns404_ForUnknownId()
    {
        var response = await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Recipes/Detail/9999");
        Assert.That(response?.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task SavedRecipes_ShowsEmptyState_Initially()
    {
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/SavedRecipes");
        var emptyState = Page.Locator("[data-testid='saved-recipes-empty-state']");
        Assert.That(await emptyState.IsVisibleAsync(), Is.True);
    }
}
