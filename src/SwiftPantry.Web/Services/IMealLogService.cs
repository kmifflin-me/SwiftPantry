using SwiftPantry.Web.Models;
using SwiftPantry.Web.ViewModels;

namespace SwiftPantry.Web.Services;

public interface IMealLogService
{
    /// <summary>
    /// Returns all entries whose LoggedAt (converted to server local date) matches the given date.
    /// </summary>
    Task<List<MealLogEntry>> GetEntriesForDateAsync(DateOnly date);

    /// <summary>
    /// Aggregates consumed macros for the date and returns a DailySummary with the user's targets.
    /// </summary>
    Task<DailySummary> GetDailySummaryAsync(DateOnly date, UserProfile profile);

    /// <summary>Creates a new MealLogEntry. Sets LoggedAt = DateTime.UtcNow.</summary>
    Task<MealLogEntry> AddEntryAsync(MealLogEntry entry);

    /// <summary>Permanently deletes a meal log entry by ID.</summary>
    Task DeleteEntryAsync(int id);

    /// <summary>
    /// Deletes all MealLogEntry rows where LoggedAt (UTC) is more than 7 days
    /// before the current server date. Called once on application startup.
    /// </summary>
    Task CleanupOldEntriesAsync();
}
