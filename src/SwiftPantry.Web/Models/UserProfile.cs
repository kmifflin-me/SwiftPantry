using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftPantry.Web.Models;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(10, 120, ErrorMessage = "Age must be between 10 and 120.")]
    public int Age { get; set; }

    /// <summary>Stored as enum name: "Male" or "Female".</summary>
    [Required(ErrorMessage = "Please select a sex.")]
    public string Sex { get; set; } = "";

    /// <summary>Always stored in centimeters. Convert from inches on input if HeightUnit = "in".</summary>
    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal HeightCm { get; set; }

    /// <summary>Always stored in kilograms. Convert from lbs on input if WeightUnit = "lbs".</summary>
    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal WeightKg { get; set; }

    /// <summary>"in" or "cm" — user's display preference for height input/output.</summary>
    [Required]
    public string HeightUnit { get; set; } = "in";

    /// <summary>"lbs" or "kg" — user's display preference for weight input/output.</summary>
    [Required]
    public string WeightUnit { get; set; } = "lbs";

    /// <summary>Stored as ActivityLevel enum name, e.g., "ModeratelyActive".</summary>
    [Required(ErrorMessage = "Please select an activity level.")]
    public string ActivityLevel { get; set; } = "";

    /// <summary>Stored as Goal enum name, e.g., "Maintain".</summary>
    [Required(ErrorMessage = "Please select a goal.")]
    public string Goal { get; set; } = "";

    /// <summary>Calculated TDEE (BMR × activity multiplier), stored on save.</summary>
    public int Tdee { get; set; }

    /// <summary>Adjusted calorie target (TDEE ± goal adjustment), stored on save.</summary>
    public int CalorieTarget { get; set; }

    /// <summary>Protein target in grams, stored on save.</summary>
    public int ProteinTargetG { get; set; }

    /// <summary>Carbohydrate target in grams, stored on save.</summary>
    public int CarbsTargetG { get; set; }

    /// <summary>Fat target in grams, stored on save.</summary>
    public int FatTargetG { get; set; }

    // ─── Display helpers (not persisted) ───────────────────────────────────

    /// <summary>Height in user's preferred display unit.</summary>
    [NotMapped]
    public decimal DisplayHeight =>
        HeightUnit == "in" ? Math.Round(HeightCm / 2.54m, 1) : HeightCm;

    /// <summary>Weight in user's preferred display unit.</summary>
    [NotMapped]
    public decimal DisplayWeight =>
        WeightUnit == "lbs" ? Math.Round(WeightKg * 2.20462m, 1) : WeightKg;
}
