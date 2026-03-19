using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section D — Suite 9: Navigation and Empty State
/// Covers: nav links work, empty states shown when collections are empty,
/// 404 handling for unknown recipe id.
/// </summary>
[TestFixture]
public class NavigationAndEmptyStateTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
    }

    [Test]
    public async Task NavLink_Pantry_NavigatesToPantryPage()
    {
        // TODO: Click nav link, assert URL contains /Pantry
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 9");
    }

    [Test]
    public async Task NavLink_Recipes_NavigatesToRecipeBrowser()
    {
        // TODO: Click nav link, assert URL contains /Recipes
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 9");
    }

    [Test]
    public async Task NavLink_ShoppingList_NavigatesToShoppingList()
    {
        // TODO: Click nav link, assert URL contains /ShoppingList
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 9");
    }

    [Test]
    public async Task RecipeDetail_Returns404_ForUnknownId()
    {
        // TODO: Navigate to /Recipes/Detail?id=9999, assert HTTP 404 or error page
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 9");
    }

    [Test]
    public async Task SavedRecipes_ShowsEmptyState_Initially()
    {
        // TODO: Navigate to /SavedRecipes, assert empty-state element visible
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 9");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
