using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section A — Suite 1: First-Time User Flow
/// Covers: redirect to profile setup, form validation, successful submission,
/// redirect to dashboard, calorie target displayed.
/// </summary>
[TestFixture]
public class FirstTimeUserTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private ProfilePage _profilePage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient(); // starts the server

    [SetUp]
    public async Task SetUp()
    {
        // Clear profile so this tests the first-time flow
        await Fixture.ResetDatabaseAsync();
        // TODO: delete the UserProfile row so middleware triggers redirect
        _profilePage = new ProfilePage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task Redirects_ToProfileSetup_WhenNoProfile()
    {
        // TODO: Navigate to / and assert URL contains /Profile/Setup
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 1");
    }

    [Test]
    public async Task ProfileSetup_SavesProfile_AndRedirectsToDashboard()
    {
        // TODO: CreateProfileAsync, assert redirect to /MealLog
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 1");
    }

    [Test]
    public async Task ProfileSetup_DisplaysCalorieTarget_AfterSave()
    {
        // TODO: Create profile, navigate to /Profile, assert calorie-target text
        Assert.Inconclusive("TODO: Implement per TEST_PLAN.md Suite 1");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
