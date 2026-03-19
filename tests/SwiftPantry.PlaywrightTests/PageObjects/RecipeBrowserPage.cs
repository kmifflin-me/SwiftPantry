namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Recipes (recipe browser / filter page).
/// data-testid contract: recipe-card-{id}, recipe-card-ownership-{id},
/// meal-type-filter, max-prep-filter, min-ownership-filter,
/// apply-filter-button, clear-filter-button, no-recipes-message.
/// </summary>
public class RecipeBrowserPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/Recipes";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Clicks a meal type toggle button and submits the filter form.</summary>
    public async Task FilterByMealTypeAsync(string mealType)
    {
        await page.ClickAsync($"[data-testid='meal-type-filter-{mealType.ToLower()}']");
        await page.ClickAsync("[data-testid='apply-filter-button']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Sets max prep time and submits.</summary>
    public async Task FilterByMaxPrepAsync(int minutes)
    {
        await page.FillAsync("[data-testid='max-prep-filter']", minutes.ToString());
        await page.ClickAsync("[data-testid='apply-filter-button']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Sets min ownership % and submits.</summary>
    public async Task FilterByMinOwnershipAsync(int pct)
    {
        await page.FillAsync("[data-testid='min-ownership-filter']", pct.ToString());
        await page.ClickAsync("[data-testid='apply-filter-button']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Clicks a recipe card's View link, navigating to its Detail page.</summary>
    public async Task OpenRecipeAsync(int recipeId)
        => await page.ClickAsync($"[data-testid='view-recipe-button-{recipeId}']");

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns the number of recipe cards currently displayed.</summary>
    public async Task<int> GetRecipeCardCountAsync()
        => await page.Locator("[data-testid^='recipe-card-']").CountAsync();

    /// <summary>Returns the ownership percentage text for the given recipe id.</summary>
    public async Task<string> GetOwnershipTextAsync(int recipeId)
        => await page.Locator($"[data-testid='recipe-card-ownership-{recipeId}']").InnerTextAsync();

    /// <summary>Returns true if the no-recipes message is visible.</summary>
    public async Task<bool> IsNoRecipesMessageVisibleAsync()
        => await page.Locator("[data-testid='no-recipes-message']").IsVisibleAsync();
}
