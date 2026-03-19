using Microsoft.Extensions.DependencyInjection;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.PlaywrightTests.Tests;

/// <summary>
/// Playwright E2E tests for the LLM recipe generation feature.
///
/// Uses a dedicated LlmPlaywrightFixture that runs its own Kestrel server on port 5098,
/// separate from the main fixture on 5099, so there are no port or database conflicts.
///
/// A FakeLlmRecipeService is injected (IsAvailable=true, returns a hardcoded Recipe) so:
///   - Tests are deterministic — no real Gemini API calls are made.
///   - Tests are fast — no network round-trip.
///   - Feature-off behavior is tested via the standard TestSetup.Fixture (NoOp service).
/// </summary>
[NonParallelizable]
[TestFixture]
public class LlmRecipeTests : PageTest
{
    // ─── Fake LLM service ─────────────────────────────────────────────────────

    /// <summary>
    /// Thread-safe fake ILlmRecipeService whose return value can be swapped per-test.
    /// IsAvailable is true by default so the Generate button renders.
    /// </summary>
    public sealed class FakeLlmRecipeService : ILlmRecipeService
    {
        public bool IsAvailable { get; set; } = true;

        /// <summary>When null, GenerateRecipeAsync returns null to simulate failure.</summary>
        public Recipe? RecipeToReturn { get; set; } = BuildFakeRecipe();

        public Task<Recipe?> GenerateRecipeAsync(RecipeGenerationRequest request)
            => Task.FromResult(RecipeToReturn);

        public static Recipe BuildFakeRecipe() => new()
        {
            Id                 = -1,
            Name               = "AI Lemon Herb Chicken",
            Description        = "A light, zesty chicken dish perfect for dinner.",
            MealTypes          = "dinner",
            PrepTimeMinutes    = 25,
            DefaultServings    = 2,
            CaloriesPerServing = 420,
            ProteinPerServing  = 38m,
            CarbsPerServing    = 20m,
            FatPerServing      = 18m,
            Instructions       = "Step 1: Marinate chicken.\nStep 2: Grill until cooked.",
            IsUserCreated      = true,
            Ingredients        = new List<RecipeIngredient>
            {
                new() { Id = 0, Name = "chicken breast", Quantity = "1 lb",    Category = "Protein"       },
                new() { Id = 0, Name = "lemon",          Quantity = "1 whole", Category = "Produce"       },
                new() { Id = 0, Name = "olive oil",      Quantity = "2 tbsp",  Category = "PantryStaples" }
            }
        };
    }

    // ─── Fixture subclass ─────────────────────────────────────────────────────

    /// <summary>
    /// PlaywrightFixture variant that:
    ///   - Runs on port 5098 (avoiding conflicts with the main test fixture on 5099).
    ///   - Injects FakeLlmRecipeService so Generate button renders and returns a fixed recipe.
    /// </summary>
    private sealed class LlmPlaywrightFixture : PlaywrightFixture
    {
        public const string LlmBaseUrl = "http://localhost:5098";

        // Expose the fake service so tests can mutate RecipeToReturn / IsAvailable per-test.
        public readonly FakeLlmRecipeService FakeService = new();

        public LlmPlaywrightFixture() : base(LlmBaseUrl) { }

        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            // Replace the NoOpLlmRecipeService registered by the base class
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ILlmRecipeService));
            if (descriptor != null) services.Remove(descriptor);

            services.AddSingleton<ILlmRecipeService>(FakeService);
        }
    }

    // ─── Fixture lifecycle ────────────────────────────────────────────────────

    private static readonly LlmPlaywrightFixture _fixture = new();

    [OneTimeSetUp]
    public void OneTimeSetUp() => _fixture.CreateClient();

    [OneTimeTearDown]
    public void OneTimeTearDown() => _fixture.Dispose();

    [SetUp]
    public async Task SetUp()
    {
        await _fixture.ResetDatabaseAsync();

        // Reset fake service to defaults for each test
        _fixture.FakeService.IsAvailable    = true;
        _fixture.FakeService.RecipeToReturn = FakeLlmRecipeService.BuildFakeRecipe();
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    /// <summary>Generate button is visible when the LLM service is available.</summary>
    [Test]
    public async Task GenerateButton_IsVisible_WhenLlmAvailable()
    {
        await Page.GotoAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes");
        await Expect(Page.GetByTestId("generate-recipe-button")).ToBeVisibleAsync();
    }

    /// <summary>Generate button is NOT visible when NoOpLlmRecipeService is registered (main fixture).</summary>
    [Test]
    public async Task GenerateButton_IsNotVisible_WhenNoOpServiceRegistered()
    {
        // Use the standard test fixture which has NoOpLlmRecipeService (IsAvailable=false)
        await Page.GotoAsync($"{PlaywrightFixture.BaseUrl}/Recipes");
        await Expect(Page.GetByTestId("generate-recipe-button")).Not.ToBeVisibleAsync();
    }

    /// <summary>Clicking Generate navigates to Generated page with AI badge visible.</summary>
    [Test]
    public async Task ClickGenerate_NavigatesToGeneratedPage_WithAiBadge()
    {
        await Page.GotoAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes");
        await Page.GetByTestId("generate-recipe-button").ClickAsync();

        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes/Generated");

        // AI badge must be visible
        await Expect(Page.GetByTestId("ai-generated-badge")).ToBeVisibleAsync();

        // Recipe title should be our fake recipe
        await Expect(Page.GetByTestId("recipe-title")).ToContainTextAsync("AI Lemon Herb Chicken");
    }

    /// <summary>Save action persists the LLM recipe and redirects to the Detail page.</summary>
    [Test]
    public async Task GeneratedPage_SaveAction_PersistsRecipeAndRedirectsToDetail()
    {
        await Page.GotoAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes");
        await Page.GetByTestId("generate-recipe-button").ClickAsync();
        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes/Generated");

        await Page.GetByTestId("save-recipe-button").ClickAsync();

        // Should redirect to /Recipes/Detail/{id} with a real positive ID
        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes/Detail/**");

        await Expect(Page.GetByTestId("recipe-title")).ToContainTextAsync("AI Lemon Herb Chicken");
    }

    /// <summary>Log action on a generated recipe redirects back to Generated page.</summary>
    [Test]
    public async Task GeneratedPage_LogAction_WorksWithoutPersisting()
    {
        await Page.GotoAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes");
        await Page.GetByTestId("generate-recipe-button").ClickAsync();
        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes/Generated");

        await Page.GetByTestId("log-recipe-button").ClickAsync();

        // After logging, redirect back to Generated so user can still save/add-missing
        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes/Generated");
        await Expect(Page.GetByTestId("ai-generated-badge")).ToBeVisibleAsync();
    }

    /// <summary>When generation fails, the browser page shows an error alert.</summary>
    [Test]
    public async Task GenerateButton_WhenGenerationFails_ShowsErrorAlert()
    {
        _fixture.FakeService.RecipeToReturn = null; // simulate failure

        await Page.GotoAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes");
        await Page.GetByTestId("generate-recipe-button").ClickAsync();

        // Should redirect back to recipe browser
        await Page.WaitForURLAsync($"{LlmPlaywrightFixture.LlmBaseUrl}/Recipes**");

        await Expect(Page.GetByTestId("generation-error-alert")).ToBeVisibleAsync();
    }
}
