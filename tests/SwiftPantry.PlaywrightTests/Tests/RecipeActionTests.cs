using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section B — Suite 4: Recipe Actions (save, unsave, add to shopping list)
/// Covers: save recipe, toggle save/unsave, add missing ingredients to shopping list.
/// </summary>
[TestFixture]
public class RecipeActionTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private RecipeDetailPage   _recipeDetailPage   = null!;
    private ShoppingListPage   _shoppingListPage   = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _recipeDetailPage = new RecipeDetailPage(Page, PlaywrightFixture.BaseUrl);
        _shoppingListPage = new ShoppingListPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task SaveRecipe_ShowsUnsaveButton()
    {
        // TODO: GotoAsync(1), SaveRecipeAsync(), assert IsUnsaveButtonVisibleAsync()
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 4");
    }

    [Test]
    public async Task UnsaveRecipe_ShowsSaveButton()
    {
        // TODO: Save then unsave, assert IsSaveButtonVisibleAsync()
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 4");
    }

    [Test]
    public async Task AddToShoppingList_AddsOnlyMissingIngredients()
    {
        // TODO: AddToShoppingListAsync() for a recipe with some owned ingredients,
        // navigate to /ShoppingList, assert only missing items appear
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 4");
    }

    [Test]
    public async Task AddToShoppingList_NoDuplicates_WhenAddedTwice()
    {
        // TODO: Add same recipe to shopping list twice, assert item count unchanged
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 4");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
