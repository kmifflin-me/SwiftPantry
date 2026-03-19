using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftPantry.Web.Models;

public class MealLogEntry
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Recipe name is required.")]
    [MaxLength(200)]
    public string RecipeName { get; set; } = "";

    /// <summary>Stored as MealType enum name, e.g., "Breakfast".</summary>
    [Required(ErrorMessage = "Please select a meal type.")]
    public string MealType { get; set; } = "";

    [Required]
    [Range(0.25, 20.0, ErrorMessage = "Servings must be at least 0.25.")]
    [Column(TypeName = "decimal(6,2)")]
    public decimal Servings { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Calories must be 0 or greater.")]
    public int CaloriesPerServing { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "Protein must be 0 or greater.")]
    [Column(TypeName = "decimal(6,1)")]
    public decimal ProteinPerServing { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "Carbs must be 0 or greater.")]
    [Column(TypeName = "decimal(6,1)")]
    public decimal CarbsPerServing { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "Fat must be 0 or greater.")]
    [Column(TypeName = "decimal(6,1)")]
    public decimal FatPerServing { get; set; }

    /// <summary>UTC timestamp; set server-side on creation via DateTime.UtcNow.</summary>
    public DateTime LoggedAt { get; set; }

    /// <summary>Nullable FK; set when logged from a seeded or saved recipe.</summary>
    public int? RecipeId { get; set; }

    // ─── Computed display properties (not persisted) ────────────────────────

    [NotMapped]
    public int TotalCalories => (int)Math.Round(CaloriesPerServing * Servings);

    [NotMapped]
    public decimal TotalProteinG => Math.Round(ProteinPerServing * Servings, 1);

    [NotMapped]
    public decimal TotalCarbsG => Math.Round(CarbsPerServing * Servings, 1);

    [NotMapped]
    public decimal TotalFatG => Math.Round(FatPerServing * Servings, 1);
}
