using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section C — Suite 6: Shopping List Management
/// Covers: check/uncheck items, move to pantry, delete item, clear checked, empty state.
/// </summary>
[TestFixture]
public class ShoppingListTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private ShoppingListPage _shoppingListPage = null!;
    private PantryPage       _pantryPage       = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _shoppingListPage = new ShoppingListPage(Page, PlaywrightFixture.BaseUrl);
        _pantryPage       = new PantryPage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task ShoppingList_ShowsEmptyState_Initially()
    {
        // TODO: GotoAsync, assert IsEmptyStateVisibleAsync()
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 6");
    }

    [Test]
    public async Task MoveToPantry_AddsItemToPantry()
    {
        // TODO: Seed a shopping list item, MoveToPantryAsync, navigate to /Pantry,
        // assert HasItemAsync for the moved ingredient
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 6");
    }

    [Test]
    public async Task MoveToPantry_RemovesItemFromShoppingList()
    {
        // TODO: MoveToPantryAsync, assert item no longer in shopping list
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 6");
    }

    [Test]
    public async Task ClearChecked_RemovesOnlyCheckedItems()
    {
        // TODO: Check some items, ClearCheckedAsync, assert unchecked items remain
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 6");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
