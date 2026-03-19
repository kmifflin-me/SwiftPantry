using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public interface IProfileService
{
    /// <summary>Returns the single user profile, or null if none exists.</summary>
    Task<UserProfile?> GetProfileAsync();

    /// <summary>Returns true if a UserProfile row exists in the database.</summary>
    Task<bool> ProfileExistsAsync();

    /// <summary>
    /// Creates a new UserProfile. Calculates and stores macro targets via IMacroCalculatorService.
    /// Throws InvalidOperationException if a profile already exists.
    /// </summary>
    Task<UserProfile> CreateProfileAsync(UserProfile profile);

    /// <summary>
    /// Updates the existing profile. Recalculates and stores macro targets.
    /// Throws InvalidOperationException if no profile exists.
    /// </summary>
    Task<UserProfile> UpdateProfileAsync(UserProfile profile);
}
