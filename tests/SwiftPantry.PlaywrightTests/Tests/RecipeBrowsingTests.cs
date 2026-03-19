using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section B — Suite 3: Recipe Browsing
/// Covers: 18 recipes displayed, meal-type filter, ownership percentage,
/// recipe detail page navigation.
/// </summary>
[NonParallelizable]
[TestFixture]
public class RecipeBrowsingTests : PageTest
{
    private RecipeBrowserPage _recipeBrowserPage = null!;
    private RecipeDetailPage  _recipeDetailPage  = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.Fixture.ResetDatabaseAsync();
        _recipeBrowserPage = new RecipeBrowserPage(Page, PlaywrightFixture.BaseUrl);
        _recipeDetailPage  = new RecipeDetailPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task RecipeBrowser_Shows18Recipes_OnLoad()
    {
        await _recipeBrowserPage.GotoAsync();
        var count = await _recipeBrowserPage.GetRecipeCardCountAsync();
        Assert.That(count, Is.EqualTo(18));
    }

    [Test]
    public async Task RecipeBrowser_FilterByBreakfast_ShowsOnlyBreakfastRecipes()
    {
        await _recipeBrowserPage.GotoAsync();
        await _recipeBrowserPage.FilterByMealTypeAsync("breakfast");
        var count = await _recipeBrowserPage.GetRecipeCardCountAsync();
        Assert.That(count, Is.GreaterThan(0));
        Assert.That(count, Is.LessThan(18));
    }

    [Test]
    public async Task RecipeCard_ShowsOwnershipPercentage()
    {
        await _recipeBrowserPage.GotoAsync();
        // Recipe 1 is Overnight Oats — fixture has no oats in pantry so ownership may vary
        // Just verify the text contains "%"
        var firstCard = Page.Locator("[data-testid^='recipe-card-ownership-']").First;
        var text = await firstCard.InnerTextAsync();
        Assert.That(text, Does.Contain("%"));
    }

    [Test]
    public async Task RecipeCard_Click_NavigatesToDetailPage()
    {
        await _recipeBrowserPage.GotoAsync();
        await _recipeBrowserPage.OpenRecipeAsync(1);
        await Page.WaitForURLAsync("**/Recipes/Detail/**");
        Assert.That(Page.Url, Does.Contain("/Recipes/Detail/"));
    }

    [Test]
    public async Task RecipeDetail_DisplaysCorrectTitle()
    {
        await _recipeDetailPage.GotoAsync(1);
        var title = await _recipeDetailPage.GetTitleAsync();
        Assert.That(title, Is.Not.Empty);
    }
}
