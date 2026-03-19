# Architecture Document — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved (Pre-implementation)

> This document is the technical contract for the Developer Agent. Follow it exactly. Do not make architectural decisions not described here.

---

## 1. Solution Structure

```
SwiftPantry/
├── SwiftPantry.sln
├── src/
│   └── SwiftPantry.Web/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── SeedData.cs
│       │   └── seed_recipes.json
│       ├── Models/
│       │   ├── UserProfile.cs
│       │   ├── MealLogEntry.cs
│       │   ├── PantryItem.cs
│       │   ├── ShoppingListItem.cs
│       │   ├── Recipe.cs
│       │   ├── RecipeIngredient.cs
│       │   └── SavedRecipe.cs
│       ├── Services/
│       │   ├── IProfileService.cs
│       │   ├── ProfileService.cs
│       │   ├── IMacroCalculatorService.cs
│       │   ├── MacroCalculatorService.cs
│       │   ├── IMealLogService.cs
│       │   ├── MealLogService.cs
│       │   ├── IPantryService.cs
│       │   ├── PantryService.cs
│       │   ├── IShoppingListService.cs
│       │   ├── ShoppingListService.cs
│       │   ├── IRecipeService.cs
│       │   ├── RecipeService.cs
│       │   ├── ILlmRecipeService.cs
│       │   ├── NoOpLlmRecipeService.cs
│       │   └── LlmRecipeService.cs
│       ├── Middleware/
│       │   └── ProfileCheckMiddleware.cs
│       ├── Pages/
│       │   ├── Index.cshtml
│       │   ├── Index.cshtml.cs
│       │   ├── Profile/
│       │   │   ├── Setup.cshtml
│       │   │   ├── Setup.cshtml.cs
│       │   │   ├── Index.cshtml
│       │   │   ├── Index.cshtml.cs
│       │   │   ├── Edit.cshtml
│       │   │   └── Edit.cshtml.cs
│       │   ├── MealLog.cshtml
│       │   ├── MealLog.cshtml.cs
│       │   ├── Pantry.cshtml
│       │   ├── Pantry.cshtml.cs
│       │   ├── ShoppingList.cshtml
│       │   ├── ShoppingList.cshtml.cs
│       │   ├── Recipes/
│       │   │   ├── Index.cshtml
│       │   │   ├── Index.cshtml.cs
│       │   │   ├── Detail.cshtml
│       │   │   └── Detail.cshtml.cs
│       │   └── SavedRecipes.cshtml
│       │   └── SavedRecipes.cshtml.cs
│       ├── ViewModels/
│       │   ├── MacroTargets.cs
│       │   ├── DailySummary.cs
│       │   ├── RecipeFilter.cs
│       │   ├── RecipeViewModel.cs
│       │   └── LlmRecipeRequest.cs
│       └── wwwroot/
│           ├── css/
│           │   └── site.css
│           └── js/
│               └── site.js
├── tests/
│   ├── SwiftPantry.Tests/
│   │   ├── SwiftPantry.Tests.csproj
│   │   └── Services/
│   │       ├── MacroCalculatorServiceTests.cs
│   │       └── RecipeServiceTests.cs
│   └── SwiftPantry.PlaywrightTests/
│       ├── SwiftPantry.PlaywrightTests.csproj
│       ├── GlobalUsings.cs
│       ├── PlaywrightFixture.cs
│       ├── Helpers/
│       │   └── TestHelpers.cs
│       ├── PageObjects/
│       │   ├── ProfilePage.cs
│       │   ├── DashboardPage.cs
│       │   ├── PantryPage.cs
│       │   ├── ShoppingListPage.cs
│       │   ├── RecipeBrowserPage.cs
│       │   ├── RecipeDetailPage.cs
│       │   └── MealLogPage.cs
│       └── Tests/
│           ├── FirstTimeUserTests.cs
│           ├── ProfileTests.cs
│           ├── RecipeBrowsingTests.cs
│           ├── RecipeActionTests.cs
│           ├── PantryTests.cs
│           ├── ShoppingListTests.cs
│           ├── MealLoggingTests.cs
│           ├── DataPersistenceTests.cs
│           └── NavigationAndEmptyStateTests.cs
```

---

## 2. Data Models

### Enums (shared across models)

Define in `Models/Enums.cs`:

```csharp
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
```

Display name helper for `IngredientCategory`:
- `PantryStaples` → "Pantry Staples" (all others use their enum name as-is)

Category display order (fixed for all pages): Produce → Protein → Dairy → Grains → PantryStaples → Frozen → Other

---

### UserProfile

