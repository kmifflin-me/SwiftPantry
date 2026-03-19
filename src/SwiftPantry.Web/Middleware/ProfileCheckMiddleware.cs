using SwiftPantry.Web.Services;

namespace SwiftPantry.Web.Middleware;

/// <summary>
/// Redirects to /Profile/Setup if no UserProfile exists in the database.
/// Skips static files, error pages, and the setup page itself.
/// </summary>
public class ProfileCheckMiddleware(RequestDelegate next)
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/Profile/Setup",
        "/Error",
        "/favicon.ico",
        "/_",
        "/css",
        "/js",
        "/lib",
        "/images"
    ];

    public async Task InvokeAsync(HttpContext context, IProfileService profileService)
    {
        var path = context.Request.Path.Value ?? "";

        bool isExcluded = ExcludedPrefixes.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (!isExcluded && !await profileService.ProfileExistsAsync())
        {
            context.Response.Redirect("/Profile/Setup");
            return;
        }

        await next(context);
    }
}
