using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section C — Suite 5: Pantry Management
/// Covers: add item, delete item, empty state, category display.
/// </summary>
[TestFixture]
public class PantryTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private PantryPage _pantryPage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _pantryPage = new PantryPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task Pantry_ShowsFixtureItems_OnLoad()
    {
        // TODO: GotoAsync, assert GetItemCountAsync() == 5
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 5");
    }

    [Test]
    public async Task AddPantryItem_AppearsInList()
    {
        // TODO: AddItemAsync("broccoli", "1 head", "Produce"), assert HasItemAsync("broccoli")
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 5");
    }

    [Test]
    public async Task DeletePantryItem_RemovesFromList()
    {
        // TODO: DeleteItemAsync on a fixture item id, assert it is gone
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 5");
    }

    [Test]
    public async Task Pantry_ShowsEmptyState_WhenNoItems()
    {
        // TODO: Delete all fixture items, assert IsEmptyStateVisibleAsync()
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 5");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
