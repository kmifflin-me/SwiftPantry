// DECISION: Option B — new Generated page rather than modifying the Detail page.
// Reason: the Detail page's OnGetAsync/OnPostSaveAsync/OnPostLogAsync all use an integer
// route ID to look up the recipe from the DB. Adapting it for an ephemeral in-memory recipe
// would require threading TempData checks through every handler. A dedicated page has zero
// impact on existing Detail page behaviour and keeps concerns cleanly separated.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Web.Pages.Recipes;

public class GeneratedModel : PageModel
{
    private readonly IPantryService       _pantryService;
    private readonly IShoppingListService _shoppingListService;
    private readonly IMealLogService      _mealLogService;
    private readonly IRecipeService       _recipeService;
    private readonly AppDbContext         _db;

    public GeneratedModel(
        IPantryService       pantryService,
        IShoppingListService shoppingListService,
        IMealLogService      mealLogService,
        IRecipeService       recipeService,
        AppDbContext         db)
    {
        _pantryService       = pantryService;
        _shoppingListService = shoppingListService;
        _mealLogService      = mealLogService;
        _recipeService       = recipeService;
        _db                  = db;
    }

    public Recipe      Recipe          { get; set; } = null!;
    public string      RecipeJson      { get; set; } = "";
    public List<string> PantryNamesLower { get; set; } = new();
    public int          OwnershipPct    { get; set; }
    public string       DefaultMealType { get; set; } = "Breakfast";

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public async Task<IActionResult> OnGetAsync()
    {
        var recipe = LoadRecipeFromTempData();
        if (recipe is null)
        {
            TempData["GenerationError"] = "No generated recipe found. Please try again.";
            return RedirectToPage("/Recipes/Index");
        }

        return await PopulateAndReturnPage(recipe);
    }

    public async Task<IActionResult> OnPostSaveAsync(string recipeJson)
    {
        var recipe = ParseRecipeJson(recipeJson);
        if (recipe is null) return RedirectToPage("/Recipes/Index");

        // Persist the LLM recipe to the Recipes table so it can be saved/viewed by ID
        recipe.Id = 0; // Let EF assign a new ID (clears the -1 temporary value)
        foreach (var ing in recipe.Ingredients)
            ing.Id = 0; // Clear temp IDs so EF inserts new rows

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        // Now save it to the SavedRecipes table
        await _recipeService.SaveRecipeAsync(recipe.Id);

        TempData["Success"] = "Recipe saved to your library.";
        return RedirectToPage("/Recipes/Detail", new { id = recipe.Id });
    }

    public async Task<IActionResult> OnPostLogAsync(string recipeJson, decimal servings, string mealType)
    {
        var recipe = ParseRecipeJson(recipeJson);
        if (recipe is null) return RedirectToPage("/Recipes/Index");

        // For logging we don't need to persist the recipe — just copy the nutrition data
        var entry = new MealLogEntry
        {
            RecipeName         = recipe.Name,
            MealType           = mealType,
            Servings           = servings > 0 ? servings : 1,
            CaloriesPerServing = recipe.CaloriesPerServing,
            ProteinPerServing  = recipe.ProteinPerServing,
            CarbsPerServing    = recipe.CarbsPerServing,
            FatPerServing      = recipe.FatPerServing,
            LoggedAt           = DateTime.UtcNow
            // RecipeId left null — the recipe isn't in the DB yet (user chose Log without Save)
        };

        await _mealLogService.AddEntryAsync(entry);
        TempData["Success"] = $"Logged \"{recipe.Name}\" to meal log.";

        // Redirect back to the generated page so the user can still save/add-missing
        TempData["GeneratedRecipeJson"] = recipeJson;
        return RedirectToPage("/Recipes/Generated");
    }

    public async Task<IActionResult> OnPostAddMissingAsync(string recipeJson, decimal servings)
    {
        var recipe = ParseRecipeJson(recipeJson);
        if (recipe is null) return RedirectToPage("/Recipes/Index");

        var pantryNames = await _pantryService.GetAllNamesLowercaseAsync();
        var count = await _shoppingListService.AddMissingIngredientsAsync(
            recipe, pantryNames, servings > 0 ? servings : 1);

        TempData["Success"] = count == 0
            ? "You already have all the ingredients!"
            : $"{count} missing ingredient{(count == 1 ? "" : "s")} added to your shopping list.";

        // Redirect back so user can still save/log
        TempData["GeneratedRecipeJson"] = recipeJson;
        return RedirectToPage("/Recipes/Generated");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<IActionResult> PopulateAndReturnPage(Recipe recipe)
    {
        Recipe          = recipe;
        RecipeJson      = JsonSerializer.Serialize(recipe, _jsonOpts);
        PantryNamesLower = await _pantryService.GetAllNamesLowercaseAsync();
        OwnershipPct    = _recipeService.CalculateOwnershipPct(recipe, PantryNamesLower);
        DefaultMealType = CapitalizeFirst(_recipeService.GetDefaultMealType(TimeOnly.FromDateTime(DateTime.Now)));
        return Page();
    }

    private Recipe? LoadRecipeFromTempData()
    {
        var json = TempData["GeneratedRecipeJson"] as string;
        return ParseRecipeJson(json);
    }

    private Recipe? ParseRecipeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<Recipe>(json, _jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private static string CapitalizeFirst(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();
}
