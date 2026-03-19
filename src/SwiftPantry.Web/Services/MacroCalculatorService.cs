using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

/// <summary>
/// Pure Mifflin-St Jeor calculation service. Stateless singleton. No DB access.
/// </summary>
public class MacroCalculatorService : IMacroCalculatorService
{
    // Activity multipliers
    private static readonly Dictionary<string, double> ActivityMultipliers = new()
    {
        ["Sedentary"]        = 1.2,
        ["LightlyActive"]    = 1.375,
        ["ModeratelyActive"] = 1.55,
        ["VeryActive"]       = 1.725,
        ["ExtraActive"]      = 1.9
    };

    // Macro percentage splits (protein, carbs, fat) per goal
    private static readonly Dictionary<string, (double Protein, double Carbs, double Fat)> MacroSplits = new()
    {
        ["LoseWeight"] = (0.40, 0.30, 0.30),
        ["Maintain"]   = (0.30, 0.40, 0.30),
        ["GainWeight"] = (0.30, 0.45, 0.25)
    };

    // Goal calorie adjustments
    private static readonly Dictionary<string, int> GoalAdjustments = new()
    {
        ["LoseWeight"] = -500,
        ["Maintain"]   = 0,
        ["GainWeight"] = +300
    };

    public MacroTargets Calculate(decimal heightCm, decimal weightKg, int age, string sex,
        string activityLevel, string goal)
    {
        // BMR using Mifflin-St Jeor
        double bmr = (10.0 * (double)weightKg)
                   + (6.25 * (double)heightCm)
                   - (5.0 * age)
                   + (sex == "Male" ? 5 : -161);

        var multiplier = ActivityMultipliers.TryGetValue(activityLevel, out var m) ? m : 1.2;
        double tdee = bmr * multiplier;

        var adjustment = GoalAdjustments.TryGetValue(goal, out var a) ? a : 0;
        double adjustedCalories = tdee + adjustment;

        var (proteinPct, carbsPct, fatPct) = MacroSplits.TryGetValue(goal, out var split)
            ? split : (0.30, 0.40, 0.30);

        int calorieTarget = (int)Math.Round(adjustedCalories);
        int proteinG = (int)Math.Round(calorieTarget * proteinPct / 4.0);
        int carbsG   = (int)Math.Round(calorieTarget * carbsPct  / 4.0);
        int fatG     = (int)Math.Round(calorieTarget * fatPct    / 9.0);

        return new MacroTargets(
            Tdee:          (int)Math.Round(tdee),
            CalorieTarget: calorieTarget,
            ProteinG:      proteinG,
            CarbsG:        carbsG,
            FatG:          fatG
        );
    }
}