```
Table: UserProfiles
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Single row; always Id=1 |
| Age | int | `[Range(10, 120)]` | |
| Sex | string | `[Required]` | Stored as enum name ("Male"/"Female") |
| HeightCm | decimal | `[Required]` | Always stored in cm; convert on input/display |
| WeightKg | decimal | `[Required]` | Always stored in kg; convert on input/display |
| HeightUnit | string | `[Required]` | "in" or "cm" — user's display preference |
| WeightUnit | string | `[Required]` | "lbs" or "kg" — user's display preference |
| ActivityLevel | string | `[Required]` | Stored as enum name |
| Goal | string | `[Required]` | Stored as enum name |
| CalorieTarget | int | | Calculated and stored on save/update |
| ProteinTargetG | int | | Calculated and stored on save/update |
| CarbsTargetG | int | | Calculated and stored on save/update |
| FatTargetG | int | | Calculated and stored on save/update |
| Tdee | int | | Calculated and stored on save/update |

**Display helpers (not persisted):**
- `DisplayHeight`: HeightUnit=="in" → `HeightCm / 2.54m` else `HeightCm`
- `DisplayWeight`: WeightUnit=="lbs" → `WeightKg * 2.20462m` else `WeightKg`

---

### MealLogEntry

```
Table: MealLogEntries
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Auto-increment |
| RecipeName | string | `[Required][MaxLength(200)]` | |
| MealType | string | `[Required]` | Enum name |
| Servings | decimal | `[Range(0.25, 20)]` | |
| CaloriesPerServing | int | `[Range(0, int.MaxValue)]` | |
| ProteinPerServing | decimal | `[Range(0, double.MaxValue)]` | grams |
| CarbsPerServing | decimal | `[Range(0, double.MaxValue)]` | grams |
| FatPerServing | decimal | `[Range(0, double.MaxValue)]` | grams |
| LoggedAt | DateTime | | UTC; set server-side on creation |
| RecipeId | int? | FK nullable | Set when logged from a Recipe |

**Computed display properties (not persisted):**
- `TotalCalories` → `(int)Math.Round(CaloriesPerServing * Servings)`
- `TotalProteinG` → `Math.Round(ProteinPerServing * Servings, 1)`
- `TotalCarbsG` → `Math.Round(CarbsPerServing * Servings, 1)`
- `TotalFatG` → `Math.Round(FatPerServing * Servings, 1)`

---

### PantryItem

```
Table: PantryItems
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Auto-increment |
| Name | string | `[Required][MaxLength(200)]` | Trimmed before save; uniqueness enforced in service (case-insensitive) |
| Quantity | string | `[Required][MaxLength(100)]` | Free text |
| Category | string | `[Required]` | IngredientCategory enum name |
| AddedAt | DateTime | | UTC; set server-side on creation |

---

### ShoppingListItem

```
Table: ShoppingListItems
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Auto-increment |
| Name | string | `[Required][MaxLength(200)]` | Duplicates allowed (unlike pantry) |
| Quantity | string | `[Required][MaxLength(100)]` | Free text |
| Category | string | `[Required]` | IngredientCategory enum name |
| IsPurchased | bool | | Default: false |
| AddedAt | DateTime | | UTC; set server-side on creation |

---

### Recipe

```
Table: Recipes
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Seeded recipes use IDs 1–18 from JSON |
| Name | string | `[Required][MaxLength(200)]` | |
| Description | string | `[Required][MaxLength(500)]` | |
| MealTypes | string | `[Required]` | Comma-separated, lowercase: e.g., "breakfast" or "lunch,dinner" |
| PrepTimeMinutes | int | `[Range(1, int.MaxValue)]` | |
| DefaultServings | int | `[Range(1, int.MaxValue)]` | |
| CaloriesPerServing | int | `[Range(0, int.MaxValue)]` | |
| ProteinPerServing | decimal | | grams |
| CarbsPerServing | decimal | | grams |
| FatPerServing | decimal | | grams |
| Instructions | string | | Stored as newline-delimited steps (join array on "\n" when seeding from JSON) |
| IsUserCreated | bool | | false for seeded; true for LLM-generated |
| Ingredients | `ICollection<RecipeIngredient>` | Navigation | Cascade delete |

**Parsed helper (not persisted):**
- `MealTypeList` → `MealTypes.Split(',').Select(s => s.Trim()).ToList()`
- `InstructionList` → `Instructions.Split('\n').ToList()`

---

### RecipeIngredient

```
Table: RecipeIngredients
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Auto-increment |
| RecipeId | int | `[Required]` FK | Cascade delete |
| Name | string | `[Required][MaxLength(200)]` | Used for pantry case-insensitive matching |
| Quantity | string | `[Required][MaxLength(100)]` | Free text, e.g., "1 lb", "to taste" |
| Category | string | | IngredientCategory enum name; present in JSON but not used for shopping list (always "Other") |
| Recipe | Recipe | Navigation | |

---

### SavedRecipe

```
Table: SavedRecipes
```

