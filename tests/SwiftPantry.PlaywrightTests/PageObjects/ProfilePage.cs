namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object for /Profile/Setup and /Profile/Edit.
/// data-testid contract: age-input, sex-select, height-input, height-unit-select,
/// weight-input, weight-unit-select, activity-level-select, goal-select, save-profile-button.
/// </summary>
public class ProfilePage(IPage page, string baseUrl)
{
    private readonly string _setupUrl = $"{baseUrl}/Profile/Setup";
    private readonly string _editUrl  = $"{baseUrl}/Profile/Edit";
    private readonly string _viewUrl  = $"{baseUrl}/Profile";

    // ─── Actions ───────────────────────────────────────────────────────────

    /// <summary>
    /// Navigates to /Profile/Setup, fills all fields, and submits the form.
    /// Heights in inches, weight in lbs (matching TC-CALC-1 fixture format).
    /// </summary>
    public async Task CreateProfileAsync(int age, string sex, decimal heightInches,
        decimal weightLbs, string activityLevel, string goal)
    {
        // TODO: Implement using data-testid locators per ARCHITECTURE.md
        await page.GotoAsync(_setupUrl);
        await page.FillAsync("[data-testid='age-input']", age.ToString());
        await page.SelectOptionAsync("[data-testid='sex-select']", sex);
        await page.FillAsync("[data-testid='height-input']", heightInches.ToString());
        await page.SelectOptionAsync("[data-testid='height-unit-select']", "in");
        await page.FillAsync("[data-testid='weight-input']", weightLbs.ToString());
        await page.SelectOptionAsync("[data-testid='weight-unit-select']", "lbs");
        await page.SelectOptionAsync("[data-testid='activity-level-select']", activityLevel);
        await page.SelectOptionAsync("[data-testid='goal-select']", goal);
        await page.ClickAsync("[data-testid='save-profile-button']");
    }

    // ─── Assertions ────────────────────────────────────────────────────────

    /// <summary>Returns true if the current page URL is /Profile/Setup.</summary>
    public bool IsOnSetupPage() => page.Url.Contains("/Profile/Setup");

    /// <summary>Returns the displayed calorie target text from /Profile.</summary>
    public async Task<string> GetCalorieTargetTextAsync()
        => await page.Locator("[data-testid='calorie-target']").InnerTextAsync();
}
