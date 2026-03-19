using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section D — Suite 8: Data Persistence
/// Covers: data survives page reload, profile edits persist across navigation,
/// meal log entries persist across sessions (simulated by full page reload).
/// </summary>
[TestFixture]
public class DataPersistenceTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private DashboardPage _dashboardPage = null!;
    private PantryPage    _pantryPage    = null!;
    private ProfilePage   _profilePage   = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _dashboardPage = new DashboardPage(Page, PlaywrightFixture.BaseUrl);
        _pantryPage    = new PantryPage(Page, PlaywrightFixture.BaseUrl);
        _profilePage   = new ProfilePage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task PantryItem_PersistedAfterReload()
    {
        await _pantryPage.GotoAsync();
        await _pantryPage.AddItemAsync("quinoa", "500g", "Grains");
        await Page.WaitForURLAsync("**/Pantry**");

        // Reload
        await _pantryPage.GotoAsync();
        Assert.That(await _pantryPage.HasItemAsync("quinoa"), Is.True);
    }

    [Test]
    public async Task MealLogEntry_PersistedAfterReload()
    {
        // Fixture has Overnight Oats in today's meal log
        await _dashboardPage.GotoAsync();
        var listText = await Page.Locator("[data-testid='todays-meals-list']").InnerTextAsync();
        Assert.That(listText, Does.Contain("Overnight Oats"));

        // Reload
        await _dashboardPage.GotoAsync();
        listText = await Page.Locator("[data-testid='todays-meals-list']").InnerTextAsync();
        Assert.That(listText, Does.Contain("Overnight Oats"));
    }

    [Test]
    public async Task ProfileEdit_PersistedAfterNavigation()
    {
        // Edit profile to change goal to LoseWeight
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile/Edit");
        await Page.SelectOptionAsync("[data-testid='goal-select']", "LoseWeight");
        await Page.ClickAsync("[data-testid='save-profile-button']");
        await Page.WaitForURLAsync("**/Profile**");

        // Navigate away and come back
        await _dashboardPage.GotoAsync();
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile");

        var targetText = await _profilePage.GetCalorieTargetTextAsync();
        // LoseWeight = 2763 - 500 = 2263
        Assert.That(targetText, Does.Contain("2,263").Or.Contain("2263"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
