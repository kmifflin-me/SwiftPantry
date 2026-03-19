using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Web.Pages;

public class SavedRecipesModel : PageModel
{
    private readonly IRecipeService  _recipeService;
    private readonly IPantryService  _pantryService;
    private readonly IMealLogService _mealLogService;

    public SavedRecipesModel(IRecipeService recipeService, IPantryService pantryService, IMealLogService mealLogService)
    {
        _recipeService  = recipeService;
        _pantryService  = pantryService;
        _mealLogService = mealLogService;
    }

    public List<SavedRecipe> SavedRecipes { get; set; } = new();
    public Dictionary<int, int> OwnershipByRecipeId { get; set; } = new();

    public async Task OnGetAsync()
    {
        SavedRecipes = await _recipeService.GetAllSavedRecipesAsync();
        var pantryNames = await _pantryService.GetAllNamesLowercaseAsync();

        foreach (var sr in SavedRecipes)
        {
            OwnershipByRecipeId[sr.RecipeId] =
                _recipeService.CalculateOwnershipPct(sr.Recipe, pantryNames);
        }
    }

    public async Task<IActionResult> OnPostUnsaveAsync(int savedRecipeId)
    {
        await _recipeService.DeleteSavedRecipeAsync(savedRecipeId);
        TempData["Success"] = "Recipe removed from saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLogRecipeAsync(int RecipeId, decimal Servings, string MealType)
    {
        var recipe = await _recipeService.GetByIdAsync(RecipeId);
        if (recipe is null)
            return NotFound();

        var entry = new MealLogEntry
        {
            RecipeId           = RecipeId,
            RecipeName         = recipe.Name,
            MealType           = MealType,
            Servings           = Servings,
            CaloriesPerServing = recipe.CaloriesPerServing,
            ProteinPerServing  = recipe.ProteinPerServing,
            CarbsPerServing    = recipe.CarbsPerServing,
            FatPerServing      = recipe.FatPerServing,
            LoggedAt           = DateTime.UtcNow
        };

        await _mealLogService.AddEntryAsync(entry);
        TempData["Success"] = $"Logged \"{recipe.Name}\" successfully.";
        return RedirectToPage("/MealLog");
    }
}
