using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class PantryItem
{
    [Key]
    public int Id { get; set; }

    /// <summary>Trimmed before save. Uniqueness enforced case-insensitively in PantryService.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Quantity is required.")]
    [MaxLength(100)]
    public string Quantity { get; set; } = "";

    /// <summary>Stored as IngredientCategory enum name, e.g., "Protein" or "PantryStaples".</summary>
    [Required(ErrorMessage = "Please select a category.")]
    public string Category { get; set; } = "";

    /// <summary>UTC timestamp set server-side on creation.</summary>
    public DateTime AddedAt { get; set; }
}
