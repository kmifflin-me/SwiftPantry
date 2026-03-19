using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.ViewModels;

/// <summary>Recipe with pre-calculated ownership percentage for display on the browser page.</summary>
public class RecipeViewModel
{
    public Recipe Recipe { get; set; } = null!;
    public int OwnershipPct { get; set; }
}