| Column | C# Type | EF/Annotation | Notes |
|--------|---------|---------------|-------|
| Id | int | `[Key]` | Auto-increment |
| RecipeId | int | `[Required]` FK | Unique constraint enforced (no duplicate saves) |
| SavedAt | DateTime | | UTC; set server-side on creation |
| Recipe | Recipe | Navigation | |

**EF Fluent API required:**
```csharp
modelBuilder.Entity<SavedRecipe>()
    .HasIndex(sr => sr.RecipeId)
    .IsUnique();
```

---

## 3. Service Interface Contracts

### MacroTargets (ViewModel)

```csharp
public record MacroTargets(int Tdee, int CalorieTarget, int ProteinG, int CarbsG, int FatG);
```

### DailySummary (ViewModel)

```csharp
public record DailySummary(
    int CaloriesConsumed, int CaloriesTarget,
    decimal ProteinConsumed, int ProteinTarget,
    decimal CarbsConsumed, int CarbsTarget,
    decimal FatConsumed, int FatTarget
)
{
    public int CaloriesPct => CaloriesTarget > 0
        ? Math.Min(100, (int)((CaloriesConsumed / (double)CaloriesTarget) * 100)) : 0;
    public bool CaloriesOver => CaloriesConsumed > CaloriesTarget;
    // same pattern for Protein, Carbs, Fat
}
```

### RecipeFilter (ViewModel)

```csharp
public class RecipeFilter
{
    public List<string> MealTypes { get; set; } = new(); // empty = all
    public int? MaxPrepTimeMinutes { get; set; }
    public int? MinOwnershipPct { get; set; }
}
```

### LlmRecipeRequest (ViewModel)

```csharp
public class LlmRecipeRequest
{
    public string MealType { get; set; } = "";
    public int? MaxPrepTimeMinutes { get; set; }
    public string? DietaryNotes { get; set; }
    public List<string> PantryIngredients { get; set; } = new();
    public string UserGoal { get; set; } = "";
}
```

---

### IMacroCalculatorService

**Pure calculation — no DB access, no DI dependencies.**

```csharp
public interface IMacroCalculatorService
{
    /// <summary>
    /// Calculates BMR, TDEE, and macro targets from profile data.
    /// Uses Mifflin-St Jeor equation. Height in cm, weight in kg.
    /// </summary>
    MacroTargets Calculate(decimal heightCm, decimal weightKg, int age, string sex,
        string activityLevel, string goal);
}
```

Activity multipliers:
- Sedentary: 1.2 | LightlyActive: 1.375 | ModeratelyActive: 1.55 | VeryActive: 1.725 | ExtraActive: 1.9

Calorie adjustments:
- LoseWeight: TDEE − 500 | Maintain: TDEE | GainWeight: TDEE + 300

Macro splits (protein/carbs/fat as % of adjusted calories):
- LoseWeight: 40/30/30 | Maintain: 30/40/30 | GainWeight: 30/45/25

Conversion: protein/carbs = 4 kcal/g; fat = 9 kcal/g. Round each macro to nearest whole gram.

---

### IProfileService

**DI dependency: `AppDbContext`, `IMacroCalculatorService`**

```csharp
public interface IProfileService
{
    /// <summary>Returns the single user profile, or null if none exists.</summary>
    Task<UserProfile?> GetProfileAsync();

    /// <summary>Returns true if a UserProfile row exists.</summary>
    Task<bool> ProfileExistsAsync();

    /// <summary>
    /// Creates a new UserProfile. Calculates and stores macro targets.
    /// Throws InvalidOperationException if a profile already exists.
    /// </summary>
    Task<UserProfile> CreateProfileAsync(UserProfile profile);

    /// <summary>
    /// Updates the existing profile. Recalculates and stores macro targets.
    /// Throws InvalidOperationException if no profile exists.
    /// </summary>
    Task<UserProfile> UpdateProfileAsync(UserProfile profile);
}
```

---

### IMealLogService

**DI dependency: `AppDbContext`**

```csharp
public interface IMealLogService
{
    /// <summary>Returns all entries whose LoggedAt (server local date) matches the given date.</summary>
    Task<List<MealLogEntry>> GetEntriesForDateAsync(DateOnly date);

    /// <summary>Aggregates consumed macros for the date and returns with the user's targets.</summary>
    Task<DailySummary> GetDailySummaryAsync(DateOnly date, UserProfile profile);

    /// <summary>Creates a new MealLogEntry. Sets LoggedAt = DateTime.UtcNow.</summary>
    Task<MealLogEntry> AddEntryAsync(MealLogEntry entry);

    /// <summary>Permanently deletes a log entry by ID.</summary>
    Task DeleteEntryAsync(int id);

    /// <summary>
    /// Deletes all MealLogEntry rows where LoggedAt (UTC) is more than 7 days
    /// before the current server date. Called once on startup.
    /// </summary>
    Task CleanupOldEntriesAsync();
}
```

---

### IPantryService

**DI dependency: `AppDbContext`**

