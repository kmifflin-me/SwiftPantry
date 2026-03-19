using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwiftPantry.Web.Middleware;

namespace SwiftPantry.PlaywrightTests;

/// <summary>
/// Shared test fixture that starts SwiftPantry.Web via a real Kestrel server on port 5099.
///
/// Uses WebApplication.CreateBuilder directly (not WebApplicationFactory) so that
/// Playwright's browser process can connect over TCP.
///
/// BROWSER INSTALL (one-time setup required):
///   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
///
/// USAGE IN TESTS:
///   [OneTimeSetUp] public void OneTimeSetUp() => Fixture.CreateClient();
///   [SetUp]        public async Task SetUp() => await Fixture.ResetDatabaseAsync();
///   [OneTimeTearDown] public void OneTimeTearDown() => Fixture.Dispose();
/// </summary>
public class PlaywrightFixture : IDisposable
{
    public const string BaseUrl = "http://localhost:5099";

    // Absolute path to the web project so Razor Pages, wwwroot, and Data/ are found.
    private static readonly string WebProjectPath = FindWebProjectPath();

    private WebApplication? _app;
    private bool _disposed;

    // Per-instance URL and DB path — subclasses may use a different port/DB to avoid conflicts.
    private readonly string _baseUrl;
    private readonly string _testConnectionString;

    public PlaywrightFixture() : this(BaseUrl) { }

    protected PlaywrightFixture(string baseUrl)
    {
        _baseUrl = baseUrl;
        // Derive a unique DB filename from the port so multiple fixtures don't share a file.
        var port = new Uri(baseUrl).Port;
        var dbPath = Path.Combine(WebProjectPath, "Data", $"swiftpantry_test_{port}.db");
        _testConnectionString = $"Data Source={dbPath};Pooling=False";
    }

    /// <summary>Exposes the DI container for ad-hoc service access from tests.</summary>
    public IServiceProvider Services =>
        _app?.Services ?? throw new InvalidOperationException("Call CreateClient() first.");

    // ─── Server lifecycle ──────────────────────────────────────────────────

