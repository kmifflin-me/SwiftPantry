using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section B — Suite 3: Recipe Browsing
/// Covers: 18 recipes displayed, meal-type filter, ingredient filter,
/// ownership percentage, recipe detail page navigation.
/// </summary>
[TestFixture]
public class RecipeBrowsingTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private RecipeBrowserPage _recipeBrowserPage = null!;
    private RecipeDetailPage _recipeDetailPage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _recipeBrowserPage = new RecipeBrowserPage(Page, PlaywrightFixture.BaseUrl);
        _recipeDetailPage  = new RecipeDetailPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task RecipeBrowser_Shows18Recipes_OnLoad()
    {
        // TODO: GotoAsync, assert GetRecipeCardCountAsync() == 18
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [Test]
    public async Task RecipeBrowser_FilterByBreakfast_ShowsOnlyBreakfastRecipes()
    {
        // TODO: FilterByMealTypeAsync("breakfast"), assert card count and all shown are breakfast
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [Test]
    public async Task RecipeBrowser_FilterByIngredient_NarrowsResults()
    {
        // TODO: FilterByIngredientAsync("chicken"), assert reduced card count
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [Test]
    public async Task RecipeCard_ShowsOwnershipPercentage()
    {
        // TODO: GotoAsync, read ownership badge for a recipe with known pantry match
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [Test]
    public async Task RecipeCard_Click_NavigatesToDetailPage()
    {
        // TODO: OpenRecipeAsync(1), assert page URL contains /Recipes/Detail
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [Test]
    public async Task RecipeDetail_DisplaysCorrectTitle()
    {
        // TODO: GotoAsync(1), assert GetTitleAsync() == "Overnight Oats"
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 3");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
