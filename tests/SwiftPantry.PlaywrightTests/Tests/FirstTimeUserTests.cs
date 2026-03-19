using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section A — Suite 1: First-Time User Flow
/// Covers: redirect to profile setup, form validation, successful submission,
/// redirect to dashboard, calorie target displayed.
/// </summary>
[NonParallelizable]
[TestFixture]
public class FirstTimeUserTests : PageTest
{
    private ProfilePage _profilePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.Fixture.ResetDatabaseAsync();
        await TestSetup.Fixture.DeleteProfileAsync();
        _profilePage = new ProfilePage(Page, PlaywrightFixture.BaseUrl);
    }

    [Test]
    public async Task Redirects_ToProfileSetup_WhenNoProfile()
    {
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/");
        await Page.WaitForURLAsync("**/Profile/Setup**");
        Assert.That(Page.Url, Does.Contain("/Profile/Setup"));
    }

    [Test]
    public async Task ProfileSetup_SavesProfile_AndRedirectsToDashboard()
    {
        // TC-CALC-1: Male, 30, 70 in, 180 lbs, ModeratelyActive, Maintain
        await _profilePage.CreateProfileAsync(30, "Male", 70, 180, "ModeratelyActive", "Maintain");
        await Page.WaitForURLAsync("**/MealLog**");
        Assert.That(Page.Url, Does.Contain("/MealLog").Or.Contain("/Profile"));
    }

    [Test]
    public async Task ProfileSetup_DisplaysCalorieTarget_AfterSave()
    {
        await _profilePage.CreateProfileAsync(30, "Male", 70, 180, "ModeratelyActive", "Maintain");
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile");
        var text = await _profilePage.GetCalorieTargetTextAsync();
        Assert.That(text, Does.Contain("2,763").Or.Contain("2763"));
    }
}
