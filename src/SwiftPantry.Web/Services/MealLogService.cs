using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public class MealLogService(AppDbContext db) : IMealLogService
{
    public async Task<List<MealLogEntry>> GetEntriesForDateAsync(DateOnly date)
    {
        // Convert UTC stored times to server local date for comparison
        return await db.MealLogEntries
            .Where(e => DateOnly.FromDateTime(e.LoggedAt.ToLocalTime()) == date)
            .OrderBy(e => e.LoggedAt)
            .ToListAsync();
    }

    public async Task<DailySummary> GetDailySummaryAsync(DateOnly date, UserProfile profile)
    {
        var entries = await GetEntriesForDateAsync(date);

        var caloriesConsumed = entries.Sum(e => e.TotalCalories);
        var proteinConsumed  = entries.Sum(e => e.TotalProteinG);
        var carbsConsumed    = entries.Sum(e => e.TotalCarbsG);
        var fatConsumed      = entries.Sum(e => e.TotalFatG);

        return new DailySummary(
            caloriesConsumed, profile.CalorieTarget,
            proteinConsumed,  profile.ProteinTargetG,
            carbsConsumed,    profile.CarbsTargetG,
            fatConsumed,      profile.FatTargetG);
    }

    public async Task<MealLogEntry> AddEntryAsync(MealLogEntry entry)
    {
        entry.LoggedAt = DateTime.UtcNow;
        db.MealLogEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteEntryAsync(int id)
    {
        var entry = await db.MealLogEntries.FindAsync(id);
        if (entry is not null)
        {
            db.MealLogEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }

    public async Task CleanupOldEntriesAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var old = await db.MealLogEntries
            .Where(e => e.LoggedAt < cutoff)
            .ToListAsync();

        if (old.Count > 0)
        {
            db.MealLogEntries.RemoveRange(old);
            await db.SaveChangesAsync();
        }
    }
}
