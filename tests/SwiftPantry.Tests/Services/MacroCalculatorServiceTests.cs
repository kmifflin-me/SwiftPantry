using SwiftPantry.Web.Services;

namespace SwiftPantry.Tests.Services;

/// <summary>
/// Unit tests for MacroCalculatorService.
/// Reference: TEST_PLAN.md Section A — TC-CALC-1 through TC-CALC-4.
/// All expected values derived from Mifflin-St Jeor equation per ARCHITECTURE.md.
/// Signature: Calculate(heightCm, weightKg, age, sex, activityLevel, goal) → MacroTargets
/// </summary>
[TestFixture]
public class MacroCalculatorServiceTests
{
    private MacroCalculatorService _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new MacroCalculatorService();

    // ─── TC-CALC-1 ─────────────────────────────────────────────────────────
    // Male, 30 yo, 180 lbs (81.65 kg), 5'10" (177.8 cm), ModeratelyActive, Maintain
    // BMR = 10×81.65 + 6.25×177.8 − 5×30 + 5 = 1,783 kcal (approx)
    // TDEE = 1783 × 1.55 = 2763 kcal
    // Protein(30%) = 207 g, Carbs(40%) = 276 g, Fat(30%) = 92 g

    [Test]
    public void Calculate_MaleMaintain_ReturnsTdee2763()
    {
        var result = _sut.Calculate(177.8m, 81.65m, 30, "Male", "ModeratelyActive", "Maintain");
        Assert.That(result.Tdee, Is.EqualTo(2763));
    }

    [Test]
    public void Calculate_MaleMaintain_ReturnsCalorieTarget2763()
    {
        var result = _sut.Calculate(177.8m, 81.65m, 30, "Male", "ModeratelyActive", "Maintain");
        Assert.That(result.CalorieTarget, Is.EqualTo(2763));
    }

    [Test]
    public void Calculate_MaleMaintain_ReturnsMacros_207_276_92()
    {
        var result = _sut.Calculate(177.8m, 81.65m, 30, "Male", "ModeratelyActive", "Maintain");
        Assert.Multiple(() =>
        {
            Assert.That(result.ProteinG, Is.EqualTo(207));
            Assert.That(result.CarbsG,   Is.EqualTo(276));
            Assert.That(result.FatG,     Is.EqualTo(92));
        });
    }

    // ─── TC-CALC-2 ─────────────────────────────────────────────────────────
    // Female, 25 yo, 130 lbs (58.97 kg), 5'5" (165.1 cm), LightlyActive, LoseWeight

    [Test]
    public void Calculate_FemaleLoseWeight_ReturnsExpectedCalorieTarget()
    {
        var result = _sut.Calculate(165.1m, 58.97m, 25, "Female", "LightlyActive", "LoseWeight");
        Assert.Inconclusive($"TODO: Confirm expected value per ACCEPTANCE_CRITERIA TC-CALC-2. Got CalorieTarget={result.CalorieTarget}, Tdee={result.Tdee}");
    }

    // ─── TC-CALC-3 ─────────────────────────────────────────────────────────
    // Male, 22 yo, 160 lbs (72.57 kg), 6'0" (182.88 cm), VeryActive, GainWeight

    [Test]
    public void Calculate_MaleGainWeight_ReturnsExpectedCalorieTarget()
    {
        var result = _sut.Calculate(182.88m, 72.57m, 22, "Male", "VeryActive", "GainWeight");
        Assert.Inconclusive($"TODO: Confirm expected value per ACCEPTANCE_CRITERIA TC-CALC-3. Got CalorieTarget={result.CalorieTarget}, Tdee={result.Tdee}");
    }

    // ─── TC-CALC-4 ─────────────────────────────────────────────────────────
    // Female, 45 yo, 170 lbs (77.11 kg), 5'8" (172.72 cm), Sedentary, Maintain

    [Test]
    public void Calculate_FemaleSedentaryMaintain_ReturnsExpectedCalorieTarget()
    {
        var result = _sut.Calculate(172.72m, 77.11m, 45, "Female", "Sedentary", "Maintain");
        Assert.Inconclusive($"TODO: Confirm expected value per ACCEPTANCE_CRITERIA TC-CALC-4. Got CalorieTarget={result.CalorieTarget}, Tdee={result.Tdee}");
    }
}