```csharp
public interface IPantryService
{
    /// <summary>Returns all pantry items, ordered by Category (fixed order), then Name ascending.</summary>
    Task<List<PantryItem>> GetAllItemsAsync();

    /// <summary>Returns a single pantry item by ID, or null if not found.</summary>
    Task<PantryItem?> GetByIdAsync(int id);

    /// <summary>
    /// Returns true if any pantry item has the given name (case-insensitive, trimmed).
    /// excludeId: when editing, exclude the item being edited from the uniqueness check.
    /// </summary>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>Adds a new pantry item. Sets AddedAt = DateTime.UtcNow. Caller must check uniqueness first.</summary>
    Task<PantryItem> AddItemAsync(PantryItem item);

    /// <summary>Updates an existing pantry item. Caller must check uniqueness first.</summary>
    Task<PantryItem> UpdateItemAsync(PantryItem item);

    /// <summary>Permanently deletes a pantry item.</summary>
    Task DeleteItemAsync(int id);

    /// <summary>Returns all pantry item names (lowercase, trimmed) for recipe matching.</summary>
    Task<List<string>> GetAllNamesLowercaseAsync();
}
```

---

### IShoppingListService

**DI dependency: `AppDbContext`, `IPantryService`**

```csharp
public interface IShoppingListService
{
    /// <summary>Returns all shopping list items.</summary>
    Task<List<ShoppingListItem>> GetAllItemsAsync();

    /// <summary>Adds a new item. Duplicates by name are allowed. Sets AddedAt = UtcNow.</summary>
    Task<ShoppingListItem> AddItemAsync(ShoppingListItem item);

    /// <summary>Permanently deletes an item.</summary>
    Task DeleteItemAsync(int id);

    /// <summary>Sets IsPurchased = true for the given item.</summary>
    Task MarkPurchasedAsync(int id);

    /// <summary>Bulk-deletes all items where IsPurchased = true.</summary>
    Task DeleteAllPurchasedAsync();

    /// <summary>
    /// For each RecipeIngredient in the recipe that is NOT in pantryNamesLower:
    ///   - Scale quantity by (requestedServings / recipe.DefaultServings)
    ///   - Add a ShoppingListItem with Category = "Other"
    /// Returns count of items added.
    /// </summary>
    Task<int> AddMissingIngredientsAsync(Recipe recipe, List<string> pantryNamesLower,
        decimal requestedServings);

    /// <summary>
    /// Moves a purchased shopping list item to the pantry.
    /// Returns true on success, false if a pantry item with the same name already exists.
    /// On success: creates PantryItem (same name/quantity/category), deletes the ShoppingListItem.
    /// On conflict: no data is changed.
    /// </summary>
    Task<bool> MoveToPantryAsync(int shoppingListItemId);
}
```

**Quantity scaling algorithm** (shared with ACCEPTANCE_CRITERIA UT-039–UT-042):
```
given quantity string:
  1. Regex match leading decimal: ^(\d+\.?\d*)\s*(.*)$
  2. If match: scaledNum = (parsedNum * requestedServings / defaultServings), round to 2dp
               result = $"{scaledNum:F2} {unit}".Trim()
  3. If no match (e.g., "to taste"): return quantity unchanged
  Note: "1/2 cup" will NOT match the leading decimal regex — return unchanged (documented behavior per UT-042)
```

---

### IRecipeService

**DI dependency: `AppDbContext`**

```csharp
public interface IRecipeService
{
    /// <summary>Returns all recipes including their Ingredients collection.</summary>
    Task<List<Recipe>> GetAllRecipesAsync();

    /// <summary>Returns a single recipe with Ingredients, or null if not found.</summary>
    Task<Recipe?> GetByIdAsync(int id);

    /// <summary>
    /// Returns filtered recipes with ownership % calculated per pantryNamesLower.
    /// filter.MealTypes empty = all types; MaxPrepTimeMinutes null = no filter; MinOwnershipPct null = no filter.
    /// Results sorted by Name ascending.
    /// </summary>
    Task<List<(Recipe Recipe, int OwnershipPct)>> FilterRecipesAsync(
        RecipeFilter filter, List<string> pantryNamesLower);

    /// <summary>
    /// Calculates ingredient ownership % for a recipe against pantryNamesLower.
    /// floor(matchCount / totalIngredients * 100). Returns 0 if no ingredients.
    /// </summary>
    int CalculateOwnershipPct(Recipe recipe, List<string> pantryNamesLower);

    /// <summary>Returns the default meal type string for the given time of day (server local).</summary>
    string GetDefaultMealType(TimeOnly time);

    /// <summary>Returns the saved recipe record for the given recipeId, or null.</summary>
    Task<SavedRecipe?> GetSavedRecipeAsync(int recipeId);

    /// <summary>Returns all saved recipes with their Recipe navigation property loaded, sorted by SavedAt DESC.</summary>
    Task<List<SavedRecipe>> GetAllSavedRecipesAsync();

    /// <summary>
    /// Creates a SavedRecipe. Idempotent: if already saved, returns the existing record.
    /// Sets SavedAt = UtcNow on new creation only.
    /// </summary>
    Task<SavedRecipe> SaveRecipeAsync(int recipeId);

    /// <summary>Deletes a SavedRecipe by its ID (not RecipeId).</summary>
    Task DeleteSavedRecipeAsync(int savedRecipeId);

    /// <summary>
    /// Seeds the Recipes table from Data/seed_recipes.json if the table is empty.
    /// Idempotent: skips if any recipe rows exist.
    /// </summary>
    Task SeedRecipesIfEmptyAsync();
}
```

