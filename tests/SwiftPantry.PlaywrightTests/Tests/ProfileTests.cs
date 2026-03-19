using SwiftPantry.PlaywrightTests.PageObjects;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// TEST_PLAN.md Section A — Suite 2: Profile Management
/// Covers: TC-CALC-1 through TC-CALC-4 calorie calculations,
/// edit profile flow, unit conversion display.
/// </summary>
[TestFixture]
public class ProfileTests : PageTest
{
    private static readonly PlaywrightFixture Fixture = new();
    private ProfilePage _profilePage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Fixture.CreateClient();

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
        _profilePage = new ProfilePage(Page, PlaywrightFixture.BaseUrl);
    }

    /// <summary>TC-CALC-1: Male, 30, 180 lbs, 5'10", ModeratelyActive, Maintain → 2763 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_MaleMaintain_ReturnsExpected()
    {
        // Fixture already has TC-CALC-1 profile; just assert the displayed target
        // TODO: Navigate to /Profile, assert calorie-target text == "2763"
        Assert.Inconclusive("TODO: Implement TC-CALC-1");
    }

    /// <summary>TC-CALC-2: Female, 25, 130 lbs, 5'5", LightlyActive, LoseWeight → 1388 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_FemaleLoseWeight_ReturnsExpected()
    {
        // TODO: Create profile with TC-CALC-2 values, assert calorie-target
        Assert.Inconclusive("TODO: Implement TC-CALC-2");
    }

    /// <summary>TC-CALC-3: Male, 22, 160 lbs, 6'0", VeryActive, GainWeight → 3476 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_MaleGainWeight_ReturnsExpected()
    {
        // TODO: Create profile with TC-CALC-3 values, assert calorie-target
        Assert.Inconclusive("TODO: Implement TC-CALC-3");
    }

    /// <summary>TC-CALC-4: Female, 45, 170 lbs, 5'8", Sedentary, Maintain → 1781 kcal.</summary>
    [Test]
    public async Task CalcCalorieTarget_FemaleSedentaryMaintain_ReturnsExpected()
    {
        // TODO: Create profile with TC-CALC-4 values, assert calorie-target
        Assert.Inconclusive("TODO: Implement TC-CALC-4");
    }

    [Test]
    public async Task EditProfile_UpdatesCalorieTarget()
    {
        // TODO: Navigate to /Profile/Edit, change activity level, save, assert new target
        Assert.Inconclusive("TODO: Implement edit profile test");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Fixture.Dispose();
}
