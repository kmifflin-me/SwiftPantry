using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Tests.Services;

/// <summary>
/// Unit tests for RecipeService.
/// Covers: FilterRecipesAsync (meal type + ingredient filters),
/// CalculateOwnershipPct, ScaleQuantity logic via ShoppingListService.
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
        // ContentRootPath pointing to where seed_recipes.json lives at test time
        envMock.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);

        _sut = new RecipeService(_db, envMock.Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── FilterRecipesAsync ────────────────────────────────────────────────

    [Test]
    public async Task FilterRecipes_NoFilter_ReturnsAll()
    {
        // TODO: Seed recipes into in-memory DB, call FilterRecipesAsync(new RecipeFilter()),
        // assert count == seeded count
        Assert.Inconclusive("TODO: Implement");
    }

    [Test]
    public async Task FilterRecipes_ByMealType_ReturnsOnlyMatchingRecipes()
    {
        // TODO: Seed mixed meal-type recipes, filter by "breakfast", assert only breakfast returned
        Assert.Inconclusive("TODO: Implement");
    }

    [Test]
    public async Task FilterRecipes_ByIngredient_ReturnsOnlyMatchingRecipes()
    {
        // TODO: Seed recipes with/without "chicken", filter by ingredient "chicken",
        // assert only chicken recipes returned
        Assert.Inconclusive("TODO: Implement");
    }

    // ─── CalculateOwnershipPct ─────────────────────────────────────────────

    [Test]
    public void CalculateOwnershipPct_AllOwned_Returns100()
    {
        // TODO: Build recipe with 3 ingredients all in pantry, assert 100
        Assert.Inconclusive("TODO: Implement");
    }

    [Test]
    public void CalculateOwnershipPct_NoneOwned_Returns0()
    {
        // TODO: Build recipe with 3 ingredients none in pantry, assert 0
        Assert.Inconclusive("TODO: Implement");
    }

    [Test]
    public void CalculateOwnershipPct_PartialOwned_ReturnsFloor()
    {
        // TODO: 2/3 owned → floor(66.6) = 66
        Assert.Inconclusive("TODO: Implement");
    }

    [Test]
    public void CalculateOwnershipPct_CaseInsensitive_MatchesPantryItem()
    {
        // TODO: pantry has "Chicken Breast", recipe needs "chicken breast" → should match
        Assert.Inconclusive("TODO: Implement");
    }
}