**Time-of-day meal type defaults:**
- Before 10:00 → "Breakfast"
- 10:00–13:59 → "Lunch"
- 14:00–16:59 → "Snack"
- 17:00+ → "Dinner"

---

### ILlmRecipeService

**Feature-flagged. DI dependency: `IConfiguration` (for real impl only)**

```csharp
public interface ILlmRecipeService
{
    /// <summary>Generates a recipe using LLM. Stub throws NotImplementedException.</summary>
    Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request);
}
```

**NoOpLlmRecipeService** (registered when `Features:EnableLlmRecipes = false`):
```csharp
public class NoOpLlmRecipeService : ILlmRecipeService
{
    public Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request)
        => throw new NotImplementedException("LLM recipe generation is not enabled.");
}
```

**LlmRecipeService** (registered when `Features:EnableLlmRecipes = true`):
- Stub only in scaffold. Implementation to be added.

---

### DI Graph

```
MacroCalculatorService     ← no deps
ProfileService             ← AppDbContext, IMacroCalculatorService
MealLogService             ← AppDbContext
PantryService              ← AppDbContext
ShoppingListService        ← AppDbContext, IPantryService
RecipeService              ← AppDbContext
LlmRecipeService           ← IConfiguration (real impl only)
```

---

## 4. Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/swiftpantry.db"
  },
  "Features": {
    "EnableLlmRecipes": false
  },
  "LlmSettings": {
    "ApiKey": "",
    "ApiEndpoint": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/swiftpantry.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

## 5. DI Registration and Program.cs

### Exact Program.cs Structure

```csharp
using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services — scoped (one per request)
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMealLogService, MealLogService>();
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

// MacroCalculatorService — singleton (stateless pure math)
builder.Services.AddSingleton<IMacroCalculatorService, MacroCalculatorService>();

// LLM service — conditional on feature flag
var enableLlm = builder.Configuration.GetValue<bool>("Features:EnableLlmRecipes");
if (enableLlm)
    builder.Services.AddScoped<ILlmRecipeService, LlmRecipeService>();
else
    builder.Services.AddScoped<ILlmRecipeService, NoOpLlmRecipeService>();

var app = builder.Build();

// Ensure DB is created and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed recipes if empty
    var recipeService = scope.ServiceProvider.GetRequiredService<IRecipeService>();
    await recipeService.SeedRecipesIfEmptyAsync();

    // Clean up old meal log entries (>7 days)
    var mealLogService = scope.ServiceProvider.GetRequiredService<IMealLogService>();
    await mealLogService.CleanupOldEntriesAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Profile check middleware: redirect to /Profile/Setup if no profile exists
app.UseMiddleware<ProfileCheckMiddleware>();

app.MapRazorPages();

// Make Program accessible for WebApplicationFactory in tests
public partial class Program { }
```

### ProfileCheckMiddleware

```csharp
// Middleware/ProfileCheckMiddleware.cs
public class ProfileCheckMiddleware(RequestDelegate next)
{
    private static readonly string[] ExcludedPaths =
    [
        "/Profile/Setup",
        "/Error",
        "/favicon.ico",
    ];

    public async Task InvokeAsync(HttpContext context, IProfileService profileService)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip static files, excluded pages, and POST requests to setup
        bool isExcluded = ExcludedPaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || path.StartsWith("/_", StringComparison.OrdinalIgnoreCase) // Blazor/internal
            || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase);

        if (!isExcluded && !await profileService.ProfileExistsAsync())
        {
            context.Response.Redirect("/Profile/Setup");
            return;
        }

        await next(context);
    }
}
```

### AppDbContext

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<MealLogEntry> MealLogEntries => Set<MealLogEntry>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<SavedRecipe> SavedRecipes => Set<SavedRecipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique index: one save per recipe
        modelBuilder.Entity<SavedRecipe>()
            .HasIndex(sr => sr.RecipeId)
            .IsUnique();

        // Cascade delete ingredients when recipe is deleted
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Root Redirect (Index.cshtml.cs)

```csharp
// Pages/Index.cshtml.cs — redirects / → /MealLog
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/MealLog");
}
```

---

## 6. Playwright Test Infrastructure

### PlaywrightFixture.cs

```csharp
// tests/SwiftPantry.PlaywrightTests/PlaywrightFixture.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;

public class PlaywrightFixture : WebApplicationFactory<Program>
{
    public const string TestDbPath = "Data/swiftpantry_test.db";
    private const string TestConnectionString = "Data Source=Data/swiftpantry_test.db";

    // The base URL that Playwright navigates to.
    // Set in test project via ASPNETCORE_URLS environment var or read from server.
    public string BaseUrl { get; private set; } = "http://localhost:5099";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(BaseUrl);
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Register test DbContext pointing at test DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(TestConnectionString));
        });
    }

    /// <summary>
    /// Wipes and recreates the test database, applies migrations,
    /// seeds all 18 recipes, and inserts the standard fixture set.
    /// Call in [OneTimeSetUp] or [SetUp] of each test class.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // Seed recipes via RecipeService
        var recipeService = scope.ServiceProvider
            .GetRequiredService<IRecipeService>();
        await recipeService.SeedRecipesIfEmptyAsync();

        // Standard fixture: UserProfile (TC-CALC-1 values)
        db.UserProfiles.Add(new UserProfile
        {
            Id = 1,
            Age = 30,
            Sex = "Male",
            HeightCm = 177.8m,
            WeightKg = 81.65m,
            HeightUnit = "in",
            WeightUnit = "lbs",
            ActivityLevel = "ModeratelyActive",
            Goal = "Maintain",
            CalorieTarget = 2763,
            ProteinTargetG = 207,
            CarbsTargetG = 276,
            FatTargetG = 92,
            Tdee = 2763
        });

        // Standard fixture: 5 pantry items
        db.PantryItems.AddRange(
            new PantryItem { Name = "chicken breast", Quantity = "6 oz",      Category = "Protein",       AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "olive oil",       Quantity = "1 bottle",  Category = "PantryStaples", AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "salt",            Quantity = "1 container",Category = "PantryStaples",AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "black pepper",    Quantity = "1 jar",     Category = "PantryStaples", AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "egg",             Quantity = "12 large",  Category = "Protein",       AddedAt = DateTime.UtcNow }
        );

        // Standard fixture: 1 meal log entry for today
        db.MealLogEntries.Add(new MealLogEntry
        {
            RecipeName = "Overnight Oats",
            MealType = "Breakfast",
            Servings = 1,
            CaloriesPerServing = 350,
            ProteinPerServing = 15,
            CarbsPerServing = 54,
            FatPerServing = 8,
            LoggedAt = DateTime.UtcNow.Date.AddHours(8),
            RecipeId = 1
        });

        await db.SaveChangesAsync();
    }
}
```

> **Note:** Playwright requires a running HTTP server. The fixture uses `WebApplicationFactory<Program>` but Playwright's browser cannot connect to the in-process `TestServer` directly. **Option A (recommended):** Use `CreateClient()` to start the in-process server and configure Playwright to use the base address from the factory. **Option B (simpler for Playwright):** Start the app as a background process on port 5099 (via `dotnet run` in test setup). The scaffold uses a `TODO` comment for the Developer to implement the chosen approach.

### GlobalUsings.cs

```csharp
global using Microsoft.Playwright;
global using Microsoft.Playwright.NUnit;
global using NUnit.Framework;
global using SwiftPantry.Web.Data;
global using SwiftPantry.Web.Models;
global using SwiftPantry.Web.Services;
```

### SwiftPantry.PlaywrightTests.csproj — Required NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.51.0" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
</ItemGroup>
```

---

### data-testid Attribute Contract

All attributes below are **mandatory** in Razor markup. The Developer adds them; the QA Agent references them in tests.

#### Profile Setup & Edit Pages (`/Profile/Setup`, `/Profile/Edit`)

| Attribute | Element |
|-----------|---------|
| `data-testid="age-input"` | Age field `<input>` |
| `data-testid="sex-select"` | Sex `<select>` |
| `data-testid="height-input"` | Height field `<input>` |
| `data-testid="height-unit-select"` | Height unit `<select>` |
| `data-testid="weight-input"` | Weight field `<input>` |
| `data-testid="weight-unit-select"` | Weight unit `<select>` |
| `data-testid="activity-level-select"` | Activity level `<select>` |
| `data-testid="goal-select"` | Goal `<select>` |
| `data-testid="save-profile-button"` | Save/Submit button |

#### Profile View Page (`/Profile`)

| Attribute | Element |
|-----------|---------|
| `data-testid="calorie-target"` | Calculated calorie display |
| `data-testid="protein-target"` | Protein grams display |
| `data-testid="carbs-target"` | Carbs grams display |
| `data-testid="fat-target"` | Fat grams display |
| `data-testid="tdee-display"` | TDEE display |

#### Dashboard / Meal Log Page (`/MealLog`)

| Attribute | Element |
|-----------|---------|
| `data-testid="calorie-progress-bar"` | Calorie progress bar `<div class="progress-bar">` |
| `data-testid="protein-progress-bar"` | Protein progress bar |
| `data-testid="carbs-progress-bar"` | Carbs progress bar |
| `data-testid="fat-progress-bar"` | Fat progress bar |
| `data-testid="calorie-progress-label"` | "X / Y kcal" text |
| `data-testid="protein-progress-label"` | "X / Y g" text |
| `data-testid="carbs-progress-label"` | "X / Y g" text |
| `data-testid="fat-progress-label"` | "X / Y g" text |
| `data-testid="todays-meals-list"` | Container `<ul>` for logged meals |
| `data-testid="meal-entry-{id}"` | Each `<li>` entry row |
| `data-testid="delete-entry-{id}"` | Delete button for entry |
| `data-testid="quick-log-section"` | Quick log panel container |
| `data-testid="quick-log-item-{id}"` | Each saved recipe card in quick log |
| `data-testid="quick-log-button-{id}"` | "Log" button for each saved recipe |
| `data-testid="no-profile-prompt"` | "Set up your profile" empty state |
| `data-testid="no-entries-state"` | "No meals logged" empty state |
| `data-testid="date-picker"` | Date input |
| `data-testid="manual-meal-name"` | Manual log Name input |
| `data-testid="manual-meal-type"` | Manual log MealType select |
| `data-testid="manual-servings"` | Manual log Servings input |
| `data-testid="manual-calories"` | Manual log Calories input |
| `data-testid="manual-protein"` | Manual log Protein input |
| `data-testid="manual-carbs"` | Manual log Carbs input |
| `data-testid="manual-fat"` | Manual log Fat input |
| `data-testid="manual-submit"` | Manual log Submit button |

#### Pantry Page (`/Pantry`)

| Attribute | Element |
|-----------|---------|
| `data-testid="pantry-name-input"` | Name input in Add form |
| `data-testid="pantry-quantity-input"` | Quantity input in Add form |
| `data-testid="pantry-category-select"` | Category select in Add form |
| `data-testid="add-pantry-button"` | Add to Pantry submit button |
| `data-testid="pantry-items-list"` | Container for all category groups |
| `data-testid="pantry-empty-state"` | Empty state message container |
| `data-testid="pantry-item-{id}"` | Each ingredient row (view state) |
| `data-testid="pantry-edit-btn-{id}"` | Edit button for item |
| `data-testid="pantry-delete-btn-{id}"` | Delete button/form for item |
| `data-testid="pantry-edit-form-{id}"` | Edit form for item (hidden by default) |
| `data-testid="pantry-edit-name-{id}"` | Name input in edit form |
| `data-testid="pantry-edit-quantity-{id}"` | Quantity input in edit form |
| `data-testid="pantry-edit-category-{id}"` | Category select in edit form |
| `data-testid="pantry-save-btn-{id}"` | Save button in edit form |

#### Shopping List Page (`/ShoppingList`)

| Attribute | Element |
|-----------|---------|
| `data-testid="shopping-name-input"` | Name input in Add form |
| `data-testid="shopping-quantity-input"` | Quantity input in Add form |
| `data-testid="shopping-category-select"` | Category select in Add form |
| `data-testid="add-shopping-button"` | Add to List submit button |
| `data-testid="shopping-items-list"` | Container for all category groups |
| `data-testid="shopping-empty-state"` | Empty state message container |
| `data-testid="shopping-item-{id}"` | Each list item `<li>` |
| `data-testid="shopping-check-{id}"` | Checkbox input for item |
| `data-testid="shopping-add-to-pantry-{id}"` | "Add to Pantry" button |
| `data-testid="shopping-delete-btn-{id}"` | Delete button for item |
| `data-testid="clear-purchased-button"` | "Clear Purchased" button |

#### Recipe Browser Page (`/Recipes`)

| Attribute | Element |
|-----------|---------|
| `data-testid="recipe-grid"` | Recipe cards container |
| `data-testid="recipe-card-{id}"` | Each recipe card |
| `data-testid="filter-meal-type-all"` | "All" checkbox |
| `data-testid="filter-meal-type-breakfast"` | Breakfast checkbox |
| `data-testid="filter-meal-type-lunch"` | Lunch checkbox |
| `data-testid="filter-meal-type-dinner"` | Dinner checkbox |
| `data-testid="filter-meal-type-snack"` | Snack checkbox |
| `data-testid="filter-max-prep-time"` | Max prep time select |
| `data-testid="filter-min-owned"` | Min ingredients owned select |
| `data-testid="filter-submit-button"` | Filter submit button |
| `data-testid="recipes-empty-state"` | "No recipes match" message |
| `data-testid="recipe-view-btn-{id}"` | "View Recipe" button on each card |
| `data-testid="recipe-ownership-{id}"` | Ownership badge/text on each card |

#### Recipe Detail Page (`/Recipes/{id}`)

| Attribute | Element |
|-----------|---------|
| `data-testid="recipe-title"` | Recipe name `<h1>` |
| `data-testid="recipe-description"` | Description paragraph |
| `data-testid="servings-input"` | Servings number input (`id="servingsInput"`) |
| `data-testid="ingredients-list"` | Ingredients `<ul>` |
| `data-testid="ingredient-qty-{index}"` | Quantity span for ingredient at 0-based index |
| `data-testid="instructions-list"` | Instructions `<ol>` |
| `data-testid="total-calories"` | Total calories display (`id="totalCalories"`) |
| `data-testid="total-protein"` | Total protein display (`id="totalProtein"`) |
| `data-testid="total-carbs"` | Total carbs display (`id="totalCarbs"`) |
| `data-testid="total-fat"` | Total fat display (`id="totalFat"`) |
| `data-testid="save-recipe-button"` | Save Recipe button |
| `data-testid="saved-recipe-badge"` | "Saved ✓" disabled button (shown when saved) |
| `data-testid="log-meal-button"` | "Log This Meal" button (opens modal) |
| `data-testid="add-missing-button"` | "Add Missing Ingredients" button |
| `data-testid="ownership-badge"` | Ownership % badge in sidebar |
| `data-testid="log-meal-modal"` | Log meal modal container |
| `data-testid="log-modal-servings"` | Servings input inside modal |
| `data-testid="log-modal-meal-type"` | Meal type select inside modal |
| `data-testid="log-modal-confirm"` | Confirm button inside modal |

#### Saved Recipes Page (`/SavedRecipes`)

| Attribute | Element |
|-----------|---------|
| `data-testid="saved-recipes-grid"` | Cards grid container |
| `data-testid="saved-recipe-card-{id}"` | Each saved recipe card |
| `data-testid="saved-recipe-view-{id}"` | "View Recipe" link on card |
| `data-testid="saved-recipe-remove-{id}"` | "Remove" button on card |
| `data-testid="saved-recipes-empty-state"` | Empty state container |

---

## 7. Key Technical Decisions

1. **Recipe seeding:** Recipes are seeded from `Data/seed_recipes.json` into the `Recipes` SQLite table on startup when the table is empty. Idempotent — skipped if rows exist. Recipe data lives in the DB; the JSON file is the source for initial seeding only.

2. **Pantry matching:** Case-insensitive string comparison using `.Equals(name, StringComparison.OrdinalIgnoreCase)` after `.Trim()`. No fuzzy matching. "Pantry Staples" category is stored as enum name `"PantryStaples"`.

3. **JSON seed mapping:** The seed JSON uses lowercase meal types (`"breakfast"`) and "Pantry Staples" (with space) for categories. When seeding:
   - `mealTypes` array → join with comma, store lowercase: `"breakfast"` or `"lunch,dinner"`
   - `category` values with spaces → strip space before storing as enum name: `"Pantry Staples"` → `"PantryStaples"`
   - `instructions` array → join with `"\n"`
   - `servings` field in JSON → `DefaultServings` in model

4. **All times use server local time** for display/date grouping; `LoggedAt` is stored as UTC.

5. **EF migrations:** Code-first. Initial migration named `InitialCreate` created during scaffold. Apply with `db.Database.Migrate()` in startup.

6. **Anti-forgery for AJAX:** The `MarkPurchased` endpoint must accept anti-forgery token as a request header (`RequestVerificationToken`). Use `[ValidateAntiForgeryToken]` on the page handler or configure a middleware-level CSRF validator. Simplest: use `[IgnoreAntiforgeryToken]` only on that specific handler since it still requires CORS to be effective, OR use `services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken")` globally.

7. **Playwright browser install:** After adding the Playwright NuGet package, run the following in the `SwiftPantry.PlaywrightTests` project output directory to install Chromium:
   ```
   pwsh bin/Debug/net8.0/playwright.ps1 install chromium
   ```
   If PowerShell is unavailable on the build agent, run:
   ```
   dotnet tool install --global Microsoft.Playwright.CLI
   playwright install chromium
   ```
   **The Developer or QA Agent must run this command before executing Playwright tests. The scaffold does not run it automatically.**

8. **`public partial class Program { }`** must appear at the bottom of `Program.cs` to enable `WebApplicationFactory<Program>` from the test project.

9. **Category display order (fixed):** Produce, Protein, Dairy, Grains, PantryStaples, Frozen, Other. Pages must render category groups in this order. Use a static list as the canonical sort key.

10. **Navbar active class:** Set `active` class on nav items server-side using `ViewContext.RouteData.Values["page"]` or the current `HttpContext.Request.Path`. Use a `_Layout.cshtml` partial with this logic.
