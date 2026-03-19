using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public class RecipeService(AppDbContext db, IWebHostEnvironment env) : IRecipeService
{
    public async Task<List<Recipe>> GetAllRecipesAsync()
        => await db.Recipes
            .Include(r => r.Ingredients)
            .OrderBy(r => r.Name)
            .ToListAsync();

    public async Task<Recipe?> GetByIdAsync(int id)
        => await db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<RecipeViewModel>> FilterRecipesAsync(
        RecipeFilter filter, List<string> pantryNamesLower)
    {
        var query = db.Recipes.Include(r => r.Ingredients).AsQueryable();

        // Meal type filter
        if (filter.MealTypes.Count > 0)
        {
            var lower = filter.MealTypes.Select(t => t.ToLower()).ToList();
            query = query.Where(r =>
                lower.Any(t => ("," + r.MealTypes + ",").Contains("," + t + ",")));
        }

        // Prep time filter
        if (filter.MaxPrepTimeMinutes.HasValue)
            query = query.Where(r => r.PrepTimeMinutes <= filter.MaxPrepTimeMinutes.Value);

        var recipes = await query.OrderBy(r => r.Name).ToListAsync();

        // Calculate ownership % and apply ownership filter
        var result = recipes
            .Select(r => new RecipeViewModel
            {
                Recipe = r,
                OwnershipPct = CalculateOwnershipPct(r, pantryNamesLower)
            })
            .Where(vm => !filter.MinOwnershipPct.HasValue
                         || vm.OwnershipPct >= filter.MinOwnershipPct.Value)
            .ToList();

        return result;
    }

    public int CalculateOwnershipPct(Recipe recipe, List<string> pantryNamesLower)
    {
        if (recipe.Ingredients.Count == 0) return 0;

        int owned = recipe.Ingredients
            .Count(i => pantryNamesLower.Contains(i.Name.Trim().ToLower()));

        return (int)Math.Floor((double)owned / recipe.Ingredients.Count * 100);
    }

    public string GetDefaultMealType(TimeOnly time)
    {
        if (time < new TimeOnly(10, 0)) return "breakfast";
        if (time < new TimeOnly(14, 0)) return "lunch";
        if (time < new TimeOnly(17, 0)) return "snack";
        return "dinner";
    }

    public async Task<SavedRecipe?> GetSavedRecipeAsync(int recipeId)
        => await db.SavedRecipes.FirstOrDefaultAsync(sr => sr.RecipeId == recipeId);

    public async Task<List<SavedRecipe>> GetAllSavedRecipesAsync()
        => await db.SavedRecipes
            .Include(sr => sr.Recipe)
                .ThenInclude(r => r.Ingredients)
            .OrderByDescending(sr => sr.SavedAt)
            .ToListAsync();

    public async Task<SavedRecipe> SaveRecipeAsync(int recipeId)
    {
        var existing = await db.SavedRecipes.FirstOrDefaultAsync(sr => sr.RecipeId == recipeId);
        if (existing is not null) return existing;

        var saved = new SavedRecipe { RecipeId = recipeId, SavedAt = DateTime.UtcNow };
        db.SavedRecipes.Add(saved);
        await db.SaveChangesAsync();
        return saved;
    }

    public async Task DeleteSavedRecipeAsync(int savedRecipeId)
    {
        var saved = await db.SavedRecipes.FindAsync(savedRecipeId);
        if (saved is not null)
        {
            db.SavedRecipes.Remove(saved);
            await db.SaveChangesAsync();
        }
    }

    public async Task SeedRecipesIfEmptyAsync()
    {
        if (await db.Recipes.AnyAsync()) return;

        var jsonPath = Path.Combine(env.ContentRootPath, "Data", "seed_recipes.json");
        if (!File.Exists(jsonPath)) return;

        var recipes = SeedData.LoadSeedRecipes(jsonPath);
        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();
    }
}
