namespace SwiftPantry.Web.Models;

public enum Sex { Male, Female }

public enum ActivityLevel
{
    Sedentary,
    LightlyActive,
    ModeratelyActive,
    VeryActive,
    ExtraActive
}

public enum Goal { LoseWeight, Maintain, GainWeight }

public enum MealType { Breakfast, Lunch, Dinner, Snack }

public enum IngredientCategory
{
    Produce,
    Protein,
    Dairy,
    Grains,
    PantryStaples,
    Frozen,
    Other
}

/// <summary>Fixed display order and display names for ingredient categories.</summary>
public static class CategoryOrder
{
    public static readonly IReadOnlyList<string> Ordered =
    [
        "Produce",
        "Protein",
        "Dairy",
        "Grains",
        "PantryStaples",
        "Frozen",
        "Other"
    ];

    public static readonly IReadOnlyDictionary<string, string> DisplayNames =
        new Dictionary<string, string>
        {
            ["Produce"]       = "Produce",
            ["Protein"]       = "Protein",
            ["Dairy"]         = "Dairy",
            ["Grains"]        = "Grains",
            ["PantryStaples"] = "Pantry Staples",
            ["Frozen"]        = "Frozen",
            ["Other"]         = "Other"
        };
}
