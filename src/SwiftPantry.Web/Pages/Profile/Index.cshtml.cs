using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;

namespace SwiftPantry.Web.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly IProfileService _profileService;

    public IndexModel(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public UserProfile Profile { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _profileService.GetProfileAsync();
        if (profile is null)
            return RedirectToPage("/Profile/Setup");

        Profile = profile;
        return Page();
    }
}
