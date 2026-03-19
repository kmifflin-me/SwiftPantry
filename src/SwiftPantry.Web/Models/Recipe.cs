using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftPantry.Web.Models;

public class Recipe
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = "";

    /// <summary>
    /// Comma-separated lowercase meal types, e.g., "breakfast" or "lunch,dinner".
    /// Use MealTypeList for a parsed collection.
    /// </summary>
    [Required]
    public string MealTypes { get; set; } = "";

    [Required]
    [Range(1, int.MaxValue)]
    public int PrepTimeMinutes { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DefaultServings { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int CaloriesPerServing { get; set; }

    [Column(TypeName = "decimal(6,1)")]
    public decimal ProteinPerServing { get; set; }

    [Column(TypeName = "decimal(6,1)")]
    public decimal CarbsPerServing { get; set; }

    [Column(TypeName = "decimal(6,1)")]
    public decimal FatPerServing { get; set; }

    /// <summary>Steps joined with "\n". Use InstructionList for rendering.</summary>
    public string Instructions { get; set; } = "";

    /// <summary>False for seeded recipes; true for LLM-generated user recipes.</summary>
    public bool IsUserCreated { get; set; } = false;

    // ─── Navigation ────────────────────────────────────────────────────────

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    // ─── Computed helpers (not persisted) ──────────────────────────────────

    [NotMapped]
    public List<string> MealTypeList =>
        MealTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                 .Select(s => s.Trim())
                 .ToList();

    [NotMapped]
    public List<string> InstructionList =>
        Instructions.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();
}
