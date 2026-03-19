using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public class PantryService(AppDbContext db) : IPantryService
{
    public async Task<List<PantryItem>> GetAllItemsAsync()
        => await db.PantryItems
            .OrderBy(i => i.Name)
            .ToListAsync();

    public async Task<PantryItem?> GetByIdAsync(int id)
        => await db.PantryItems.FindAsync(id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var trimmed = name.Trim();
        return await db.PantryItems
            .Where(i => excludeId == null || i.Id != excludeId)
            .AnyAsync(i => i.Name.ToLower() == trimmed.ToLower());
    }

    public async Task<PantryItem> AddItemAsync(PantryItem item)
    {
        item.Name = item.Name.Trim();
        item.AddedAt = DateTime.UtcNow;
        db.PantryItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<PantryItem> UpdateItemAsync(PantryItem item)
    {
        item.Name = item.Name.Trim();
        db.PantryItems.Update(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await db.PantryItems.FindAsync(id);
        if (item is not null)
        {
            db.PantryItems.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetAllNamesLowercaseAsync()
        => await db.PantryItems
            .Select(i => i.Name.Trim().ToLower())
            .ToListAsync();
}
