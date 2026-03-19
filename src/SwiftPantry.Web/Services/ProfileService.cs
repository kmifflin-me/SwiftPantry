using Microsoft.EntityFrameworkCore;
using SwiftPantry.Web.Data;
using SwiftPantry.Web.Models;

namespace SwiftPantry.Web.Services;

public class ProfileService(AppDbContext db, IMacroCalculatorService calculator) : IProfileService
{
    public async Task<UserProfile?> GetProfileAsync()
        => await db.UserProfiles.FirstOrDefaultAsync();

    public async Task<bool> ProfileExistsAsync()
        => await db.UserProfiles.AnyAsync();

    public async Task<UserProfile> CreateProfileAsync(UserProfile profile)
    {
        if (await ProfileExistsAsync())
            throw new InvalidOperationException("A user profile already exists.");

        ApplyCalculatedTargets(profile);
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
    {
        var existing = await db.UserProfiles.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No user profile exists to update.");

        // Update fields
        existing.Age = profile.Age;
        existing.Sex = profile.Sex;
        existing.HeightCm = profile.HeightCm;
        existing.WeightKg = profile.WeightKg;
        existing.HeightUnit = profile.HeightUnit;
        existing.WeightUnit = profile.WeightUnit;
        existing.ActivityLevel = profile.ActivityLevel;
        existing.Goal = profile.Goal;

        ApplyCalculatedTargets(existing);
        await db.SaveChangesAsync();
        return existing;
    }

    private void ApplyCalculatedTargets(UserProfile profile)
    {
        var targets = calculator.Calculate(
            profile.HeightCm, profile.WeightKg, profile.Age,
            profile.Sex, profile.ActivityLevel, profile.Goal);

        profile.Tdee = targets.Tdee;
        profile.CalorieTarget = targets.CalorieTarget;
        profile.ProteinTargetG = targets.ProteinG;
        profile.CarbsTargetG = targets.CarbsG;
        profile.FatTargetG = targets.FatG;
    }
}
