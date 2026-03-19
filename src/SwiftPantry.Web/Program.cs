using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Configuration;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Middleware;
using SwiftPantry.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Allow AJAX to pass the anti-forgery token via a request header
builder.Services.AddAntiforgery(options =>
    options.HeaderName = "RequestVerificationToken");

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application services (scoped — one per request)
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMealLogService, MealLogService>();
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

// MacroCalculatorService — singleton (stateless pure math, no DB access)
builder.Services.AddSingleton<IMacroCalculatorService, MacroCalculatorService>();

// Gemini configuration
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));

// LLM service — always registered; feature flag + API key check is inside IsAvailable.
// The Generate button only renders when IsAvailable = true, so the flag stays respected.
builder.Services.AddHttpClient<LlmRecipeService>()
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<ILlmRecipeService, LlmRecipeService>();

var app = builder.Build();

// Apply migrations and run startup tasks
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed recipes from Data/seed_recipes.json if Recipes table is empty
    var recipeService = scope.ServiceProvider.GetRequiredService<IRecipeService>();
    await recipeService.SeedRecipesIfEmptyAsync();

    // Clean up meal log entries older than 7 days
    var mealLogService = scope.ServiceProvider.GetRequiredService<IMealLogService>();
    await mealLogService.CleanupOldEntriesAsync();
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Redirect to /Profile/Setup if no profile exists
app.UseMiddleware<ProfileCheckMiddleware>();

app.MapRazorPages();

app.Run();

// Required for WebApplicationFactory<Program> in Playwright tests
public partial class Program { }
