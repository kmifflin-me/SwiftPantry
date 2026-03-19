using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public interface IRecipeService
{
    /// <summary>Returns all recipes including their Ingredients collection.</summary>
    Task<List<Recipe>> GetAllRecipesAsync();

    /// <summary>Returns a single recipe with Ingredients, or null if not found.</summary>
    Task<Recipe?> GetByIdAsync(int id);

    /// <summary>
    /// Filters recipes by the given criteria and calculates ownership % per pantryNamesLower.
    /// MealTypes empty = all types. Results sorted by Name ascending.
    /// </summary>
    Task<List<RecipeViewModel>> FilterRecipesAsync(RecipeFilter filter, List<string> pantryNamesLower);

    /// <summary>
    /// Calculates ingredient ownership % for a recipe against pantryNamesLower (case-insensitive).
    /// Returns floor(matchCount / totalIngredients * 100). Returns 0 if no ingredients.
    /// </summary>
    int CalculateOwnershipPct(Recipe recipe, List<string> pantryNamesLower);

    /// <summary>
    /// Returns the default meal type string based on time of day (server local):
    ///   Before 10:00 → "breakfast"; 10:00–13:59 → "lunch"; 14:00–16:59 → "snack"; 17:00+ → "dinner"
    /// </summary>
    string GetDefaultMealType(TimeOnly time);

    /// <summary>Returns the SavedRecipe record for the given recipeId, or null.</summary>
    Task<SavedRecipe?> GetSavedRecipeAsync(int recipeId);

    /// <summary>Returns all saved recipes with Recipe navigation loaded, sorted by SavedAt DESC.</summary>
    Task<List<SavedRecipe>> GetAllSavedRecipesAsync();

    /// <summary>
    /// Creates a SavedRecipe for the given recipeId. Idempotent — if already saved, returns existing record.
    /// Sets SavedAt = UtcNow on new creation only.
    /// </summary>
    Task<SavedRecipe> SaveRecipeAsync(int recipeId);

    /// <summary>Deletes a SavedRecipe by its own ID (not RecipeId).</summary>
    Task DeleteSavedRecipeAsync(int savedRecipeId);

    /// <summary>
    /// Seeds the Recipes table from Data/seed_recipes.json if the table is empty.
    /// Idempotent — skips entirely if any Recipe rows exist.
    /// </summary>
    Task SeedRecipesIfEmptyAsync();
}
