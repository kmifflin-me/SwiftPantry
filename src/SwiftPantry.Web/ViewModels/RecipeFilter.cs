namespace SwiftPantry.Web.ViewModels;

public class RecipeFilter
{
    /// <summary>Empty list = show all meal types. Values are lowercase, e.g., "breakfast".</summary>
    public List<string> MealTypes { get; set; } = new();

    /// <summary>Null = no prep time filter.</summary>
    public int? MaxPrepTimeMinutes { get; set; }

    /// <summary>Null = no ownership filter. 0–100 when set.</summary>
    public int? MinOwnershipPct { get; set; }
}
