# LLM Recipe Generation Setup

SwiftPantry can generate personalized recipes using **Google Gemini 2.0 Flash** (free tier).
The feature is fully isolated behind a feature flag — the rest of the app is unaffected when it is off.

---

## Getting a Gemini API Key

1. Go to [https://aistudio.google.com/apikey](https://aistudio.google.com/apikey)
2. Sign in with a Google account.
3. Click **Create API Key**, choose a project, and copy the key.

The free tier gives you:
- **15 requests per minute** (RPM)
- **1,000,000 tokens per minute** (TPM)
- No credit card required

This is more than sufficient for interactive single-user recipe generation.

---

## Configuration

### Option A — appsettings.json (local dev)

Edit `src/SwiftPantry.Web/appsettings.json`:

```json
{
  "Features": {
    "EnableLlmRecipes": true
  },
  "Gemini": {
    "ApiKey": "YOUR_KEY_HERE",
    "Model": "gemini-2.0-flash",
    "MaxOutputTokens": 4096
  }
}
```

> **Security note:** `appsettings.json` is gitignored for local overrides.
> Never commit a real API key to source control.
> Use `appsettings.Development.json` (which is already gitignored) or an environment variable.

### Option B — Environment variable (recommended for CI/deployment)

Set the environment variable before running the app:

```bash
# Linux / macOS
export GEMINI_API_KEY="YOUR_KEY_HERE"

# Windows (PowerShell)
$env:GEMINI_API_KEY = "YOUR_KEY_HERE"

# Docker
docker run -e GEMINI_API_KEY=YOUR_KEY_HERE swiftpantry
```

The service checks `GEMINI_API_KEY` as a fallback when `Gemini:ApiKey` in appsettings is empty.

---

## Feature Flag

The Generate button only renders when **both** conditions are true:
1. `Features:EnableLlmRecipes = true`
2. A non-empty API key is configured (appsettings OR environment variable)

If either condition is false, `ILlmRecipeService.IsAvailable` returns `false` and the button is simply not shown — no errors, no degraded UX.

---

## How It Works

1. User clicks **✨ Generate Recipe** on the Recipes browser page.
2. SwiftPantry sends a structured prompt to Gemini 2.0 Flash including:
   - The user's current pantry ingredients (to maximize "I already have this")
   - The user's daily macro targets (for context-appropriate portion sizing)
   - The active meal-type and max-prep-time filters
3. Gemini responds with pure JSON (using `responseMimeType: "application/json"`).
4. The response is validated and displayed on the **Generated Recipe** page with an **✨ AI Generated** badge.
5. The user can:
   - **Save** — persists the recipe to the database and redirects to the Detail page.
   - **Log** — logs the meal immediately without persisting.
   - **Add Missing to Shopping List** — adds any ingredients not in the pantry.

---

## Rate Limits & Error Handling

| Scenario | Behaviour |
|----------|-----------|
| 429 Too Many Requests | Returns null → "Recipe generation is temporarily unavailable" |
| Network timeout (>30s) | Returns null → "Recipe generation failed. Please try again." |
| Malformed LLM response | Returns null → same error message |
| Invalid macros / empty name | Returns null → same error message |
| Feature off or no key | Button not shown, no error |

---

## Running Tests Without a Key

The unit tests in `SwiftPantry.Tests` mock the `HttpMessageHandler` — no API key is needed.
The Playwright tests in `SwiftPantry.PlaywrightTests` use a `FakeLlmRecipeService` that returns a hardcoded recipe — also no API key needed.