    /// <summary>
    /// Builds and starts the Kestrel web server.  Call once in [OneTimeSetUp].
    /// Subsequent calls are no-ops.
    /// </summary>
    public void CreateClient()
    {
        if (_app != null) return;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = WebProjectPath,
            EnvironmentName = "Development"
        });

        builder.WebHost.UseUrls(_baseUrl);

        // ── Services (mirror Program.cs) ───────────────────────────────────
        builder.Services.AddRazorPages();
        builder.Services.AddAntiforgery(opts =>
            opts.HeaderName = "RequestVerificationToken");

        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(_testConnectionString));

        builder.Services.AddScoped<IProfileService, ProfileService>();
        builder.Services.AddScoped<IMealLogService, MealLogService>();
        builder.Services.AddScoped<IPantryService, PantryService>();
        builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
        builder.Services.AddScoped<IRecipeService, RecipeService>();
        builder.Services.AddSingleton<IMacroCalculatorService, MacroCalculatorService>();
        builder.Services.AddScoped<ILlmRecipeService, NoOpLlmRecipeService>();

        // Extension point for test subclasses that need to swap services (e.g., LLM tests)
        ConfigureAdditionalServices(builder.Services);

        _app = builder.Build();

        // ── Pipeline (mirror Program.cs, omit HTTPS redirect for tests) ───
        _app.UseStaticFiles();
        _app.UseRouting();
        _app.UseAuthorization();
        _app.UseMiddleware<ProfileCheckMiddleware>();
        _app.MapRazorPages();

        // Start the Kestrel server (non-blocking)
        _app.StartAsync().GetAwaiter().GetResult();

        // One-time DB initialisation: drop-create + seed 18 recipes.
        // Subsequent per-test resets only truncate user data (faster, no file lock).
        using var initScope = _app.Services.CreateScope();
        var initDb = initScope.ServiceProvider.GetRequiredService<AppDbContext>();
        initDb.Database.EnsureDeleted();
        initDb.Database.Migrate();
        var initRecipes = initScope.ServiceProvider.GetRequiredService<IRecipeService>();
        initRecipes.SeedRecipesIfEmptyAsync().GetAwaiter().GetResult();
    }

    // ─── Database helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Clears all user-generated data and re-inserts the standard fixture set.
    /// Uses raw SQL for all operations to avoid Windows file-lock issues and
    /// EF Core ValueGeneratedOnAdd interference with explicit-Id inserts.
    /// Call in [SetUp] of each test class.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Explicit transaction ensures all DELETEs + INSERTs are committed atomically
        await using var tx = await db.Database.BeginTransactionAsync();

        // Truncate user data tables (recipes stay — they never change between tests)
        await db.Database.ExecuteSqlRawAsync("DELETE FROM SavedRecipes");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ShoppingListItems");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM MealLogEntries");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM PantryItems");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM UserProfiles");

        // Standard fixture: UserProfile (TC-CALC-1 — Male, 30, 180 lbs, 70 in, ModeratelyActive, Maintain)
        // Raw SQL bypasses EF Core ValueGeneratedOnAdd to guarantee the row is written.
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO UserProfiles
                (Age, Sex, HeightCm, WeightKg, HeightUnit, WeightUnit,
                 ActivityLevel, Goal, Tdee, CalorieTarget, ProteinTargetG, CarbsTargetG, FatTargetG)
            VALUES
                (30, 'Male', 177.8, 81.65, 'in', 'lbs',
                 'ModeratelyActive', 'Maintain', 2763, 2763, 207, 276, 92)");

        // Standard fixture: 5 pantry items
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO PantryItems (Name, Quantity, Category, AddedAt) VALUES
                ('chicken breast', '6 oz',        'Protein',       datetime('now')),
                ('olive oil',      '1 bottle',    'PantryStaples', datetime('now')),
                ('salt',           '1 container', 'PantryStaples', datetime('now')),
                ('black pepper',   '1 jar',       'PantryStaples', datetime('now')),
                ('egg',            '12 large',    'Protein',       datetime('now'))");

        // Standard fixture: 1 meal log entry for today (Overnight Oats, recipe ID 1)
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO MealLogEntries
                (RecipeName, MealType, Servings, CaloriesPerServing,
                 ProteinPerServing, CarbsPerServing, FatPerServing, LoggedAt, RecipeId)
            VALUES
                ('Overnight Oats', 'Breakfast', 1, 350, 15, 54, 8, datetime('now'), 1)");

        await tx.CommitAsync();

        // Verify the profile was actually written — surfaces INSERT failures as setup errors
        // rather than as confusing Playwright timeouts.
        var profileCount = await db.UserProfiles.CountAsync();
        if (profileCount == 0)
            throw new InvalidOperationException(
                "ResetDatabaseAsync: UserProfile seed failed — table is empty after INSERT");
    }

    /// <summary>
    /// Removes the UserProfile so the app redirects to /Profile/Setup.
    /// Used in FirstTimeUser tests.
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

    // ─── Cleanup ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_app is not null)
        {
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
            _app.StopAsync(cts.Token).GetAwaiter().GetResult();
            ((IAsyncDisposable)_app).DisposeAsync().GetAwaiter().GetResult();
            _app = null;
        }
    }

    // ─── Extension points for subclasses ──────────────────────────────────────

    /// <summary>
    /// Override to register additional or replacement services for this fixture's server.
    /// Called after all standard services are registered.
    /// </summary>
    protected virtual void ConfigureAdditionalServices(IServiceCollection services) { }

    // ─── Path helpers ──────────────────────────────────────────────────────

    private static string FindWebProjectPath()
    {
        // Walk up from the test output directory until we find the repo root
        // (identified by having both "src" and "tests" subdirectories)
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "SwiftPantry.Web");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            $"Could not find solution root (src/SwiftPantry.Web) starting from: {AppContext.BaseDirectory}");
    }
}
