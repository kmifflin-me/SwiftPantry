namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Pantry.
/// data-testid contract: add-pantry-item-name, add-pantry-item-quantity,
/// add-pantry-item-category, add-pantry-item-button, pantry-item-{id},
/// pantry-item-delete-{id}, pantry-empty-state.
/// See ARCHITECTURE.md for complete data-testid contract.
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
        // TODO: Implement using data-testid locators per ARCHITECTURE.md
        throw new NotImplementedException("TODO: Implement AddItemAsync");
    }

    /// <summary>Clicks the delete button for the given pantry item id.</summary>
    public async Task DeleteItemAsync(int id)
    {
        // TODO: Implement using data-testid="pantry-item-delete-{id}"
        throw new NotImplementedException("TODO: Implement DeleteItemAsync");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if an item with the given name is visible in the pantry list.</summary>
    public async Task<bool> HasItemAsync(string name)
    {
        // TODO: Implement by scanning pantry-item rows for matching name text
        throw new NotImplementedException("TODO: Implement HasItemAsync");
    }

    /// <summary>Returns true if the empty-state placeholder is visible.</summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
        => await page.Locator("[data-testid='pantry-empty-state']").IsVisibleAsync();

    /// <summary>Returns the count of visible pantry item rows.</summary>
    public async Task<int> GetItemCountAsync()
    {
        // TODO: Implement by counting pantry-item-* locators
        throw new NotImplementedException("TODO: Implement GetItemCountAsync");
    }
}
