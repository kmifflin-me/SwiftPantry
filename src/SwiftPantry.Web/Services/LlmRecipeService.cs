using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SwiftPantry.Web.Configuration;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

/// <summary>
/// Real LLM recipe generation service using the Google Gemini API (free tier).
/// Endpoint: https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}
/// Authentication: API key as a query parameter.
/// No SDK required — raw HttpClient with JSON.
///
/// To configure:
///   1. Get a free API key at https://aistudio.google.com/apikey
///   2. Set Gemini:ApiKey in appsettings.json  OR  set the GEMINI_API_KEY environment variable.
///   3. Set Features:EnableLlmRecipes = true in appsettings.json.
/// </summary>
public class LlmRecipeService : ILlmRecipeService
{
    private const string GeminiBaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly HttpClient _httpClient;
    private readonly IOptions<GeminiOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmRecipeService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LlmRecipeService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        IConfiguration configuration,
        ILogger<LlmRecipeService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _configuration = configuration;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // ─── Public API ─────────────────────────────────────────────────────────

    public bool IsAvailable =>
        _configuration.GetValue<bool>("Features:EnableLlmRecipes") &&
        !string.IsNullOrWhiteSpace(ResolveApiKey());

    public async Task<Recipe?> GenerateRecipeAsync(RecipeGenerationRequest request)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning("GenerateRecipeAsync called but IsAvailable=false (flag off or no API key).");
            return null;
        }

        var apiKey = ResolveApiKey()!;
        var model  = _options.Value.Model;

        _logger.LogInformation(
            "Generating recipe with Gemini. MealType={MealType}, PantryItems={Count}",
            request.MealType ?? "(any)", request.PantryIngredients.Count);

        var prompt = BuildPrompt(request);

        _logger.LogDebug("Gemini prompt:\n{Prompt}", prompt);

        var requestBody = new GeminiRequest
        {
            Contents = new[]
            {
                new GeminiContent
                {
                    Role  = "user",
                    Parts = new[] { new GeminiPart { Text = prompt } }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature      = 0.7,
                MaxOutputTokens  = _options.Value.MaxOutputTokens,
                ResponseMimeType = "application/json"
            }
        };

        var url = $"{GeminiBaseUrl}/{model}:generateContent?key={apiKey}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, requestBody, _jsonOptions);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Gemini API request timed out.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini API network error.");
            return null;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Gemini API rate limit hit (429). Will return null.");
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Gemini API returned non-success status {Status}.", response.StatusCode);
            return null;
        }

        string rawJson;
        try
        {
            rawJson = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Gemini response body.");
            return null;
        }

        // Extract the recipe JSON from the Gemini response envelope
        string? recipeJson = ExtractTextFromGeminiResponse(rawJson);
        if (recipeJson is null)
        {
            _logger.LogWarning("Could not extract text from Gemini response. Raw: {Raw}", rawJson);
            return null;
        }

        return ParseAndValidateRecipe(recipeJson);
    }

    // ─── Prompt construction ────────────────────────────────────────────────

    private static string BuildPrompt(RecipeGenerationRequest req)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a recipe generator. Respond with ONLY a single JSON object — no markdown fences, no explanation, no extra text.");
        sb.AppendLine();
        sb.AppendLine("The JSON object must follow this exact schema:");
        sb.AppendLine("""
{
  "name": "Recipe Name",
  "description": "One sentence description.",
  "mealTypes": ["lunch"],
  "prepTimeMinutes": 25,
  "servings": 2,
  "caloriesPerServing": 400,
  "proteinPerServing": 30,
  "carbsPerServing": 40,
  "fatPerServing": 12,
  "ingredients": [
    { "name": "chicken breast", "quantity": "1 lb", "category": "Protein" }
  ],
  "instructions": [
    "Step 1: Do something.",
    "Step 2: Do something else."
  ]
}
""");

        sb.AppendLine("Rules:");
        sb.AppendLine("- ingredient 'name' must be lowercase and singular (e.g., 'egg', not 'Eggs').");
        sb.AppendLine("- ingredient 'category' must be exactly one of: Produce, Protein, Dairy, Grains, PantryStaples, Frozen, Other.");
        sb.AppendLine("- 'mealTypes' must be an array of lowercase strings from: breakfast, lunch, dinner, snack.");
        sb.AppendLine("- Macros must be realistic: calories ≈ (protein × 4) + (carbs × 4) + (fat × 9), within 10% tolerance.");
        sb.AppendLine("- All numeric values must be positive integers (except servings which must be ≥ 1).");
        sb.AppendLine("- Provide at least 3 ingredients and at least 2 instruction steps.");
        sb.AppendLine();

        if (req.DailyCalorieTarget > 0)
        {
            sb.AppendLine($"The user targets approximately {req.DailyCalorieTarget} calories per day, " +
                          $"{req.ProteinTargetGrams}g protein, {req.CarbsTargetGrams}g carbs, {req.FatTargetGrams}g fat. " +
                          "Design a meal that fits as a reasonable portion of those daily targets.");
        }

        if (!string.IsNullOrWhiteSpace(req.MealType))
        {
            sb.AppendLine($"This recipe should be suitable for: {req.MealType}. Set 'mealTypes' accordingly.");
        }

        if (req.MaxPrepTimeMinutes.HasValue)
        {
            sb.AppendLine($"Prep time must not exceed {req.MaxPrepTimeMinutes} minutes.");
        }

        if (req.PantryIngredients.Count > 0)
        {
            var pantryList = string.Join(", ", req.PantryIngredients);
            sb.AppendLine($"The user currently has these ingredients: {pantryList}.");
            sb.AppendLine($"Strongly prefer using these pantry ingredients (preference weight: {req.PantryPreference:P0}).");
            sb.AppendLine("You may include additional ingredients that are not in the pantry if needed to complete the recipe.");
        }
        else
        {
            sb.AppendLine("The user's pantry is empty — suggest a recipe with common, easy-to-find ingredients.");
        }

        sb.AppendLine();
        sb.AppendLine("Now generate one complete recipe as a single JSON object.");

        return sb.ToString();
    }

    // ─── Response parsing ───────────────────────────────────────────────────

    private string? ExtractTextFromGeminiResponse(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            // candidates[0].content.parts[0].text
            if (!root.TryGetProperty("candidates", out var candidates)) return null;
            if (candidates.GetArrayLength() == 0) return null;

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content)) return null;
            if (!content.TryGetProperty("parts", out var parts)) return null;
            if (parts.GetArrayLength() == 0) return null;

            var firstPart = parts[0];
            if (!firstPart.TryGetProperty("text", out var text)) return null;

            return text.GetString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini response envelope.");
            return null;
        }
    }

    private Recipe? ParseAndValidateRecipe(string recipeJson)
    {
        GeminiRecipeDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<GeminiRecipeDto>(recipeJson, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize recipe JSON. Raw: {Raw}", recipeJson);
            return null;
        }

        if (dto is null)
        {
            _logger.LogWarning("Deserialized recipe DTO is null.");
            return null;
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            _logger.LogWarning("Generated recipe has an empty name — discarding.");
            return null;
        }

        if (dto.Ingredients is null || dto.Ingredients.Count == 0)
        {
            _logger.LogWarning("Generated recipe '{Name}' has no ingredients — discarding.", dto.Name);
            return null;
        }

        if (dto.Instructions is null || dto.Instructions.Count == 0)
        {
            _logger.LogWarning("Generated recipe '{Name}' has no instructions — discarding.", dto.Name);
            return null;
        }

        if (dto.CaloriesPerServing <= 0)
        {
            _logger.LogWarning("Generated recipe '{Name}' has non-positive calories — discarding.", dto.Name);
            return null;
        }

        if (dto.Servings <= 0)
        {
            _logger.LogWarning("Generated recipe '{Name}' has non-positive servings — discarding.", dto.Name);
            return null;
        }

        if (dto.ProteinPerServing < 0 || dto.CarbsPerServing < 0 || dto.FatPerServing < 0)
        {
            _logger.LogWarning("Generated recipe '{Name}' has negative macros — discarding.", dto.Name);
            return null;
        }

        // Map DTO to Recipe model
        var recipe = new Recipe
        {
            Id                 = -1, // Temporary — distinguishes from persisted seed recipes
            Name               = dto.Name.Trim(),
            Description        = dto.Description?.Trim() ?? "",
            MealTypes          = dto.MealTypes is { Count: > 0 }
                                     ? string.Join(",", dto.MealTypes.Select(m => m.Trim().ToLower()))
                                     : "dinner",
            PrepTimeMinutes    = dto.PrepTimeMinutes > 0 ? dto.PrepTimeMinutes : 30,
            DefaultServings    = dto.Servings > 0 ? dto.Servings : 2,
            CaloriesPerServing = dto.CaloriesPerServing,
            ProteinPerServing  = (decimal)dto.ProteinPerServing,
            CarbsPerServing    = (decimal)dto.CarbsPerServing,
            FatPerServing      = (decimal)dto.FatPerServing,
            Instructions       = string.Join("\n", dto.Instructions),
            IsUserCreated      = true,
            Ingredients        = dto.Ingredients
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => new RecipeIngredient
                {
                    Name     = i.Name.Trim().ToLower(),
                    Quantity = i.Quantity?.Trim() ?? "to taste",
                    Category = NormalizeCategory(i.Category)
                })
                .ToList()
        };

        _logger.LogInformation("Recipe generated successfully: {Name}", recipe.Name);
        return recipe;
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private string ResolveApiKey()
    {
        var key = _options.Value.ApiKey;
        if (!string.IsNullOrWhiteSpace(key)) return key;
        return Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
    }

    private static string NormalizeCategory(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Other";

        // Handle "Pantry Staples" (with space) coming from LLM
        var normalized = raw.Trim().Replace(" ", "");

        return normalized switch
        {
            "Produce"       => "Produce",
            "Protein"       => "Protein",
            "Dairy"         => "Dairy",
            "Grains"        => "Grains",
            "PantryStaples" => "PantryStaples",
            "Frozen"        => "Frozen",
            _               => "Other"
        };
    }

    // ─── Gemini API DTOs ─────────────────────────────────────────────────────

    private sealed class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig GenerationConfig { get; set; } = new();
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 4096;

        [JsonPropertyName("responseMimeType")]
        public string ResponseMimeType { get; set; } = "application/json";
    }

    // ─── Recipe response DTO ─────────────────────────────────────────────────

    private sealed class GeminiRecipeDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("mealTypes")]
        public List<string>? MealTypes { get; set; }

        [JsonPropertyName("prepTimeMinutes")]
        public int PrepTimeMinutes { get; set; }

        [JsonPropertyName("servings")]
        public int Servings { get; set; }

        [JsonPropertyName("caloriesPerServing")]
        public int CaloriesPerServing { get; set; }

        [JsonPropertyName("proteinPerServing")]
        public double ProteinPerServing { get; set; }

        [JsonPropertyName("carbsPerServing")]
        public double CarbsPerServing { get; set; }

        [JsonPropertyName("fatPerServing")]
        public double FatPerServing { get; set; }

        [JsonPropertyName("ingredients")]
        public List<GeminiIngredientDto>? Ingredients { get; set; }

        [JsonPropertyName("instructions")]
        public List<string>? Instructions { get; set; }
    }

    private sealed class GeminiIngredientDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }
}
