using System.Text.Json;
using System.Text.Json.Serialization;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Data;

/// <summary>
/// Reads seed_recipes.json and maps it to Recipe + RecipeIngredient entities.
/// Called by RecipeService.SeedRecipesIfEmptyAsync().
/// </summary>
public static class SeedData
{
    public static List<Recipe> LoadSeedRecipes(string jsonFilePath)
    {
        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dtos = JsonSerializer.Deserialize<List<RecipeSeedDto>>(json, options)
                   ?? throw new InvalidOperationException("Failed to deserialize seed_recipes.json");

        return dtos.Select(dto => new Recipe
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            MealTypes = string.Join(",", dto.MealTypesArray),
            PrepTimeMinutes = dto.PrepTimeMinutes,
            DefaultServings = dto.DefaultServings,
            CaloriesPerServing = dto.CaloriesPerServing,
            ProteinPerServing = dto.ProteinPerServing,
            CarbsPerServing = dto.CarbsPerServing,
            FatPerServing = dto.FatPerServing,
            Instructions = string.Join("\n", dto.Instructions),
            IsUserCreated = false,
            Ingredients = dto.Ingredients.Select(i => new RecipeIngredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                // "Pantry Staples" → "PantryStaples" (strip spaces for enum name storage)
                Category = i.Category.Replace(" ", "")
            }).ToList()
        }).ToList();
    }

    // ─── Internal DTOs for JSON deserialization ─────────────────────────────

    private class RecipeSeedDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        [JsonPropertyName("mealTypes")]
        public List<string> MealTypesArray { get; set; } = new();

        public int PrepTimeMinutes { get; set; }

        [JsonPropertyName("servings")]
        public int DefaultServings { get; set; }

        public int CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; }
        public decimal CarbsPerServing { get; set; }
        public decimal FatPerServing { get; set; }
        public List<string> Instructions { get; set; } = new();
        public List<IngredientSeedDto> Ingredients { get; set; } = new();
    }

    private class IngredientSeedDto
    {
        public string Name { get; set; } = "";
        public string Quantity { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
