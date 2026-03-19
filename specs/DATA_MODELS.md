# Data Models — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved (Pre-implementation)

> Full C# class definitions ready to copy into the project. All classes belong to namespace `SwiftPantry.Web.Models`.

---

## Database Table Diagram

```
UserProfiles
  Id (PK)
  Age | Sex | HeightCm | WeightKg | HeightUnit | WeightUnit
  ActivityLevel | Goal
  CalorieTarget | ProteinTargetG | CarbsTargetG | FatTargetG | Tdee

MealLogEntries
  Id (PK)
  RecipeName | MealType | Servings
  CaloriesPerServing | ProteinPerServing | CarbsPerServing | FatPerServing
  LoggedAt | RecipeId (FK → Recipes.Id, nullable)

PantryItems
  Id (PK)
  Name | Quantity | Category | AddedAt

ShoppingListItems
  Id (PK)
  Name | Quantity | Category | IsPurchased | AddedAt

Recipes
  Id (PK)
  Name | Description | MealTypes | PrepTimeMinutes | DefaultServings
  CaloriesPerServing | ProteinPerServing | CarbsPerServing | FatPerServing
  Instructions | IsUserCreated

RecipeIngredients
  Id (PK)
  RecipeId (FK → Recipes.Id, CASCADE DELETE)
  Name | Quantity | Category

SavedRecipes
  Id (PK)
  RecipeId (FK → Recipes.Id, UNIQUE INDEX)
  SavedAt

Relationships:
  Recipes 1──* RecipeIngredients   (cascade delete)
  Recipes 1──* MealLogEntries      (nullable FK, no cascade)
  Recipes 1──* SavedRecipes        (unique per RecipeId)
```

---

## Enums

```csharp
// Models/Enums.cs
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

/// <summary>Fixed display order for category grouping on Pantry and Shopping List pages.</summary>
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
```

---

## UserProfile

```csharp
// Models/UserProfile.cs
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

    [Required(ErrorMessage = "Please select a sex.")]
    public string Sex { get; set; } = "";

    /// <summary>Always stored in centimeters.</summary>
    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal HeightCm { get; set; }

    /// <summary>Always stored in kilograms.</summary>
    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal WeightKg { get; set; }

    /// <summary>"in" or "cm" — user's display preference for height.</summary>
    [Required]
    public string HeightUnit { get; set; } = "in";

    /// <summary>"lbs" or "kg" — user's display preference for weight.</summary>
    [Required]
    public string WeightUnit { get; set; } = "lbs";

    [Required(ErrorMessage = "Please select an activity level.")]
    public string ActivityLevel { get; set; } = "";

    [Required(ErrorMessage = "Please select a goal.")]
    public string Goal { get; set; } = "";

    /// <summary>Calculated and stored on save/update.</summary>
    public int Tdee { get; set; }

    /// <summary>Calculated and stored on save/update. Adjusted for goal.</summary>
    public int CalorieTarget { get; set; }

    /// <summary>Calculated macro target in grams.</summary>
    public int ProteinTargetG { get; set; }

    /// <summary>Calculated macro target in grams.</summary>
    public int CarbsTargetG { get; set; }

    /// <summary>Calculated macro target in grams.</summary>
    public int FatTargetG { get; set; }

    // ─── Display helpers (not persisted) ───────────────────────────────────

    [NotMapped]
    public decimal DisplayHeight =>
        HeightUnit == "in" ? Math.Round(HeightCm / 2.54m, 1) : HeightCm;

    [NotMapped]
    public decimal DisplayWeight =>
        WeightUnit == "lbs" ? Math.Round(WeightKg * 2.20462m, 1) : WeightKg;
}
```

**Example JSON (for testing):**
```json
{
  "id": 1,
  "age": 30,
  "sex": "Male",
  "heightCm": 177.8,
  "weightKg": 81.65,
  "heightUnit": "in",
  "weightUnit": "lbs",
  "activityLevel": "ModeratelyActive",
  "goal": "Maintain",
  "tdee": 2763,
  "calorieTarget": 2763,
  "proteinTargetG": 207,
  "carbsTargetG": 276,
  "fatTargetG": 92
}
```

