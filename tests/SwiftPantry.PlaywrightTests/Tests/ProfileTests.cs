using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section A — Suite 2: Profile Management
/// Covers: TC-CALC-1 through TC-CALC-4 calorie calculations,
/// edit profile flow, unit conversion display.
/// </summary>
[NonParallelizable]
[TestFixture]
public class ProfileTests : PageTest
{
    private ProfilePage _profilePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.Fixture.ResetDatabaseAsync();
        _profilePage = new ProfilePage(Page, PlaywrightFixture.BaseUrl);
    }

    /// <summary>TC-CALC-1: Male, 30, 180 lbs, 5'10", ModeratelyActive, Maintain → 2763 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_MaleMaintain_ReturnsExpected()
    {
        // Fixture already has TC-CALC-1 profile
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile");
        var text = await _profilePage.GetCalorieTargetTextAsync();
        Assert.That(text, Does.Contain("2,763").Or.Contain("2763"));
    }

    /// <summary>TC-CALC-2: Female, 25, 130 lbs, 5'4", LightlyActive, LoseWeight → 1315 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_FemaleLoseWeight_ReturnsExpected()
    {
        // Delete fixture profile, create TC-CALC-2 profile
        await TestSetup.Fixture.DeleteProfileAsync();
        // 162.56 cm ≈ 64 in, 58.97 kg ≈ 130 lbs
        await _profilePage.CreateProfileAsync(25, "Female", 64, 130, "LightlyActive", "LoseWeight");
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile");
        var text = await _profilePage.GetCalorieTargetTextAsync();
        Assert.That(text, Does.Contain("1,315").Or.Contain("1315"));
    }

    /// <summary>TC-CALC-3: Male, 45, 220 lbs, 6'0", VeryActive, GainWeight → 3614 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_MaleGainWeight_ReturnsExpected()
    {
        await TestSetup.Fixture.DeleteProfileAsync();
        // 182.88 cm ≈ 72 in, 99.79 kg ≈ 220 lbs
        await _profilePage.CreateProfileAsync(45, "Male", 72, 220, "VeryActive", "GainWeight");
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile");
        var text = await _profilePage.GetCalorieTargetTextAsync();
        Assert.That(text, Does.Contain("3,614").Or.Contain("3614"));
    }

    [Test]
    public async Task EditProfile_UpdatesCalorieTarget()
    {
        // Start with TC-CALC-1 fixture (Maintain = 2763). Change to LoseWeight.
        await Page.GotoAsync(PlaywrightFixture.BaseUrl + "/Profile/Edit");
        await Page.SelectOptionAsync("[data-testid='goal-select']", "LoseWeight");
        await Page.ClickAsync("[data-testid='save-profile-button']");
        await Page.WaitForURLAsync("**/Profile**");

        var text = await _profilePage.GetCalorieTargetTextAsync();
        // LoseWeight = 2763 - 500 = 2263
        Assert.That(text, Does.Contain("2,263").Or.Contain("2263"));
    }
}
