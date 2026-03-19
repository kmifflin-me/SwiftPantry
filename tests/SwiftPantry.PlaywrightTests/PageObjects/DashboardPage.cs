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
        await page.FillAsync("[data-testid='manual-meal-name']", recipeName);
        await page.SelectOptionAsync("[data-testid='manual-meal-type']", mealType);
        await page.FillAsync("[data-testid='manual-servings']", servings.ToString());
        await page.FillAsync("[data-testid='manual-calories']", calories.ToString());
        await page.FillAsync("[data-testid='manual-protein']", protein.ToString());
        await page.FillAsync("[data-testid='manual-carbs']", carbs.ToString());
        await page.FillAsync("[data-testid='manual-fat']", fat.ToString());
        await page.ClickAsync("[data-testid='manual-submit']");
    }

    /// <summary>Reads the four progress bar labels and returns a MacroSummary.</summary>
    public async Task<MacroSummary> GetDailyMacroSummaryAsync()
    {
        var calText  = await page.Locator("[data-testid='calorie-progress-label']").InnerTextAsync();
        var protText = await page.Locator("[data-testid='protein-progress-label']").InnerTextAsync();
        var carbText = await page.Locator("[data-testid='carbs-progress-label']").InnerTextAsync();
        var fatText  = await page.Locator("[data-testid='fat-progress-label']").InnerTextAsync();

        // Parse "X / Y kcal" or "X / Y g"
        static (int consumed, int target) ParseLabel(string text)
        {
            var parts = text.Split('/');
            var consumed = int.Parse(new string(parts[0].Trim().Where(char.IsDigit).ToArray()));
            var targetStr = new string(parts[1].Trim().TakeWhile(c => char.IsDigit(c) || c == ',').ToArray())
                .Replace(",", "");
            var target = int.Parse(targetStr);
            return (consumed, target);
        }

        var (calC, calT)  = ParseLabel(calText);
        var (protC, protT) = ParseLabel(protText);
        var (carbC, carbT) = ParseLabel(carbText);
        var (fatC, fatT)   = ParseLabel(fatText);

        return new MacroSummary(calC, calT, protC, protT, carbC, carbT, fatC, fatT);
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
