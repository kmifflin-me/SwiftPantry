using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Data;

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

        // SavedRecipes: unique per RecipeId — no duplicate saves allowed
        modelBuilder.Entity<SavedRecipe>()
            .HasIndex(sr => sr.RecipeId)
            .IsUnique();

        // Recipe ──< RecipeIngredient (cascade delete when recipe is removed)
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // MealLogEntry.RecipeId: nullable FK, no cascade
        // (log entries survive recipe changes)
        modelBuilder.Entity<MealLogEntry>()
            .HasOne<Recipe>()
            .WithMany()
            .HasForeignKey(e => e.RecipeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
