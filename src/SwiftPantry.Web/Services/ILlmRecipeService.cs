using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public interface ILlmRecipeService
{
    /// <summary>
    /// Generates a recipe using an external LLM API.
    /// The stub implementation (NoOpLlmRecipeService) throws NotImplementedException.
    /// Only active when Features:EnableLlmRecipes = true in appsettings.json.
    /// </summary>
    Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request);
}
