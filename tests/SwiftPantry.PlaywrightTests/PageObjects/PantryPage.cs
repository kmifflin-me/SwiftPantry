namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Pantry.
/// data-testid contract: add-item-name, add-item-quantity,
/// add-item-category, add-item-submit, pantry-item-{id},
/// delete-item-{id}, pantry-empty-state.
/// </summary>
public class PantryPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/Pantry";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Fills and submits the add-pantry-item form.</summary>
    public async Task AddItemAsync(string name, string quantity, string category)
    {
        await page.FillAsync("[data-testid='add-item-name']", name);
        await page.FillAsync("[data-testid='add-item-quantity']", quantity);
        await page.SelectOptionAsync("[data-testid='add-item-category']", category);
        await page.ClickAsync("[data-testid='add-item-submit']");
    }

    /// <summary>Clicks the delete button for the given pantry item id.</summary>
    public async Task DeleteItemAsync(int id)
    {
        page.Dialog += AcceptDialog;
        await page.ClickAsync($"[data-testid='delete-item-{id}']");
        page.Dialog -= AcceptDialog;
    }

    private static async void AcceptDialog(object? _, IDialog dialog)
        => await dialog.AcceptAsync();

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if an item with the given name is visible in the pantry list.</summary>
    public async Task<bool> HasItemAsync(string name)
        => await page.GetByText(name, new PageGetByTextOptions { Exact = false }).IsVisibleAsync();

    /// <summary>Returns true if the empty-state placeholder is visible.</summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
        => await page.Locator("[data-testid='pantry-empty-state']").IsVisibleAsync();

    /// <summary>Returns the count of visible pantry item rows.</summary>
    public async Task<int> GetItemCountAsync()
        => await page.Locator("[data-testid^='pantry-item-']").CountAsync();
}
