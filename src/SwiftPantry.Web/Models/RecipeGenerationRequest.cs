namespace SwiftPantry.Web.Models;

public class RecipeGenerationRequest
{
    /// <summary>User's daily calorie target and macro goals for context.</summary>
    public int DailyCalorieTarget { get; set; }
    public int ProteinTargetGrams { get; set; }
    public int CarbsTargetGrams { get; set; }
    public int FatTargetGrams { get; set; }

    /// <summary>Ingredients the user currently has in their pantry.</summary>
    public List<string> PantryIngredients { get; set; } = new();

    /// <summary>Filter preferences passed from the recipe browser.</summary>
    public string? MealType { get; set; } // breakfast, lunch, dinner, snack
    public int? MaxPrepTimeMinutes { get; set; }

    /// <summary>How strongly to prefer pantry ingredients (0.0 to 1.0).</summary>
    public double PantryPreference { get; set; } = 0.7;
}
