using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Services;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Pages.Recipes;

public class IndexModel : PageModel
{
    private readonly IRecipeService  _recipeService;
    private readonly IPantryService  _pantryService;

    public IndexModel(IRecipeService recipeService, IPantryService pantryService)
    {
        _recipeService = recipeService;
        _pantryService = pantryService;
    }

    public List<RecipeViewModel> Recipes    { get; set; } = new();
    public List<string>          MealTypes  { get; set; } = new();
    public int?                  MaxPrep    { get; set; }
    public int?                  MinOwn     { get; set; }

    public async Task OnGetAsync(
        [FromQuery] List<string>? mealTypes,
        [FromQuery] int?          maxPrep,
        [FromQuery] int?          minOwn)
    {
        MealTypes = mealTypes ?? new List<string>();
        MaxPrep   = maxPrep;
        MinOwn    = minOwn;

        var pantryNames = await _pantryService.GetAllNamesLowercaseAsync();

        var filter = new RecipeFilter
        {
            MealTypes           = MealTypes.Select(m => m.ToLower()).ToList(),
            MaxPrepTimeMinutes  = MaxPrep,
            MinOwnershipPct     = MinOwn
        };

        Recipes = await _recipeService.FilterRecipesAsync(filter, pantryNames);
    }

    public async Task<IActionResult> OnPostSaveAsync(int recipeId)
    {
        await _recipeService.SaveRecipeAsync(recipeId);
        TempData["Success"] = "Recipe saved.";
        return RedirectToPage(new { mealTypes = MealTypes, maxPrep = MaxPrep, minOwn = MinOwn });
    }
}
