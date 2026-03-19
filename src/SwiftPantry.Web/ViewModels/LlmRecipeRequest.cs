using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.ViewModels;

public class LlmRecipeRequest
{
    public string MealType { get; set; } = "";

    public int? MaxPrepTimeMinutes { get; set; }

    [MaxLength(200)]
    public string? DietaryNotes { get; set; }

    public List<string> PantryIngredients { get; set; } = new();

    public string UserGoal { get; set; } = "";
}
