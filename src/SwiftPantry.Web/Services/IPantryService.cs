using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public interface IPantryService
{
    /// <summary>Returns all pantry items ordered by Category (fixed CategoryOrder), then Name ascending.</summary>
    Task<List<PantryItem>> GetAllItemsAsync();

    /// <summary>Returns a single pantry item by ID, or null if not found.</summary>
    Task<PantryItem?> GetByIdAsync(int id);

    /// <summary>
    /// Returns true if any pantry item has the given name (case-insensitive, trimmed).
    /// excludeId: when editing, exclude the current item from the uniqueness check.
    /// </summary>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>Adds a new pantry item. Sets AddedAt = DateTime.UtcNow. Caller must check uniqueness first.</summary>
    Task<PantryItem> AddItemAsync(PantryItem item);

    /// <summary>Updates an existing pantry item. Caller must check uniqueness first.</summary>
    Task<PantryItem> UpdateItemAsync(PantryItem item);

    /// <summary>Permanently deletes a pantry item by ID.</summary>
    Task DeleteItemAsync(int id);

    /// <summary>Returns all pantry item names as lowercase trimmed strings for recipe matching.</summary>
    Task<List<string>> GetAllNamesLowercaseAsync();
}
