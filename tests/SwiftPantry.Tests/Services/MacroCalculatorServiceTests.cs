using SwiftPantry.Web.Services;

namespace SwiftPantry.Tests.Services;

/// <summary>
/// Unit tests for MacroCalculatorService.
/// Reference: ACCEPTANCE_CRITERIA.md TC-CALC-1 through TC-CALC-3 and TEST_PLAN.md Section A.
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
    // BMR = 10×81.65 + 6.25×177.8 − 5×30 + 5 = 1,783 kcal
    // TDEE = 1783 × 1.55 = 2,763 kcal
    // Protein(30%) = 207 g, Carbs(40%) = 276 g, Fat(30%) = 92 g

    [Test]
    public void Calculate_TC_CALC_1_ReturnsTdee2763()
    {
        var result = _sut.Calculate(177.8m, 81.65m, 30, "Male", "ModeratelyActive", "Maintain");
        Assert.That(result.Tdee, Is.EqualTo(2763));
    }

    [Test]
    public void Calculate_TC_CALC_1_ReturnsCalorieTarget2763()
    {
        var result = _sut.Calculate(177.8m, 81.65m, 30, "Male", "ModeratelyActive", "Maintain");
        Assert.That(result.CalorieTarget, Is.EqualTo(2763));
    }

    [Test]
    public void Calculate_TC_CALC_1_ReturnsMacros_207_276_92()
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
    // Female, 25 yo, 130 lbs (58.97 kg), 64 in (162.56 cm), LightlyActive, LoseWeight
    // BMR = 10×58.97 + 6.25×162.56 − 5×25 − 161 = 1,320 kcal
    // TDEE = 1320 × 1.375 = 1,815 kcal; Target = 1,815 − 500 = 1,315 kcal
    // Protein(40%) = 132 g, Carbs(30%) = 99 g, Fat(30%) = 44 g

    [Test]
    public void Calculate_TC_CALC_2_ReturnsCalorieTarget1315()
    {
        var result = _sut.Calculate(162.56m, 58.97m, 25, "Female", "LightlyActive", "LoseWeight");
        Assert.That(result.CalorieTarget, Is.EqualTo(1315));
    }

    [Test]
    public void Calculate_TC_CALC_2_ReturnsTdee1815()
    {
        var result = _sut.Calculate(162.56m, 58.97m, 25, "Female", "LightlyActive", "LoseWeight");
        Assert.That(result.Tdee, Is.EqualTo(1815));
    }

    [Test]
    public void Calculate_TC_CALC_2_ReturnsMacros_132_99_44()
    {
        var result = _sut.Calculate(162.56m, 58.97m, 25, "Female", "LightlyActive", "LoseWeight");
        Assert.Multiple(() =>
        {
            Assert.That(result.ProteinG, Is.EqualTo(132));
            Assert.That(result.CarbsG,   Is.EqualTo(99));
            Assert.That(result.FatG,     Is.EqualTo(44));
        });
    }

    // ─── TC-CALC-3 ─────────────────────────────────────────────────────────
    // Male, 45 yo, 220 lbs (99.79 kg), 72 in (182.88 cm), VeryActive, GainWeight
    // BMR = 10×99.79 + 6.25×182.88 − 5×45 + 5 = 1,921 kcal
    // TDEE = 1921 × 1.725 = 3,314 kcal; Target = 3,314 + 300 = 3,614 kcal
    // Protein(30%) = 271 g, Carbs(45%) = 407 g, Fat(25%) = 100 g

    [Test]
    public void Calculate_TC_CALC_3_ReturnsCalorieTarget3614()
    {
        var result = _sut.Calculate(182.88m, 99.79m, 45, "Male", "VeryActive", "GainWeight");
        Assert.That(result.CalorieTarget, Is.EqualTo(3614));
    }

    [Test]
    public void Calculate_TC_CALC_3_ReturnsTdee3314()
    {
        var result = _sut.Calculate(182.88m, 99.79m, 45, "Male", "VeryActive", "GainWeight");
        Assert.That(result.Tdee, Is.EqualTo(3314));
    }

    [Test]
    public void Calculate_TC_CALC_3_ReturnsMacros_271_407_100()
    {
        var result = _sut.Calculate(182.88m, 99.79m, 45, "Male", "VeryActive", "GainWeight");
        Assert.Multiple(() =>
        {
            Assert.That(result.ProteinG, Is.EqualTo(271));
            Assert.That(result.CarbsG,   Is.EqualTo(407));
            Assert.That(result.FatG,     Is.EqualTo(100));
        });
    }

    // ─── Goal adjustments ─────────────────────────────────────────────────

    [Test]
    public void Calculate_LoseWeight_SubtractsFiveHundredFromTdee()
    {
        // Use simple values: Female, sedentary — just verify the adjustment
        var maintain = _sut.Calculate(170m, 70m, 30, "Female", "Sedentary", "Maintain");
        var lose     = _sut.Calculate(170m, 70m, 30, "Female", "Sedentary", "LoseWeight");
        Assert.That(lose.CalorieTarget, Is.EqualTo(maintain.CalorieTarget - 500));
    }

    [Test]
    public void Calculate_GainWeight_AddsThreeHundredToTdee()
    {
        var maintain = _sut.Calculate(170m, 70m, 30, "Male", "Sedentary", "Maintain");
        var gain     = _sut.Calculate(170m, 70m, 30, "Male", "Sedentary", "GainWeight");
        Assert.That(gain.CalorieTarget, Is.EqualTo(maintain.CalorieTarget + 300));
    }

    // ─── Activity multipliers ─────────────────────────────────────────────

    [Test]
    public void Calculate_Sedentary_UsesMuliplier1_2()
    {
        var result = _sut.Calculate(170m, 70m, 30, "Male", "Sedentary", "Maintain");
        // Just verify TDEE > 0 and is reasonable (not testing exact value here)
        Assert.That(result.Tdee, Is.GreaterThan(1000));
    }

    [Test]
    public void Calculate_ExtraActive_HigherTdeeThanSedentary()
    {
        var sed    = _sut.Calculate(170m, 70m, 30, "Male", "Sedentary", "Maintain");
        var extra  = _sut.Calculate(170m, 70m, 30, "Male", "ExtraActive", "Maintain");
        Assert.That(extra.Tdee, Is.GreaterThan(sed.Tdee));
    }

    // ─── No calorie floor ─────────────────────────────────────────────────

    [Test]
    public void Calculate_LowCalorieProfile_NoClamping()
    {
        // Very low: female, sedentary, lose weight — can fall below 1200
        var result = _sut.Calculate(150m, 45m, 60, "Female", "Sedentary", "LoseWeight");
        // Verify we do NOT artificially floor at 1200
        // BMR ≈ (10×45)+(6.25×150)−(5×60)−161 = 450+937.5−300−161 = 926.5
        // TDEE ≈ 926.5 × 1.2 = 1111.8; Adjusted = 611.8 < 1200 — should not be clamped
        Assert.That(result.CalorieTarget, Is.LessThan(1200));
    }
}