---

## MealLogEntry

```csharp
// Models/MealLogEntry.cs
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

    /// <summary>UTC timestamp set on creation.</summary>
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
```

**Example JSON:**
```json
{
  "id": 1,
  "recipeName": "Overnight Oats",
  "mealType": "Breakfast",
  "servings": 1.0,
  "caloriesPerServing": 350,
  "proteinPerServing": 15.0,
  "carbsPerServing": 54.0,
  "fatPerServing": 8.0,
  "loggedAt": "2026-03-19T08:00:00Z",
  "recipeId": 1
}
```

---

## PantryItem

```csharp
// Models/PantryItem.cs
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class PantryItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Quantity is required.")]
    [MaxLength(100)]
    public string Quantity { get; set; } = "";

    /// <summary>Stored as IngredientCategory enum name, e.g., "PantryStaples".</summary>
    [Required(ErrorMessage = "Please select a category.")]
    public string Category { get; set; } = "";

    /// <summary>UTC timestamp set on creation.</summary>
    public DateTime AddedAt { get; set; }
}
```

**Example JSON:**
```json
{
  "id": 1,
  "name": "chicken breast",
  "quantity": "6 oz",
  "category": "Protein",
  "addedAt": "2026-03-19T10:00:00Z"
}
```

---

## ShoppingListItem

```csharp
// Models/ShoppingListItem.cs
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

    /// <summary>Stored as IngredientCategory enum name, e.g., "Other".</summary>
    [Required(ErrorMessage = "Please select a category.")]
    public string Category { get; set; } = "";

    /// <summary>False by default; set true via MarkPurchased action.</summary>
    public bool IsPurchased { get; set; } = false;

    /// <summary>UTC timestamp set on creation.</summary>
    public DateTime AddedAt { get; set; }
}
```

**Example JSON:**
```json
{
  "id": 5,
  "name": "mixed green",
  "quantity": "3.00 cup",
  "category": "Other",
  "isPurchased": false,
  "addedAt": "2026-03-19T14:30:00Z"
}
```

---

## Recipe

```csharp
// Models/Recipe.cs
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
    /// Comma-separated meal types in lowercase, e.g., "breakfast" or "lunch,dinner".
    /// Matches the seed JSON format directly.
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

    /// <summary>Steps joined with "\n". Split on "\n" to render as numbered list.</summary>
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
                    .ToList();
}
```

