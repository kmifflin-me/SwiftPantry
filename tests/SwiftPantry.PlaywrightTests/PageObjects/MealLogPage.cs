namespace SwiftPantry.PlaywrightTests.PageObjects;

/// <summary>
/// Page object alias for /MealLog — delegates to DashboardPage.
/// Kept separate per TEST_PLAN.md naming convention.
/// See ARCHITECTURE.md for complete data-testid contract.
/// </summary>
public class MealLogPage(IPage page, string baseUrl) : DashboardPage(page, baseUrl)
{
    // Additional meal-log specific helpers can be added here.
    // TODO: Implement as needed per TEST_PLAN.md Section B Suite 7.
}
