namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /ShoppingList.
/// data-testid contract: shopping-item-{id}, shopping-item-check-{id},
/// shopping-item-delete-{id}, move-to-pantry-{id}, shopping-empty-state,
/// clear-checked-button.
/// See ARCHITECTURE.md for complete data-testid contract.
/// </summary>
public class ShoppingListPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/ShoppingList";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Checks/unchecks the shopping list item with the given id.</summary>
    public async Task ToggleItemAsync(int id)
    {
        // TODO: Implement using data-testid="shopping-item-check-{id}"
        throw new NotImplementedException("TODO: Implement ToggleItemAsync");
    }

    /// <summary>Clicks the move-to-pantry button for the given item id.</summary>
    public async Task MoveToPantryAsync(int id)
    {
        // TODO: Implement using data-testid="move-to-pantry-{id}"
        throw new NotImplementedException("TODO: Implement MoveToPantryAsync");
    }

    /// <summary>Clicks the delete button for the given shopping list item id.</summary>
    public async Task DeleteItemAsync(int id)
    {
        // TODO: Implement using data-testid="shopping-item-delete-{id}"
        throw new NotImplementedException("TODO: Implement DeleteItemAsync");
    }

    /// <summary>Clicks the clear-checked button to remove all checked items.</summary>
    public async Task ClearCheckedAsync()
    {
        // TODO: Implement using data-testid="clear-checked-button"
        throw new NotImplementedException("TODO: Implement ClearCheckedAsync");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if the empty-state placeholder is visible.</summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
        => await page.Locator("[data-testid='shopping-empty-state']").IsVisibleAsync();

    /// <summary>Returns the count of visible shopping list item rows.</summary>
    public async Task<int> GetItemCountAsync()
    {
        // TODO: Implement by counting shopping-item-* locators
        throw new NotImplementedException("TODO: Implement GetItemCountAsync");
    }
}