**Example JSON (seeded recipe #6):**
```json
{
  "id": 6,
  "name": "Grilled Chicken Salad",
  "description": "Tender grilled chicken breast over mixed greens with cherry tomatoes and a light vinaigrette.",
  "mealTypes": "lunch",
  "prepTimeMinutes": 20,
  "defaultServings": 1,
  "caloriesPerServing": 325,
  "proteinPerServing": 38.0,
  "carbsPerServing": 12.0,
  "fatPerServing": 14.0,
  "instructions": "Season chicken breast with salt and pepper.\nGrill or pan-sear over medium-high heat...",
  "isUserCreated": false,
  "ingredients": [...]
}
```

---

## RecipeIngredient

```csharp
// Models/RecipeIngredient.cs
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class RecipeIngredient
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecipeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    /// <summary>Free text, e.g., "1 lb", "3 tbsp", "to taste", "1/2 cup".</summary>
    [Required]
    [MaxLength(100)]
    public string Quantity { get; set; } = "";

    /// <summary>
    /// Stored as IngredientCategory enum name (e.g., "PantryStaples").
    /// Present for reference; not used when adding to shopping list (always "Other" there).
    /// </summary>
    public string Category { get; set; } = "Other";

    // ─── Navigation ────────────────────────────────────────────────────────
    public Recipe Recipe { get; set; } = null!;
}
```

**Example JSON:**
```json
{
  "id": 41,
  "recipeId": 6,
  "name": "chicken breast",
  "quantity": "6 oz",
  "category": "Protein"
}
```

---

## SavedRecipe

```csharp
// Models/SavedRecipe.cs
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Models;

public class SavedRecipe
{
    [Key]
    public int Id { get; set; }

    /// <summary>Unique index enforced in AppDbContext.OnModelCreating.</summary>
    [Required]
    public int RecipeId { get; set; }

    /// <summary>UTC timestamp set on creation.</summary>
    public DateTime SavedAt { get; set; }

    // ─── Navigation ────────────────────────────────────────────────────────
    public Recipe Recipe { get; set; } = null!;
}
```

**Example JSON:**
```json
{
  "id": 1,
  "recipeId": 6,
  "savedAt": "2026-03-19T15:00:00Z"
}
```

---

## EF Fluent API Configuration (AppDbContext)

```csharp
// Data/AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // SavedRecipes: unique per RecipeId (no duplicate saves)
    modelBuilder.Entity<SavedRecipe>()
        .HasIndex(sr => sr.RecipeId)
        .IsUnique();

    // Recipe ──< RecipeIngredient (cascade delete)
    modelBuilder.Entity<Recipe>()
        .HasMany(r => r.Ingredients)
        .WithOne(i => i.Recipe)
        .HasForeignKey(i => i.RecipeId)
        .OnDelete(DeleteBehavior.Cascade);

    // MealLogEntry.RecipeId: nullable FK, no cascade (log entries survive recipe changes)
    modelBuilder.Entity<MealLogEntry>()
        .HasOne<Recipe>()
        .WithMany()
        .HasForeignKey(e => e.RecipeId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);
}
```

---

## Seed JSON Mapping Notes

When `SeedData.cs` reads `seed_recipes.json` and populates the `Recipes` table:

| JSON field | DB field | Transformation |
|-----------|----------|----------------|
| `id` | `Recipe.Id` | Direct (1–18) |
| `name` | `Recipe.Name` | Direct |
| `description` | `Recipe.Description` | Direct |
| `mealTypes` (array) | `Recipe.MealTypes` (string) | `string.Join(",", arr)` — already lowercase |
| `prepTimeMinutes` | `Recipe.PrepTimeMinutes` | Direct |
| `servings` | `Recipe.DefaultServings` | Renamed field |
| `caloriesPerServing` | `Recipe.CaloriesPerServing` | Direct |
| `proteinPerServing` | `Recipe.ProteinPerServing` | Direct |
| `carbsPerServing` | `Recipe.CarbsPerServing` | Direct |
| `fatPerServing` | `Recipe.FatPerServing` | Direct |
| `instructions` (array) | `Recipe.Instructions` (string) | `string.Join("\n", arr)` |
| `isUserCreated` | `Recipe.IsUserCreated` | `false` (hardcoded for seeded) |
| `ingredients[].name` | `RecipeIngredient.Name` | Direct |
| `ingredients[].quantity` | `RecipeIngredient.Quantity` | Direct |
| `ingredients[].category` | `RecipeIngredient.Category` | Strip spaces: `"Pantry Staples"` → `"PantryStaples"` |

**Seed JSON DTO class** (used only in `SeedData.cs`):

```csharp
// Data/SeedData.cs — internal DTO for deserialization
internal class RecipeSeedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [JsonPropertyName("mealTypes")]
    public List<string> MealTypesArray { get; set; } = new();
    public int PrepTimeMinutes { get; set; }
    [JsonPropertyName("servings")]
    public int DefaultServings { get; set; }
    public int CaloriesPerServing { get; set; }
    public decimal ProteinPerServing { get; set; }
    public decimal CarbsPerServing { get; set; }
    public decimal FatPerServing { get; set; }
    public List<string> Instructions { get; set; } = new();
    public List<IngredientSeedDto> Ingredients { get; set; } = new();
}

internal class IngredientSeedDto
{
    public string Name { get; set; } = "";
    public string Quantity { get; set; } = "";
    public string Category { get; set; } = "";
}
```

**Mapping from DTO to Model:**
```csharp
var recipe = new Recipe
{
    Id = dto.Id,
    Name = dto.Name,
    Description = dto.Description,
    MealTypes = string.Join(",", dto.MealTypesArray),
    PrepTimeMinutes = dto.PrepTimeMinutes,
    DefaultServings = dto.DefaultServings,
    CaloriesPerServing = dto.CaloriesPerServing,
    ProteinPerServing = dto.ProteinPerServing,
    CarbsPerServing = dto.CarbsPerServing,
    FatPerServing = dto.FatPerServing,
    Instructions = string.Join("\n", dto.Instructions),
    IsUserCreated = false,
    Ingredients = dto.Ingredients.Select(i => new RecipeIngredient
    {
        Name = i.Name,
        Quantity = i.Quantity,
        // "Pantry Staples" → "PantryStaples", others pass through
        Category = i.Category.Replace(" ", "")
    }).ToList()
};
```

---

## ViewModels

```csharp
// ViewModels/MacroTargets.cs
namespace SwiftPantry.Web.ViewModels;

public record MacroTargets(int Tdee, int CalorieTarget, int ProteinG, int CarbsG, int FatG);
```

```csharp
// ViewModels/DailySummary.cs
namespace SwiftPantry.Web.ViewModels;

public record DailySummary(
    int CaloriesConsumed,
    int CaloriesTarget,
    decimal ProteinConsumed,
    int ProteinTarget,
    decimal CarbsConsumed,
    int CarbsTarget,
    decimal FatConsumed,
    int FatTarget)
{
    public int CaloriesPct =>
        CaloriesTarget > 0 ? Math.Min(100, (int)(CaloriesConsumed / (double)CaloriesTarget * 100)) : 0;
    public bool CaloriesOver => CaloriesConsumed > CaloriesTarget;

    public int ProteinPct =>
        ProteinTarget > 0 ? Math.Min(100, (int)(ProteinConsumed / (double)ProteinTarget * 100)) : 0;
    public bool ProteinOver => ProteinConsumed > ProteinTarget;

    public int CarbsPct =>
        CarbsTarget > 0 ? Math.Min(100, (int)(CarbsConsumed / (double)CarbsTarget * 100)) : 0;
    public bool CarbsOver => CarbsConsumed > CarbsTarget;

    public int FatPct =>
        FatTarget > 0 ? Math.Min(100, (int)(FatConsumed / (double)FatTarget * 100)) : 0;
    public bool FatOver => FatConsumed > FatTarget;
}
```

```csharp
// ViewModels/RecipeFilter.cs
namespace SwiftPantry.Web.ViewModels;

public class RecipeFilter
{
    /// <summary>Empty list = show all. Values are lowercase meal type strings.</summary>
    public List<string> MealTypes { get; set; } = new();
    public int? MaxPrepTimeMinutes { get; set; }
    /// <summary>0–100. Null = no filter.</summary>
    public int? MinOwnershipPct { get; set; }
}
```

```csharp
// ViewModels/RecipeViewModel.cs
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.ViewModels;

/// <summary>Recipe with calculated ownership % for display on the browser page.</summary>
public class RecipeViewModel
{
    public Recipe Recipe { get; set; } = null!;
    public int OwnershipPct { get; set; }
}
```

```csharp
// ViewModels/LlmRecipeRequest.cs
namespace SwiftPantry.Web.ViewModels;

public class LlmRecipeRequest
{
    public string MealType { get; set; } = "";
    public int? MaxPrepTimeMinutes { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? DietaryNotes { get; set; }
    public List<string> PantryIngredients { get; set; } = new();
    public string UserGoal { get; set; } = "";
}
```
