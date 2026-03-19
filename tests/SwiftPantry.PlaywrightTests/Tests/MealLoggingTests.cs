using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section B — Suite 7: Meal Logging
/// Covers: log meal from recipe detail, log meal manually from dashboard,
/// macro progress bar updates, calorie over-target indicator.
/// </summary>
[TestFixture]
public class MealLoggingTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private MealLogPage      _mealLogPage      = null!;
    private RecipeDetailPage _recipeDetailPage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _mealLogPage      = new MealLogPage(Page, PlaywrightFixture.BaseUrl);
        _recipeDetailPage = new RecipeDetailPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task Dashboard_ShowsFixtureMealEntry_OnLoad()
    {
        // Fixture has "Overnight Oats" (350 cal) logged for today
        await _mealLogPage.GotoAsync();
        var list = Page.Locator("[data-testid='todays-meals-list']");
        Assert.That(await list.IsVisibleAsync(), Is.True);
        Assert.That(await list.InnerTextAsync(), Does.Contain("Overnight Oats"));
    }

    [Test]
    public async Task LogMealManually_AppearsOnDashboard()
    {
        await _mealLogPage.GotoAsync();
        await _mealLogPage.LogMealManuallyAsync("Custom Meal", "Snack", 1, 200, 10, 20, 8);
        await Page.WaitForURLAsync("**/MealLog**");

        var list = Page.Locator("[data-testid='todays-meals-list']");
        Assert.That(await list.InnerTextAsync(), Does.Contain("Custom Meal"));
    }

    [Test]
    public async Task LogMealFromRecipeDetail_UpdatesDashboardMacros()
    {
        // Get initial macro summary
        await _mealLogPage.GotoAsync();
        var before = await _mealLogPage.GetDailyMacroSummaryAsync();

        // Log recipe 1 (350 cal)
        await _recipeDetailPage.GotoAsync(1);
        await _recipeDetailPage.LogMealAsync(1, "Lunch");
        await Page.WaitForURLAsync("**/Recipes/Detail/**");

        // Check updated macros
        await _mealLogPage.GotoAsync();
        var after = await _mealLogPage.GetDailyMacroSummaryAsync();

        Assert.That(after.CaloriesConsumed, Is.GreaterThan(before.CaloriesConsumed));
    }

    [Test]
    public async Task CalorieProgressBar_TurnsRed_WhenOverTarget()
    {
        // Log a huge manual meal (3000 cal) to exceed the fixture target (2763)
        await _mealLogPage.GotoAsync();
        await _mealLogPage.LogMealManuallyAsync("Big Feast", "Dinner", 1, 3000, 100, 300, 100);
        await Page.WaitForURLAsync("**/MealLog**");

        Assert.That(await _mealLogPage.IsCaloriesOverTargetAsync(), Is.True);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
