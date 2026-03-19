using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Web.Pages.Recipes;

public class DetailModel : PageModel
{
    private readonly IRecipeService       _recipeService;
    private readonly IPantryService       _pantryService;
    private readonly IShoppingListService _shoppingListService;
    private readonly IMealLogService      _mealLogService;

    public DetailModel(
        IRecipeService       recipeService,
        IPantryService       pantryService,
        IShoppingListService shoppingListService,
        IMealLogService      mealLogService)
    {
        _recipeService       = recipeService;
        _pantryService       = pantryService;
        _shoppingListService = shoppingListService;
        _mealLogService      = mealLogService;
    }

    public Recipe      Recipe      { get; set; } = null!;
    public SavedRecipe? SavedRecipe { get; set; }
    public List<string> PantryNamesLower { get; set; } = new();
    public int          OwnershipPct     { get; set; }
    public string       DefaultMealType  { get; set; } = "Breakfast";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var recipe = await _recipeService.GetByIdAsync(id);
        if (recipe is null) return NotFound();

        Recipe          = recipe;
        PantryNamesLower = await _pantryService.GetAllNamesLowercaseAsync();
        OwnershipPct    = _recipeService.CalculateOwnershipPct(Recipe, PantryNamesLower);
        SavedRecipe     = await _recipeService.GetSavedRecipeAsync(id);
        DefaultMealType = CapitalizeFirst(_recipeService.GetDefaultMealType(TimeOnly.FromDateTime(DateTime.Now)));

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id)
    {
        await _recipeService.SaveRecipeAsync(id);
        TempData["Success"] = "Recipe saved.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUnsaveAsync(int savedRecipeId, int recipeId)
    {
        await _recipeService.DeleteSavedRecipeAsync(savedRecipeId);
        TempData["Success"] = "Recipe removed from saved.";
        return RedirectToPage(new { id = recipeId });
    }

    public async Task<IActionResult> OnPostAddMissingAsync(int id, decimal servings)
    {
        var recipe = await _recipeService.GetByIdAsync(id);
        if (recipe is null) return NotFound();

        var pantryNames = await _pantryService.GetAllNamesLowercaseAsync();
        var count = await _shoppingListService.AddMissingIngredientsAsync(recipe, pantryNames, servings);

        TempData["Success"] = count == 0
            ? "You already have all the ingredients!"
            : $"{count} missing ingredient{(count == 1 ? "" : "s")} added to your shopping list.";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostLogAsync(int id, decimal servings, string mealType)
    {
        var recipe = await _recipeService.GetByIdAsync(id);
        if (recipe is null) return NotFound();

        var entry = new MealLogEntry
        {
            RecipeId           = id,
            RecipeName         = recipe.Name,
            MealType           = mealType,
            Servings           = servings,
            CaloriesPerServing = recipe.CaloriesPerServing,
            ProteinPerServing  = recipe.ProteinPerServing,
            CarbsPerServing    = recipe.CarbsPerServing,
            FatPerServing      = recipe.FatPerServing,
            LoggedAt           = DateTime.UtcNow
        };

        await _mealLogService.AddEntryAsync(entry);
        TempData["Success"] = $"Logged \"{recipe.Name}\" to meal log.";
        return RedirectToPage(new { id });
    }

    private static string CapitalizeFirst(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();
}
