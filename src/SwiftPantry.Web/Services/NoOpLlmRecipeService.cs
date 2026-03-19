using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

/// <summary>
/// Stub implementation registered when Features:EnableLlmRecipes = false.
/// All calls throw NotImplementedException.
/// </summary>
public class NoOpLlmRecipeService : ILlmRecipeService
{
    public Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request)
        => throw new NotImplementedException(
            "LLM recipe generation is disabled. Set Features:EnableLlmRecipes = true in appsettings.json.");
}
