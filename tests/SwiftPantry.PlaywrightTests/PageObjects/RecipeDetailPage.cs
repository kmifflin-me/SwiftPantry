namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Recipes/Detail?id={id}.
/// data-testid contract: recipe-title, recipe-meal-types, recipe-servings,
/// recipe-calories, recipe-macros, recipe-ingredients-list,
/// recipe-instructions-list, ownership-badge,
/// save-recipe-button, unsave-recipe-button,
/// add-to-shopping-list-button, log-meal-button,
/// log-servings-input, log-meal-type-select, log-meal-submit-button.
/// See ARCHITECTURE.md for complete data-testid contract.
/// </summary>
public class RecipeDetailPage(IPage page, string baseUrl)
{
    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync(int recipeId)
        => await page.GotoAsync($"{baseUrl}/Recipes/Detail?id={recipeId}");

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Clicks the save-recipe-button to save this recipe.</summary>
    public async Task SaveRecipeAsync()
        => await page.ClickAsync("[data-testid='save-recipe-button']");

    /// <summary>Clicks the unsave-recipe-button to remove this recipe from saved.</summary>
    public async Task UnsaveRecipeAsync()
        => await page.ClickAsync("[data-testid='unsave-recipe-button']");

    /// <summary>Clicks the add-to-shopping-list-button to add missing ingredients.</summary>
    public async Task AddToShoppingListAsync()
        => await page.ClickAsync("[data-testid='add-to-shopping-list-button']");

    /// <summary>Fills the log-meal form and submits it.</summary>
    public async Task LogMealAsync(decimal servings, string mealType)
    {
        // TODO: Implement using data-testid="log-servings-input", "log-meal-type-select", "log-meal-submit-button"
        throw new NotImplementedException("TODO: Implement LogMealAsync");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns the recipe title text.</summary>
    public async Task<string> GetTitleAsync()
        => await page.Locator("[data-testid='recipe-title']").InnerTextAsync();

    /// <summary>Returns the ownership badge text (e.g. "80% owned").</summary>
    public async Task<string> GetOwnershipBadgeTextAsync()
        => await page.Locator("[data-testid='ownership-badge']").InnerTextAsync();

    /// <summary>Returns true if the save button is visible (recipe not yet saved).</summary>
    public async Task<bool> IsSaveButtonVisibleAsync()
        => await page.Locator("[data-testid='save-recipe-button']").IsVisibleAsync();

    /// <summary>Returns true if the unsave button is visible (recipe already saved).</summary>
    public async Task<bool> IsUnsaveButtonVisibleAsync()
        => await page.Locator("[data-testid='unsave-recipe-button']").IsVisibleAsync();
}
