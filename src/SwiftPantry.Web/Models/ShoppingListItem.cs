using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class ShoppingListItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Quantity is required.")]
    [MaxLength(100)]
    public string Quantity { get; set; } = "";

    /// <summary>Stored as IngredientCategory enum name. Items added via AddMissing always use "Other".</summary>
    [Required(ErrorMessage = "Please select a category.")]
    public string Category { get; set; } = "";

    /// <summary>False by default. Set true via MarkPurchased action.</summary>
    public bool IsPurchased { get; set; } = false;

    /// <summary>UTC timestamp set server-side on creation.</summary>
    public DateTime AddedAt { get; set; }
}
