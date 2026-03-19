using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public interface IShoppingListService
{
    /// <summary>Returns all shopping list items.</summary>
    Task<List<ShoppingListItem>> GetAllItemsAsync();

    /// <summary>Adds a new item. Duplicates by name are allowed. Sets AddedAt = UtcNow.</summary>
    Task<ShoppingListItem> AddItemAsync(ShoppingListItem item);

    /// <summary>Permanently deletes a shopping list item by ID.</summary>
    Task DeleteItemAsync(int id);

    /// <summary>Sets IsPurchased = true for the item with the given ID.</summary>
    Task MarkPurchasedAsync(int id);

    /// <summary>Sets IsPurchased = false for the item with the given ID.</summary>
    Task UnmarkPurchasedAsync(int id);

    /// <summary>Bulk-deletes all items where IsPurchased = true.</summary>
    Task DeleteAllPurchasedAsync();

    /// <summary>
    /// For each RecipeIngredient not present in pantryNamesLower (case-insensitive):
    ///   - Scales quantity by (requestedServings / recipe.DefaultServings)
    ///   - Adds a ShoppingListItem with Category = "Other"
    /// Returns the count of items added.
    /// Scaling rule: extract leading decimal from quantity, multiply, round to 2dp, reattach unit.
    /// If no leading decimal found (e.g., "to taste", "1/2 cup"), quantity is left unchanged.
    /// </summary>
    Task<int> AddMissingIngredientsAsync(Recipe recipe, List<string> pantryNamesLower,
        decimal requestedServings);

    /// <summary>
    /// Moves a purchased shopping list item to the pantry.
    /// Returns true on success (creates PantryItem, deletes ShoppingListItem).
    /// Returns false if a pantry item with the same name already exists (case-insensitive); no data changed.
    /// </summary>
    Task<bool> MoveToPantryAsync(int shoppingListItemId);
}
