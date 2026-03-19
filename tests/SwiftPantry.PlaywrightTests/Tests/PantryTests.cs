using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section C — Suite 5: Pantry Management
/// Covers: add item, delete item, empty state, category display.
/// </summary>
[NonParallelizable]
[TestFixture]
public class PantryTests : PageTest
{
    private PantryPage _pantryPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.Fixture.ResetDatabaseAsync();
        _pantryPage = new PantryPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task Pantry_ShowsFixtureItems_OnLoad()
    {
        await _pantryPage.GotoAsync();
        // Fixture seeds 5 pantry items; each item has view row + edit row,
        // but we count only the view rows via data-testid="pantry-item-{id}"
        var count = await _pantryPage.GetItemCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5));
    }

    [Test]
    public async Task AddPantryItem_AppearsInList()
    {
        await _pantryPage.GotoAsync();
        await _pantryPage.AddItemAsync("broccoli", "1 head", "Produce");
        await Page.WaitForURLAsync("**/Pantry**");
        Assert.That(await _pantryPage.HasItemAsync("broccoli"), Is.True);
    }

    [Test]
    public async Task DeletePantryItem_RemovesFromList()
    {
        await _pantryPage.GotoAsync();

        // Find the first pantry-item id from the DOM
        var firstItem = Page.Locator("[data-testid^='pantry-item-']").First;
        var testId = await firstItem.GetAttributeAsync("data-testid");
        var id = int.Parse(testId!.Replace("pantry-item-", ""));

        await _pantryPage.DeleteItemAsync(id);
        await Page.WaitForURLAsync("**/Pantry**");

        var itemLocator = Page.Locator($"[data-testid='pantry-item-{id}']");
        Assert.That(await itemLocator.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Pantry_ShowsEmptyState_WhenNoItems()
    {
        // Remove all pantry items via DB
        using var scope = TestSetup.Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PantryItems.RemoveRange(db.PantryItems.ToList());
        await db.SaveChangesAsync();

        await _pantryPage.GotoAsync();
        Assert.That(await _pantryPage.IsEmptyStateVisibleAsync(), Is.True);
    }
}
