namespace SwiftPantry.PlaywrightTests.Helpers;

/// <summary>Shared helper utilities for Playwright tests.</summary>
public static class TestHelpers
{
    /// <summary>
    /// Waits for a flash success message containing the given text.
    /// Assumes the layout renders TempData["Success"] in an alert element.
    /// </summary>
    public static async Task WaitForSuccessMessageAsync(IPage page, string? containsText = null)
    {
        var locator = page.Locator(".alert-success");
        await locator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        if (containsText is not null)
        {
            var text = await locator.InnerTextAsync();
            if (!text.Contains(containsText, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Expected success message to contain '{containsText}' but got '{text}'");
        }
    }
}
