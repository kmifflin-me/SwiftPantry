using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public class ShoppingListService(AppDbContext db, IPantryService pantryService) : IShoppingListService
{
    public async Task<List<ShoppingListItem>> GetAllItemsAsync()
        => await db.ShoppingListItems
            .OrderBy(i => i.Name)
            .ToListAsync();

    public async Task<ShoppingListItem> AddItemAsync(ShoppingListItem item)
    {
        item.AddedAt = DateTime.UtcNow;
        db.ShoppingListItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await db.ShoppingListItems.FindAsync(id);
        if (item is not null)
        {
            db.ShoppingListItems.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public async Task MarkPurchasedAsync(int id)
    {
        var item = await db.ShoppingListItems.FindAsync(id);
        if (item is not null)
        {
            item.IsPurchased = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task UnmarkPurchasedAsync(int id)
    {
        var item = await db.ShoppingListItems.FindAsync(id);
        if (item is not null)
        {
            item.IsPurchased = false;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAllPurchasedAsync()
    {
        var purchased = await db.ShoppingListItems
            .Where(i => i.IsPurchased)
            .ToListAsync();

        if (purchased.Count > 0)
        {
            db.ShoppingListItems.RemoveRange(purchased);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> AddMissingIngredientsAsync(Recipe recipe, List<string> pantryNamesLower,
        decimal requestedServings)
    {
        var missing = recipe.Ingredients
            .Where(i => !pantryNamesLower.Contains(i.Name.Trim().ToLower()))
            .ToList();

        foreach (var ingredient in missing)
        {
            var scaledQty = ScaleQuantity(ingredient.Quantity, recipe.DefaultServings, requestedServings);
            db.ShoppingListItems.Add(new ShoppingListItem
            {
                Name     = ingredient.Name,
                Quantity = scaledQty,
                Category = "Other",
                AddedAt  = DateTime.UtcNow
            });
        }

        if (missing.Count > 0)
            await db.SaveChangesAsync();

        return missing.Count;
    }

    public async Task<bool> MoveToPantryAsync(int shoppingListItemId)
    {
        var item = await db.ShoppingListItems.FindAsync(shoppingListItemId);
        if (item is null) return false;

        if (await pantryService.NameExistsAsync(item.Name))
            return false;

        db.PantryItems.Add(new PantryItem
        {
            Name     = item.Name,
            Quantity = item.Quantity,
            Category = item.Category,
            AddedAt  = DateTime.UtcNow
        });
        db.ShoppingListItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Scales a quantity string by (requestedServings / defaultServings).
    /// Extracts leading decimal number, multiplies, rounds to 2dp, reattaches unit.
    /// If no leading decimal found (e.g., "to taste", "1/2 cup"), returns unchanged.
    /// </summary>
    private static string ScaleQuantity(string quantity, int defaultServings, decimal requestedServings)
    {
        if (defaultServings <= 0) return quantity;

        var match = Regex.Match(quantity.Trim(), @"^(\d+\.?\d*)\s+(.+)$");
        if (!match.Success) return quantity;

        if (!decimal.TryParse(match.Groups[1].Value, out var num)) return quantity;

        var factor = requestedServings / defaultServings;
        var scaled = Math.Round(num * factor, 2);
        var unit = match.Groups[2].Value.Trim();

        return unit.Length > 0 ? $"{scaled:F2} {unit}" : $"{scaled:F2}";
    }
}
