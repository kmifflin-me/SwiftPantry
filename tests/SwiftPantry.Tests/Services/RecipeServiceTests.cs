using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Tests.Services;

/// <summary>
/// Unit tests for RecipeService.
/// Covers: CalculateOwnershipPct and GetDefaultMealType.
/// </summary>
[TestFixture]
public class RecipeServiceTests
{
    private AppDbContext _db = null!;
    private RecipeService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"RecipeServiceTests_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);

        _sut = new RecipeService(_db, envMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── CalculateOwnershipPct ────────────────────────────────────────────

    /// <summary>UT-021: 4 of 8 owned → floor(4/8 × 100) = 50%</summary>
    [Test]
    public void CalculateOwnershipPct_FourOfEight_Returns50()
    {
        var recipe = BuildRecipe(["chicken breast", "olive oil", "mixed green",
            "cherry tomato", "cucumber", "red wine vinegar", "black pepper", "salt"]);
        var pantry = new List<string> { "chicken breast", "olive oil", "salt", "black pepper" };

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(50));
    }

    /// <summary>UT-022: Case-insensitive match — "Chicken Breast" in recipe vs "chicken breast" in pantry.</summary>
    [Test]
    public void CalculateOwnershipPct_CaseInsensitive_CountsAsMatch()
    {
        var recipe = BuildRecipe(["Chicken Breast"]);
        var pantry = new List<string> { "chicken breast" };

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(100));
    }

    /// <summary>UT-023: Zero ingredients → 0%</summary>
    [Test]
    public void CalculateOwnershipPct_ZeroIngredients_Returns0()
    {
        var recipe = new Recipe { Id = 1, Ingredients = new List<RecipeIngredient>() };
        var pantry = new List<string> { "chicken breast" };

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(0));
    }

    /// <summary>UT-024: All owned → 100%</summary>
    [Test]
    public void CalculateOwnershipPct_AllOwned_Returns100()
    {
        var recipe = BuildRecipe(["egg", "whole wheat bread", "butter", "milk", "salt", "black pepper"]);
        var pantry = new List<string> { "egg", "whole wheat bread", "butter", "milk", "salt", "black pepper" };

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(100));
    }

    /// <summary>UT-025: None owned → 0%</summary>
    [Test]
    public void CalculateOwnershipPct_NoneOwned_Returns0()
    {
        var recipe = BuildRecipe(["chicken breast", "olive oil", "salt"]);
        var pantry = new List<string>();

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(0));
    }

    /// <summary>2 of 3 owned → floor(66.6) = 66%</summary>
    [Test]
    public void CalculateOwnershipPct_TwoOfThree_Returns66()
    {
        var recipe = BuildRecipe(["a", "b", "c"]);
        var pantry = new List<string> { "a", "b" };

        var result = _sut.CalculateOwnershipPct(recipe, pantry);

        Assert.That(result, Is.EqualTo(66));
    }

    // ─── GetDefaultMealType ───────────────────────────────────────────────

    /// <summary>UT-026: 9:59 AM → Breakfast</summary>
    [Test]
    public void GetDefaultMealType_Before10Am_ReturnsBreakfast()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(9, 59));
        Assert.That(result, Is.EqualTo("breakfast"));
    }

    /// <summary>UT-027: 10:00 AM → Lunch</summary>
    [Test]
    public void GetDefaultMealType_At10Am_ReturnsLunch()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(10, 0));
        Assert.That(result, Is.EqualTo("lunch"));
    }

    /// <summary>UT-028: 1:59 PM → Lunch</summary>
    [Test]
    public void GetDefaultMealType_At1359_ReturnsLunch()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(13, 59));
        Assert.That(result, Is.EqualTo("lunch"));
    }

    /// <summary>UT-029: 2:00 PM → Snack</summary>
    [Test]
    public void GetDefaultMealType_At1400_ReturnsSnack()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(14, 0));
        Assert.That(result, Is.EqualTo("snack"));
    }

    /// <summary>UT-030: 4:59 PM → Snack</summary>
    [Test]
    public void GetDefaultMealType_At1659_ReturnsSnack()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(16, 59));
        Assert.That(result, Is.EqualTo("snack"));
    }

    /// <summary>UT-031: 5:00 PM → Dinner</summary>
    [Test]
    public void GetDefaultMealType_At1700_ReturnsDinner()
    {
        var result = _sut.GetDefaultMealType(new TimeOnly(17, 0));
        Assert.That(result, Is.EqualTo("dinner"));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static Recipe BuildRecipe(IEnumerable<string> ingredientNames)
    {
        var ingredients = ingredientNames
            .Select(n => new RecipeIngredient { Name = n, Quantity = "1 unit" })
            .ToList();
        return new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            MealTypes = "lunch",
            Ingredients = ingredients
        };
    }
}
