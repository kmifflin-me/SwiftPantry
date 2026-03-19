using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class RecipeIngredient
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecipeId { get; set; }

    /// <summary>Used for case-insensitive pantry matching.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    /// <summary>Free text, e.g., "1 lb", "3 tbsp", "to taste", "1/2 cup".</summary>
    [Required]
    [MaxLength(100)]
    public string Quantity { get; set; } = "";

    /// <summary>
    /// IngredientCategory enum name from seed data (e.g., "PantryStaples").
    /// Not used when adding to shopping list — items added via AddMissing always use "Other".
    /// </summary>
    public string Category { get; set; } = "Other";

    // ─── Navigation ────────────────────────────────────────────────────────
    public Recipe Recipe { get; set; } = null!;
}
