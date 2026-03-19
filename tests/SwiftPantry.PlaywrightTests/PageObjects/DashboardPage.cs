namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /MealLog (Dashboard / Daily progress + meal entries).
/// See ARCHITECTURE.md for complete data-testid contract.
/// </summary>
public class DashboardPage(IPage page, string baseUrl)
{
    private readonly string _url = $"{baseUrl}/MealLog";

    // ─── Navigation ────────────────────────────────────────────────────────

    public async Task GotoAsync() => await page.GotoAsync(_url);

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>Fills and submits the manual meal log form.</summary>
    public async Task LogMealManuallyAsync(string recipeName, string mealType,
        decimal servings, int calories, decimal protein, decimal carbs, decimal fat)
    {
        // TODO: Implement using data-testid locators
        throw new NotImplementedException("TODO: Implement LogMealManuallyAsync");
    }

    /// <summary>Reads the four progress bar labels and returns a MacroSummary.</summary>
    public async Task<MacroSummary> GetDailyMacroSummaryAsync()
    {
        // TODO: Implement by reading data-testid="calorie-progress-label" etc.
        throw new NotImplementedException("TODO: Implement GetDailyMacroSummaryAsync");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if the calorie progress bar has CSS class bg-danger.</summary>
    public async Task<bool> IsCaloriesOverTargetAsync()
    {
        var cls = await page.Locator("[data-testid='calorie-progress-bar']").GetAttributeAsync("class");
        return cls?.Contains("bg-danger") ?? false;
    }
}

/// <summary>Consumed vs target macros read from the dashboard progress bar labels.</summary>
public record MacroSummary(
    int CaloriesConsumed, int CaloriesTarget,
    int ProteinConsumed,  int ProteinTarget,
    int CarbsConsumed,    int CarbsTarget,
    int FatConsumed,      int FatTarget);
