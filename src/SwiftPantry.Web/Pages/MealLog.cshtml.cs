using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Pages;

public class MealLogModel : PageModel
{
    private readonly IMealLogService _mealLogService;
    private readonly IProfileService _profileService;
    private readonly IRecipeService  _recipeService;

    public MealLogModel(IMealLogService mealLogService,
                        IProfileService profileService,
                        IRecipeService  recipeService)
    {
        _mealLogService = mealLogService;
        _profileService = profileService;
        _recipeService  = recipeService;
    }

    public UserProfile? Profile { get; set; }
    public DailySummary? Summary { get; set; }
    public List<MealLogEntry> Entries { get; set; } = new();
    public List<SavedRecipe>  SavedRecipes { get; set; } = new();
    public DateOnly SelectedDate { get; set; }
    public DateOnly Today { get; set; }
    public string DefaultMealType { get; set; } = "Breakfast";

    [BindProperty]
    public MealLogEntry NewEntry { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? date)
    {
        Profile = await _profileService.GetProfileAsync();
        if (Profile is null)
            return RedirectToPage("/Profile/Setup");

        Today = DateOnly.FromDateTime(DateTime.Now);
        SelectedDate = date is not null && DateOnly.TryParse(date, out var d) ? d : Today;

        // Constrain to last 7 days
        var minDate = Today.AddDays(-6);
        if (SelectedDate > Today) SelectedDate = Today;
        if (SelectedDate < minDate) SelectedDate = minDate;

        Entries      = await _mealLogService.GetEntriesForDateAsync(SelectedDate);
        Summary      = await _mealLogService.GetDailySummaryAsync(SelectedDate, Profile);
        SavedRecipes = await _recipeService.GetAllSavedRecipesAsync();

        DefaultMealType = CapitalizeFirst(_recipeService.GetDefaultMealType(TimeOnly.FromDateTime(DateTime.Now)));

        return Page();
    }

    public async Task<IActionResult> OnPostLogManualAsync()
    {
        // Remove RecipeId binding from validation — it's set server-side
        ModelState.Remove(nameof(NewEntry) + ".RecipeId");
        ModelState.Remove(nameof(NewEntry) + ".LoggedAt");

        if (!ModelState.IsValid)
        {
            // Reload page data for re-render
            await ReloadPageData();
            return Page();
        }

        NewEntry.LoggedAt = DateTime.UtcNow;
        await _mealLogService.AddEntryAsync(NewEntry);

        TempData["Success"] = $"Logged \"{NewEntry.RecipeName}\" successfully.";
        return RedirectToPage(new { date = DateOnly.FromDateTime(NewEntry.LoggedAt.ToLocalTime()).ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> OnPostLogRecipeAsync(int RecipeId, decimal Servings, string MealType)
    {
        var recipe = await _recipeService.GetByIdAsync(RecipeId);
        if (recipe is null)
            return NotFound();

        var entry = new MealLogEntry
        {
            RecipeId          = RecipeId,
            RecipeName        = recipe.Name,
            MealType          = MealType,
            Servings          = Servings,
            CaloriesPerServing = recipe.CaloriesPerServing,
            ProteinPerServing  = recipe.ProteinPerServing,
            CarbsPerServing    = recipe.CarbsPerServing,
            FatPerServing      = recipe.FatPerServing,
            LoggedAt           = DateTime.UtcNow
        };

        await _mealLogService.AddEntryAsync(entry);
        TempData["Success"] = $"Logged \"{recipe.Name}\" successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _mealLogService.DeleteEntryAsync(id);
        TempData["Success"] = "Meal log entry deleted.";
        // Preserve the date parameter if it was set
        var returnDate = Request.Query["date"].ToString();
        return RedirectToPage(string.IsNullOrEmpty(returnDate) ? null : new { date = returnDate });
    }

    private async Task ReloadPageData()
    {
        Profile = await _profileService.GetProfileAsync();
        Today = DateOnly.FromDateTime(DateTime.Now);
        SelectedDate = Today;
        Entries = await _mealLogService.GetEntriesForDateAsync(SelectedDate);
        if (Profile is not null)
            Summary = await _mealLogService.GetDailySummaryAsync(SelectedDate, Profile);
        SavedRecipes = await _recipeService.GetAllSavedRecipesAsync();
        DefaultMealType = CapitalizeFirst(_recipeService.GetDefaultMealType(TimeOnly.FromDateTime(DateTime.Now)));
    }

    private static string CapitalizeFirst(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();
}
