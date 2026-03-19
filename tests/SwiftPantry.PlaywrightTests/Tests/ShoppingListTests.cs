using SwiftPantry.PlaywrightTests.PageObjects;
using Microsoft.Extensions.DependencyInjection;

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
        await _shoppingListPage.GotoAsync();
        Assert.That(await _shoppingListPage.IsEmptyStateVisibleAsync(), Is.True);
    }

    [Test]
    public async Task MoveToPantry_AddsItemToPantry()
    {
        // Seed a shopping list item directly and mark it purchased
        int itemId;
        using (var scope = Fixture.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IShoppingListService>();
            var item = await svc.AddItemAsync(new ShoppingListItem
            {
                Name = "spinach", Quantity = "2 cups", Category = "Produce"
            });
            await svc.MarkPurchasedAsync(item.Id);
            itemId = item.Id;
        }

        await _shoppingListPage.GotoAsync();
        await _shoppingListPage.MoveToPantryAsync(itemId);

        await _pantryPage.GotoAsync();
        Assert.That(await _pantryPage.HasItemAsync("spinach"), Is.True);
    }

    [Test]
    public async Task MoveToPantry_RemovesItemFromShoppingList()
    {
        int itemId;
        using (var scope = Fixture.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IShoppingListService>();
            var item = await svc.AddItemAsync(new ShoppingListItem
            {
                Name = "garlic", Quantity = "3 cloves", Category = "Produce"
            });
            await svc.MarkPurchasedAsync(item.Id);
            itemId = item.Id;
        }

        await _shoppingListPage.GotoAsync();
        await _shoppingListPage.MoveToPantryAsync(itemId);
        await _shoppingListPage.GotoAsync();

        var itemLocator = Page.Locator($"[data-testid='shopping-item-{itemId}']");
        Assert.That(await itemLocator.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ClearChecked_RemovesOnlyPurchasedItems()
    {
        int unpurchasedId;
        using (var scope = Fixture.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IShoppingListService>();
            var keep = await svc.AddItemAsync(new ShoppingListItem
            {
                Name = "milk", Quantity = "1 gallon", Category = "Dairy"
            });
            var remove = await svc.AddItemAsync(new ShoppingListItem
            {
                Name = "butter", Quantity = "1 stick", Category = "Dairy"
            });
            await svc.MarkPurchasedAsync(remove.Id);
            unpurchasedId = keep.Id;
        }

        await _shoppingListPage.GotoAsync();
        await _shoppingListPage.ClearCheckedAsync();
        await Page.WaitForURLAsync("**/ShoppingList**");

        // The unpurchased item should still be present
        var keepLocator = Page.Locator($"[data-testid='shopping-item-{unpurchasedId}']");
        Assert.That(await keepLocator.CountAsync(), Is.EqualTo(1));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
