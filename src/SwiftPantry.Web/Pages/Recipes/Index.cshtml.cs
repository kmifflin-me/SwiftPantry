using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Pages.Recipes;

public class IndexModel : PageModel
{
    private readonly IRecipeService     _recipeService;
    private readonly IPantryService     _pantryService;
    private readonly IProfileService    _profileService;
    private readonly ILlmRecipeService  _llmRecipeService;

    public IndexModel(
        IRecipeService    recipeService,
        IPantryService    pantryService,
        IProfileService   profileService,
        ILlmRecipeService llmRecipeService)
    {
        _recipeService    = recipeService;
        _pantryService    = pantryService;
        _profileService   = profileService;
        _llmRecipeService = llmRecipeService;
    }

    public List<RecipeViewModel> Recipes      { get; set; } = new();
    public List<string>          MealTypes    { get; set; } = new();
    public int?                  MaxPrep      { get; set; }
    public int?                  MinOwn       { get; set; }
    public bool                  IsLlmAvailable => _llmRecipeService.IsAvailable;

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

    public async Task<IActionResult> OnPostGenerateRecipeAsync(
        [FromForm] List<string>? mealTypes,
        [FromForm] int?          maxPrep)
    {
        // Build RecipeGenerationRequest from user profile + pantry + current filters
        var profile      = await _profileService.GetProfileAsync();
        var pantryNames  = await _pantryService.GetAllNamesLowercaseAsync();
        var selectedMeal = mealTypes?.FirstOrDefault();

        var request = new RecipeGenerationRequest
        {
            DailyCalorieTarget = profile?.CalorieTarget  ?? 0,
            ProteinTargetGrams = profile?.ProteinTargetG ?? 0,
            CarbsTargetGrams   = profile?.CarbsTargetG   ?? 0,
            FatTargetGrams     = profile?.FatTargetG      ?? 0,
            PantryIngredients  = pantryNames,
            MealType           = string.IsNullOrWhiteSpace(selectedMeal) ? null : selectedMeal,
            MaxPrepTimeMinutes = maxPrep,
            PantryPreference   = 0.7
        };

        var recipe = await _llmRecipeService.GenerateRecipeAsync(request);

        if (recipe is null)
        {
            TempData["GenerationError"] = "Recipe generation failed. Please try again.";
            return RedirectToPage(new { mealTypes, maxPrep });
        }

        // Serialize the recipe to TempData so the Generated page can display it
        TempData["GeneratedRecipeJson"] = JsonSerializer.Serialize(recipe, new JsonSerializerOptions
        {
            // Include the Ingredients collection
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        return RedirectToPage("/Recipes/Generated");
    }
}
