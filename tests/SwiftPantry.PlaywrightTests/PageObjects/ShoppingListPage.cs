namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /ShoppingList.
/// data-testid contract: shopping-item-{id}, shopping-item-check-{id},
/// shopping-item-delete-{id}, move-to-pantry-{id}, shopping-empty-state,
/// clear-checked-button.
/// </summary>
public class ShoppingListPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/ShoppingList";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Checks the shopping list item with the given id via AJAX.</summary>
    public async Task ToggleItemAsync(int id)
        => await page.ClickAsync($"[data-testid='shopping-item-check-{id}']");

    /// <summary>Clicks the move-to-pantry button for the given item id.</summary>
    public async Task MoveToPantryAsync(int id)
    {
        await page.ClickAsync($"[data-testid='move-to-pantry-{id}']");
        await page.WaitForURLAsync(_url);
    }

    /// <summary>Clicks the delete button for the given shopping list item id.</summary>
    public async Task DeleteItemAsync(int id)
    {
        page.Dialog += AcceptDialog;
        await page.ClickAsync($"[data-testid='shopping-item-delete-{id}']");
        page.Dialog -= AcceptDialog;
    }

    /// <summary>Clicks the clear-checked button to remove all purchased items.</summary>
    public async Task ClearCheckedAsync()
    {
        page.Dialog += AcceptDialog;
        await page.ClickAsync("[data-testid='clear-checked-button']");
        page.Dialog -= AcceptDialog;
    }

    private static async void AcceptDialog(object? _, IDialog dialog)
        => await dialog.AcceptAsync();

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if the empty-state placeholder is visible.</summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
        => await page.Locator("[data-testid='shopping-empty-state']").IsVisibleAsync();

    /// <summary>Returns the count of visible shopping list item rows.</summary>
    public async Task<int> GetItemCountAsync()
        => await page.Locator("[data-testid^='shopping-item-']").CountAsync();
}
