using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SwiftPantry.Web.Pages;

/// <summary>Root redirect: / → /MealLog</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/MealLog");
}
