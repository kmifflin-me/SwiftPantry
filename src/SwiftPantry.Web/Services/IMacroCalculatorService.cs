using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public interface IMacroCalculatorService
{
    /// <summary>
    /// Calculates BMR, TDEE, and daily macro targets using Mifflin-St Jeor equation.
    /// Height must be in centimeters; weight must be in kilograms.
    /// Pure calculation — no database access.
    /// </summary>
    MacroTargets Calculate(decimal heightCm, decimal weightKg, int age, string sex,
        string activityLevel, string goal);
}
