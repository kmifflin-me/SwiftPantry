namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Recipes (recipe browser / filter page).
/// data-testid contract: recipe-card-{id}, recipe-card-title-{id},
/// recipe-card-ownership-{id}, meal-type-filter, ingredient-filter-input,
/// filter-submit-button, recipe-count-label, no-recipes-message.
/// See ARCHITECTURE.md for complete data-testid contract.
/// </summary>
public class RecipeBrowserPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/Recipes";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Selects a meal type filter and submits the filter form.</summary>
    public async Task FilterByMealTypeAsync(string mealType)
    {
        // TODO: Implement using data-testid="meal-type-filter"
        throw new NotImplementedException("TODO: Implement FilterByMealTypeAsync");
    }

    /// <summary>Enters an ingredient filter term and submits.</summary>
    public async Task FilterByIngredientAsync(string ingredient)
    {
        // TODO: Implement using data-testid="ingredient-filter-input"
        throw new NotImplementedException("TODO: Implement FilterByIngredientAsync");
    }

    /// <summary>Clicks a recipe card by recipe id, navigating to its Detail page.</summary>
    public async Task OpenRecipeAsync(int recipeId)
    {
        // TODO: Implement using data-testid="recipe-card-{recipeId}"
        throw new NotImplementedException("TODO: Implement OpenRecipeAsync");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns the number of recipe cards currently displayed.</summary>
    public async Task<int> GetRecipeCardCountAsync()
    {
        // TODO: Implement by counting recipe-card-* locators
        throw new NotImplementedException("TODO: Implement GetRecipeCardCountAsync");
    }

    /// <summary>Returns the ownership percentage text for the given recipe id.</summary>
    public async Task<string> GetOwnershipTextAsync(int recipeId)
        => await page.Locator($"[data-testid='recipe-card-ownership-{recipeId}']").InnerTextAsync();

    /// <summary>Returns true if the no-recipes message is visible.</summary>
    public async Task<bool> IsNoRecipesMessageVisibleAsync()
        => await page.Locator("[data-testid='no-recipes-message']").IsVisibleAsync();
}
