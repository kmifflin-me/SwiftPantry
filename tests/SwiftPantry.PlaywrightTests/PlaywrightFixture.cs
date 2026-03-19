using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftPantry.PlaywrightTests;

/// <summary>
/// Shared test fixture that starts SwiftPantry.Web for Playwright E2E tests.
///
/// HOW IT WORKS:
///   Uses WebApplicationFactory to start the app in-process on a fixed port (5099).
///   Playwright's browser navigates to http://localhost:5099.
///
/// BROWSER INSTALL (one-time setup required):
///   In the SwiftPantry.PlaywrightTests project output directory, run:
///     pwsh bin/Debug/net10.0/playwright.ps1 install chromium
///   Or via dotnet tool:
///     dotnet tool install --global Microsoft.Playwright.CLI
///     playwright install chromium
///
/// USAGE IN TESTS:
///   [SetUp] public async Task SetUp() => await Fixture.ResetDatabaseAsync();
/// </summary>
public class PlaywrightFixture : WebApplicationFactory<Program>
{
    public const string BaseUrl = "http://localhost:5099";
    private const string TestConnectionString = "Data Source=Data/swiftpantry_test.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(BaseUrl);

        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Register test-specific SQLite DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(TestConnectionString));
        });
    }

    /// <summary>
    /// Drops and recreates the test database, applies migrations, seeds all 18 recipes,
    /// and inserts the standard fixture set (profile + 5 pantry items + 1 meal log entry).
    /// Call in [SetUp] or [OneTimeSetUp] of each test class.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // Seed 18 recipes
        var recipeService = scope.ServiceProvider.GetRequiredService<IRecipeService>();
        await recipeService.SeedRecipesIfEmptyAsync();

        // Standard fixture: UserProfile (TC-CALC-1 — Male, 30, 180 lbs, 70 in, ModeratelyActive, Maintain)
        db.UserProfiles.Add(new UserProfile
        {
            Id             = 1,
            Age            = 30,
            Sex            = "Male",
            HeightCm       = 177.8m,
            WeightKg       = 81.65m,
            HeightUnit     = "in",
            WeightUnit     = "lbs",
            ActivityLevel  = "ModeratelyActive",
            Goal           = "Maintain",
            Tdee           = 2763,
            CalorieTarget  = 2763,
            ProteinTargetG = 207,
            CarbsTargetG   = 276,
            FatTargetG     = 92
        });

        // Standard fixture: 5 pantry items
        db.PantryItems.AddRange(
            new PantryItem { Name = "chicken breast", Quantity = "6 oz",         Category = "Protein",       AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "olive oil",      Quantity = "1 bottle",     Category = "PantryStaples", AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "salt",           Quantity = "1 container",  Category = "PantryStaples", AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "black pepper",   Quantity = "1 jar",        Category = "PantryStaples", AddedAt = DateTime.UtcNow },
            new PantryItem { Name = "egg",            Quantity = "12 large",     Category = "Protein",       AddedAt = DateTime.UtcNow }
        );

        // Standard fixture: 1 meal log entry for today (Overnight Oats, recipe ID 1)
        db.MealLogEntries.Add(new MealLogEntry
        {
            RecipeName        = "Overnight Oats",
            MealType          = "Breakfast",
            Servings          = 1,
            CaloriesPerServing = 350,
            ProteinPerServing  = 15,
            CarbsPerServing    = 54,
            FatPerServing      = 8,
            LoggedAt          = DateTime.UtcNow.Date.AddHours(8),
            RecipeId          = 1
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Removes the UserProfile from the test database so the app redirects
    /// to /Profile/Setup (simulates first-time user).
    /// </summary>
    public async Task DeleteProfileAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profile = await db.UserProfiles.FirstOrDefaultAsync();
        if (profile is not null)
        {
            db.UserProfiles.Remove(profile);
            await db.SaveChangesAsync();
        }
    }
}
