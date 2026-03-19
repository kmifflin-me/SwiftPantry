using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

/// <summary>
/// Real LLM recipe generation service. Registered when Features:EnableLlmRecipes = true.
/// TODO: Implement using LlmSettings:ApiKey and LlmSettings:ApiEndpoint from IConfiguration.
/// </summary>
public class LlmRecipeService(IConfiguration configuration) : ILlmRecipeService
{
    public Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request)
    {
        // TODO: Implement LLM API call using configuration["LlmSettings:ApiKey"]
        // and configuration["LlmSettings:ApiEndpoint"].
        // Parse LLM JSON response into a Recipe with IsUserCreated = true.
        throw new NotImplementedException("LlmRecipeService implementation pending.");
    }
}
