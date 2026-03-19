using Microsoft.EntityFrameworkCore;
using Moq;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Tests.Services;

/// <summary>
/// Unit tests for ShoppingListService.
/// Covers: AddMissingIngredientsAsync diff logic, quantity scaling, duplicate shopping list items.
/// Reference: TEST_PLAN.md UT-036 through UT-044.
/// </summary>
[TestFixture]
public class ShoppingListServiceTests
{
    private AppDbContext _db = null!;
    private ShoppingListService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ShoppingListTests_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);

        var pantryService = new PantryService(_db);
        _sut = new ShoppingListService(_db, pantryService);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── AddMissingIngredientsAsync ────────────────────────────────────────

    /// <summary>UT-036: 4 ingredients, pantry owns 2 → adds 2</summary>
    [Test]
    public async Task AddMissing_PartialPantry_AddsOnlyMissing()
    {
        var recipe = BuildRecipe(3, [
            ("A", "1 cup"), ("B", "2 cups"), ("C", "1 tbsp"), ("D", "1 lb")
        ]);
        var pantry = new List<string> { "a", "c" }; // owns A and C (lowercase)

        var count = await _sut.AddMissingIngredientsAsync(recipe, pantry, 3);

        Assert.That(count, Is.EqualTo(2));
        var items = await _db.ShoppingListItems.ToListAsync();
        Assert.That(items.Count, Is.EqualTo(2));
        Assert.That(items.Select(i => i.Name), Is.EquivalentTo(new[] { "B", "D" }));
    }

    /// <summary>UT-037: All ingredients in pantry → adds 0</summary>
    [Test]
    public async Task AddMissing_AllInPantry_ReturnsZero()
    {
        var recipe = BuildRecipe(2, [("A", "1 cup"), ("B", "2 cups")]);
        var pantry = new List<string> { "a", "b" };

        var count = await _sut.AddMissingIngredientsAsync(recipe, pantry, 2);

        Assert.That(count, Is.EqualTo(0));
        Assert.That(await _db.ShoppingListItems.CountAsync(), Is.EqualTo(0));
    }

    /// <summary>UT-038: Case-insensitive pantry check</summary>
    [Test]
    public async Task AddMissing_CaseInsensitivePantryCheck_DoesNotAddOwned()
    {
        var recipe = BuildRecipe(1, [("Chicken Breast", "6 oz")]);
        var pantry = new List<string> { "chicken breast" }; // lowercase

        var count = await _sut.AddMissingIngredientsAsync(recipe, pantry, 1);

        Assert.That(count, Is.EqualTo(0));
    }

    /// <summary>UT-039: Quantity scaling — "1 lb", default=3, requested=6 → "2.00 lb"</summary>
    [Test]
    public async Task AddMissing_ScalesQuantityNumerically()
    {
        var recipe = BuildRecipe(3, [("chicken breast", "1 lb")]);
        var pantry = new List<string>(); // nothing in pantry

        await _sut.AddMissingIngredientsAsync(recipe, pantry, 6); // request 6 of default 3

        var item = await _db.ShoppingListItems.FirstAsync();
        Assert.That(item.Quantity, Is.EqualTo("2.00 lb"));
    }

    /// <summary>UT-040: "3 tbsp soy sauce", default=3, requested=9 → "9.00 tbsp"</summary>
    [Test]
    public async Task AddMissing_ScalesQuantityMultipleServings()
    {
        var recipe = BuildRecipe(3, [("soy sauce", "3 tbsp")]);
        var pantry = new List<string>();

        await _sut.AddMissingIngredientsAsync(recipe, pantry, 9);

        var item = await _db.ShoppingListItems.FirstAsync();
        Assert.That(item.Quantity, Is.EqualTo("9.00 tbsp"));
    }

    /// <summary>UT-041: "to taste" — no leading numeric → unchanged</summary>
    [Test]
    public async Task AddMissing_ToTasteQuantity_Unchanged()
    {
        var recipe = BuildRecipe(3, [("salt", "to taste")]);
        var pantry = new List<string>();

        await _sut.AddMissingIngredientsAsync(recipe, pantry, 6);

        var item = await _db.ShoppingListItems.FirstAsync();
        Assert.That(item.Quantity, Is.EqualTo("to taste"));
    }

    /// <summary>UT-042: "1/2 cup" — fraction not extractable as decimal → unchanged</summary>
    [Test]
    public async Task AddMissing_FractionQuantity_Unchanged()
    {
        var recipe = BuildRecipe(1, [("oats", "1/2 cup")]);
        var pantry = new List<string>();

        await _sut.AddMissingIngredientsAsync(recipe, pantry, 2);

        var item = await _db.ShoppingListItems.FirstAsync();
        // "1/2" does NOT match ^(\d+\.?\d*)\s*(.*)$ as a simple decimal → returned unchanged
        Assert.That(item.Quantity, Is.EqualTo("1/2 cup"));
    }

    /// <summary>UT-043: Shopping list allows duplicate names</summary>
    [Test]
    public async Task AddItem_DuplicateNames_BothSaved()
    {
        var item1 = new ShoppingListItem { Name = "Chicken Breast", Quantity = "1 lb", Category = "Other" };
        var item2 = new ShoppingListItem { Name = "Chicken Breast", Quantity = "2 lb", Category = "Other" };

        await _sut.AddItemAsync(item1);
        await _sut.AddItemAsync(item2);

        var all = await _db.ShoppingListItems.ToListAsync();
        Assert.That(all.Count, Is.EqualTo(2));
    }

    /// <summary>UT-044: Category added via AddMissing is always "Other"</summary>
    [Test]
    public async Task AddMissing_CategoryAlwaysOther()
    {
        var recipe = BuildRecipe(1, [("spinach", "2 cups")]);
        recipe.Ingredients.First().Category = "Produce"; // ingredient has its own category
        var pantry = new List<string>();

        await _sut.AddMissingIngredientsAsync(recipe, pantry, 1);

        var item = await _db.ShoppingListItems.FirstAsync();
        Assert.That(item.Category, Is.EqualTo("Other"));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static Recipe BuildRecipe(int defaultServings,
        IEnumerable<(string Name, string Quantity)> ingredients)
    {
        return new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            MealTypes = "lunch",
            DefaultServings = defaultServings,
            Ingredients = ingredients
                .Select(i => new RecipeIngredient { Name = i.Name, Quantity = i.Quantity })
                .ToList()
        };
    }
}
