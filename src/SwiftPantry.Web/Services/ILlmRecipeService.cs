using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public interface ILlmRecipeService
{
    /// <summary>
    /// Generates a recipe using the LLM based on user context and filters.
    /// Returns null if generation fails or the feature is disabled.
    /// </summary>
    Task<Recipe?> GenerateRecipeAsync(RecipeGenerationRequest request);

    /// <summary>
    /// Returns true if the feature flag is enabled AND an API key is configured.
    /// </summary>
    bool IsAvailable { get; }
}
