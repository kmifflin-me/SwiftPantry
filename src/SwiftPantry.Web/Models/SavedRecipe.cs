using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class SavedRecipe
{
    [Key]
    public int Id { get; set; }

    /// <summary>Unique index enforced in AppDbContext.OnModelCreating — no duplicate saves.</summary>
    [Required]
    public int RecipeId { get; set; }

    /// <summary>UTC timestamp set server-side on creation.</summary>
    public DateTime SavedAt { get; set; }

    // ─── Navigation ────────────────────────────────────────────────────────
    public Recipe Recipe { get; set; } = null!;
}
