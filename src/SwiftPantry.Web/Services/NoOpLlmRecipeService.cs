using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

/// <summary>
/// Stub implementation. IsAvailable always returns false so the Generate button never renders.
/// Used in tests and when the feature flag is off.
/// </summary>
public class NoOpLlmRecipeService : ILlmRecipeService
{
    public bool IsAvailable => false;

    public Task<Recipe?> GenerateRecipeAsync(RecipeGenerationRequest request)
        => Task.FromResult<Recipe?>(null);
}
