using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Pages.Profile;

public class EditModel : PageModel
{
    private readonly IProfileService _profileService;

    public EditModel(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Age is required.")]
    [Range(10, 120, ErrorMessage = "Age must be between 10 and 120.")]
    public int Age { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select a sex.")]
    public string Sex { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Height is required.")]
    public decimal Height { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select a unit.")]
    public string HeightUnit { get; set; } = "in";

    [BindProperty]
    [Required(ErrorMessage = "Weight is required.")]
    public decimal Weight { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select a unit.")]
    public string WeightUnit { get; set; } = "lbs";

    [BindProperty]
    [Required(ErrorMessage = "Please select an activity level.")]
    public string ActivityLevel { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Please select a goal.")]
    public string Goal { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _profileService.GetProfileAsync();
        if (profile is null)
            return RedirectToPage("/Profile/Setup");

        Age           = profile.Age;
        Sex           = profile.Sex;
        Height        = profile.DisplayHeight;
        HeightUnit    = profile.HeightUnit;
        Weight        = profile.DisplayWeight;
        WeightUnit    = profile.WeightUnit;
        ActivityLevel = profile.ActivityLevel;
        Goal          = profile.Goal;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateHeightWeight();

        if (!ModelState.IsValid)
            return Page();

        decimal heightCm = HeightUnit == "in" ? Height * 2.54m : Height;
        decimal weightKg = WeightUnit == "lbs" ? Weight / 2.20462m : Weight;

        var updated = new UserProfile
        {
            Age           = Age,
            Sex           = Sex,
            HeightCm      = Math.Round(heightCm, 2),
            WeightKg      = Math.Round(weightKg, 2),
            HeightUnit    = HeightUnit,
            WeightUnit    = WeightUnit,
            ActivityLevel = ActivityLevel,
            Goal          = Goal
        };

        await _profileService.UpdateProfileAsync(updated);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToPage("/Profile/Index");
    }

    private void ValidateHeightWeight()
    {
        if (HeightUnit == "in")
        {
            if (Height < 24 || Height > 120)
                ModelState.AddModelError(nameof(Height), "Height must be between 24–120 in.");
        }
        else
        {
            if (Height < 61 || Height > 305)
                ModelState.AddModelError(nameof(Height), "Height must be between 61–305 cm.");
        }

        if (WeightUnit == "lbs")
        {
            if (Weight < 50 || Weight > 1000)
                ModelState.AddModelError(nameof(Weight), "Weight must be between 50–1000 lbs.");
        }
        else
        {
            if (Weight < 23 || Weight > 454)
                ModelState.AddModelError(nameof(Weight), "Weight must be between 23–454 kg.");
        }

        if (string.IsNullOrEmpty(Sex))
            ModelState.AddModelError(nameof(Sex), "Please select a sex.");
        if (string.IsNullOrEmpty(ActivityLevel))
            ModelState.AddModelError(nameof(ActivityLevel), "Please select an activity level.");
        if (string.IsNullOrEmpty(Goal))
            ModelState.AddModelError(nameof(Goal), "Please select a goal.");
    }
}
