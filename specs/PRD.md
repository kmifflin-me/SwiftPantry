# Product Requirements Document — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved

---

## 1. Product Overview

SwiftPantry is a single-user ASP.NET Razor Pages web application for personal meal prep planning and macro nutrition tracking. It allows a single user per deployment to maintain a health profile, browse and filter recipes, manage a pantry inventory, build a shopping list, and log daily meals against calculated calorie and macro targets. All data is persisted locally in SQLite; no cloud services, accounts, or authentication are required.

---

## 2. Target User Persona

**Alex, the health-conscious meal prepper** — Alex is a busy professional in their late 20s to early 40s who tracks macros and preps meals in advance. Alex knows their fitness goal (cutting, maintaining, or bulking) and wants a no-friction local tool to plan the week's meals, check what's in the fridge, and stay on top of calorie targets without relying on a subscription app. Alex is comfortable using a web browser on both desktop and mobile, expects forms to validate immediately, and wants the app to do the math for them (calories, macros, scaling).

---

## 3. Feature Requirements

### 3.1 User Health Profile

**FR-1.1** The application shall have a single User Health Profile stored in the `UserProfile` table in SQLite. If no profile exists on first launch, the user is redirected to the `/Profile/Setup` page before accessing any other page.

**FR-1.2** The profile form shall collect the following fields with the specified types and validation rules:

| Field | Type | Validation |
|---|---|---|
| Age | Integer | 10–120, required |
| Sex | Enum: Male / Female | Required |
| Height | Decimal | Required; displayed and stored in user-selected unit (inches or centimeters); range 24–120 in / 61–305 cm |
| Weight | Decimal | Required; displayed and stored in user-selected unit (lbs or kg); range 50–1000 lbs / 23–454 kg |
| Activity Level | Enum | Required; values: Sedentary, Lightly Active, Moderately Active, Very Active, Extra Active |
| Goal | Enum | Required; values: Lose Weight, Maintain, Gain Weight |
| Height Unit | Enum: in / cm | Required; drives height field label and validation |
| Weight Unit | Enum: lbs / kg | Required; drives weight field label and validation |

**FR-1.3** Daily calorie target shall be calculated server-side using the **Mifflin-St Jeor equation**:

- BMR (Male) = (10 × weight_kg) + (6.25 × height_cm) − (5 × age) + 5
- BMR (Female) = (10 × weight_kg) + (6.25 × height_cm) − (5 × age) − 161
- TDEE = BMR × activity multiplier:
  - Sedentary: 1.2
  - Lightly Active: 1.375
  - Moderately Active: 1.55
  - Very Active: 1.725
  - Extra Active: 1.9

**FR-1.4** Calorie targets shall be adjusted by goal:

| Goal | Calorie Adjustment |
|---|---|
| Lose Weight | TDEE − 500 kcal |
| Maintain | TDEE |
| Gain Weight | TDEE + 300 kcal |

**FR-1.5** Daily macro targets (in grams) shall be derived from the adjusted calorie target using the following percentage splits:

| Goal | Protein | Carbs | Fat |
|---|---|---|---|
| Lose Weight | 40% | 30% | 30% |
| Maintain | 30% | 40% | 30% |
| Gain Weight | 30% | 45% | 25% |

Conversion: protein and carbs = 4 kcal/g; fat = 9 kcal/g. Values rounded to the nearest whole gram.

**FR-1.6** The profile page (`/Profile`) shall display the calculated TDEE, adjusted calorie target, and macro targets (protein g, carbs g, fat g) after saving, alongside the form values for review.

**FR-1.7** The user can edit the profile at any time via `/Profile/Edit`. Saving recalculates targets. All existing meal logs are unaffected by profile changes.

---

### 3.2 Daily Meal Log

**FR-2.1** The Daily Meal Log page (`/MealLog`) displays all meals logged for the **current calendar day** (server local date) by default. A date picker allows viewing any of the last 7 days.

**FR-2.2** Each logged meal entry stores the following fields:

| Field | Type | Notes |
|---|---|---|
| Id | Integer PK | Auto-increment |
| RecipeName | String (200) | Required |
| MealType | Enum | Breakfast / Lunch / Dinner / Snack |
| Servings | Decimal | Required; min 0.25; max 20 |
| CaloriesPerServing | Integer | Required; min 0 |
| ProteinPerServing | Decimal | grams; required; min 0 |
| CarbsPerServing | Decimal | grams; required; min 0 |
| FatPerServing | Decimal | grams; required; min 0 |
| LoggedAt | DateTime | UTC; set on creation; displayed in local time |
| RecipeId | Integer? FK | Nullable; set when logged from a recipe |

**FR-2.3** Logged totals (calories, protein, carbs, fat) shall be calculated as `value_per_serving × servings`, summed across all entries for the selected day.

**FR-2.4** The daily summary section at the top of `/MealLog` shall show four Bootstrap progress bars — one each for calories, protein, carbs, and fat — displaying `consumed / target` with a numeric label (e.g., "1,450 / 2,100 kcal"). Progress bars fill proportionally; they turn red (Bootstrap `bg-danger`) when consumed exceeds the target.

**FR-2.5** A "Quick Log" section shall appear below the summary showing all recipes in the user's Saved Recipes list as cards with a single "Log" button. Clicking "Log" opens a modal with a servings input (default: the recipe's default servings), meal type selector (pre-selected by time of day per FR-5.3), and a confirm button. Submitting the modal creates a new `MealLogEntry`.

**FR-2.6** A manual log form allows logging a meal without selecting a saved recipe. Fields: Recipe Name (text), Meal Type (select), Servings (number), Calories (number), Protein (number), Carbs (number), Fat (number). All fields are required. All numeric fields must be ≥ 0.

**FR-2.7** Each meal log entry in the list shows: recipe name, meal type badge, servings, total calories for that entry, time logged, and a Delete button. Deletion is confirmed via a JavaScript `confirm()` dialog before posting. There is no edit action — delete and re-log.

**FR-2.8** The app retains meal log entries for 7 days. Entries older than 7 days from the current server date are automatically deleted on application startup (via a startup service, not a background job).

---

### 3.3 Ingredients Inventory (My Pantry)

**FR-3.1** The Pantry page (`/Pantry`) displays all pantry ingredients grouped by category, with each group sorted alphabetically by ingredient name.

**FR-3.2** Each pantry ingredient stores:

| Field | Type | Validation |
|---|---|---|
| Id | Integer PK | Auto-increment |
| Name | String (200) | Required; unique (case-insensitive) within pantry |
| Quantity | String (100) | Required; free text (e.g., "2 lbs", "1 bunch") |
| Category | Enum | Required |

**FR-3.3** Valid categories (fixed, not user-configurable): `Produce`, `Protein`, `Dairy`, `Grains`, `Pantry Staples`, `Frozen`, `Other`.

**FR-3.4** Add Ingredient: An inline form at the top of the page (or a modal) with Name, Quantity, and Category fields. Submitting via POST to `/Pantry/Add` saves the ingredient and redirects back to `/Pantry`.

**FR-3.5** Edit Ingredient: Each ingredient row has an Edit button that loads the ingredient into an inline edit form (or navigates to `/Pantry/Edit/{id}`). Fields: Name, Quantity, Category. Saving posts to `/Pantry/Edit/{id}` and redirects back to `/Pantry`.

**FR-3.6** Delete Ingredient: Each ingredient row has a Delete button. Deletion is confirmed via `confirm()` before posting to `/Pantry/Delete/{id}`. Redirects back to `/Pantry`.

**FR-3.7** Uniqueness is enforced: attempting to add or edit an ingredient to a name that already exists in the pantry (case-insensitive) displays a validation error message on the form.

---

### 3.4 Shopping List

**FR-4.1** The Shopping List page (`/ShoppingList`) displays all shopping list items grouped by category, sorted alphabetically within each group. This is a separate data table from the pantry.

**FR-4.2** Each shopping list item stores:

| Field | Type | Validation |
|---|---|---|
| Id | Integer PK | Auto-increment |
| Name | String (200) | Required |
| Quantity | String (100) | Required; free text |
| Category | Enum | Required; same fixed set as pantry |
| IsPurchased | Boolean | Default false |
| AddedAt | DateTime | UTC; set on creation |

**FR-4.3** Unchecked items are shown first; purchased items are shown below with a strikethrough style and 50% opacity.

**FR-4.4** Each item has a checkbox. Checking it marks `IsPurchased = true` and triggers a POST to `/ShoppingList/MarkPurchased/{id}` (no page reload required — use a small vanilla JS `fetch` call; the page can also update the item's visual state immediately via DOM manipulation).

**FR-4.5** When an item is marked as purchased, a "Move to Pantry" button appears next to it (or the checkbox action optionally auto-adds it). The explicit approach is preferred: the item gets a "Add to Pantry" button when purchased. Clicking it POSTs to `/ShoppingList/MoveToP pantry/{id}`, which creates a pantry entry (same name, quantity, category; prompts if name already exists in pantry) and removes the item from the shopping list.

**FR-4.6** Add Item: inline form or modal with Name, Quantity, Category. Duplicate names in the shopping list are allowed (user may want 2 separate entries for the same ingredient).

**FR-4.7** Delete Item: Delete button with `confirm()`. Posts to `/ShoppingList/Delete/{id}`.

**FR-4.8** "Clear Purchased" button removes all items where `IsPurchased = true` from the shopping list (bulk delete, confirmed via `confirm()`).

---

### 3.5 Recipe Browser & Filtering

**FR-5.1** The application ships with a seed library of **18 recipes** stored in `Data/recipes.json`. This file is read at application startup and seeded into the `Recipes` table if the table is empty (idempotent). The JSON structure matches the `Recipe` entity exactly.

**FR-5.2** Each recipe stores:

| Field | Type | Notes |
|---|---|---|
| Id | Integer PK | Auto-increment |
| Name | String (200) | Required |
| Description | String (500) | Required |
| MealTypes | String | Comma-separated enum values (Breakfast/Lunch/Dinner/Snack); a recipe may have multiple |
| PrepTimeMinutes | Integer | Required; min 1 |
| DefaultServings | Integer | Required; min 1 |
| CaloriesPerServing | Integer | Required; min 0 |
| ProteinPerServing | Decimal | grams |
| CarbsPerServing | Decimal | grams |
| FatPerServing | Decimal | grams |
| Instructions | String (text) | Newline-delimited steps |
| IsUserCreated | Boolean | False for seeded recipes; True for LLM-generated (stretch goal) |

**FR-5.3** Each recipe also has a child collection of `RecipeIngredient` records:

| Field | Type | Notes |
|---|---|---|
| Id | Integer PK | |
| RecipeId | Integer FK | |
| Name | String (200) | Required |
| Quantity | String (100) | Required; free text |

**FR-5.4** The Recipe Browser page (`/Recipes`) shows recipe cards in a responsive Bootstrap grid (3 columns on desktop, 2 on tablet, 1 on mobile). Default sort order: by name ascending.

**FR-5.5** Each recipe card displays: recipe name, meal type badge(s), prep time, calories per serving, % of ingredients owned (see FR-5.7), and a "View Recipe" button.

**FR-5.6** Filter controls appear above the recipe grid (always visible, not in a collapsible panel). Available filters:

| Filter | UI Control | Default |
|---|---|---|
| Meal Type | Multi-select checkboxes (All, Breakfast, Lunch, Dinner, Snack) | Pre-selected by time of day (see FR-5.3 note below) |
| Max Prep Time | Select dropdown: Any / ≤15 min / ≤30 min / ≤45 min / ≤60 min | Any |
| Min % Ingredients Owned | Select dropdown: Any / ≥25% / ≥50% / ≥75% / 100% | Any |

Default meal type selection by time of day (server local time at page load):
- Before 10:00 AM → Breakfast pre-selected
- 10:00 AM–1:59 PM → Lunch pre-selected
- 2:00 PM–4:59 PM → Snack pre-selected
- 5:00 PM and after → Dinner pre-selected

**FR-5.7** Ingredient ownership percentage is calculated server-side as:
`floor((count of recipe ingredients whose name matches a pantry ingredient name, case-insensitive) / total recipe ingredients × 100)`.
Recipes with 0 ingredients display 0%.

**FR-5.8** Filtering is applied server-side via a POST or GET with query parameters to `/Recipes`. The page re-renders with the filtered results. No client-side filtering.

---

### 3.6 Recipe Actions (Post-Selection)

**FR-6.1** The Recipe Detail page (`/Recipes/{id}`) displays: name, description, meal type badges, prep time, default servings, full ingredients list with quantities, instructions (numbered steps), and macros per serving.

**FR-6.2** A servings input (number field, min 1, default = recipe's `DefaultServings`) triggers recipe scaling. Changing the servings value updates the ingredients quantities and macro totals displayed on the page via vanilla JS (multiply each per-serving value by the new servings count; ingredient quantities are re-scaled proportionally as a decimal multiplier — e.g., "2 cups" at 4 servings → "1 cup" at 2 servings using numeric prefix extraction).

- Scaling rule for ingredient quantities: extract leading numeric value from the quantity string, multiply by `newServings / defaultServings`, round to 2 decimal places, reattach the unit suffix. If no leading numeric is found, display the quantity unchanged.

**FR-6.3** "Save Recipe" button: POSTs to `/Recipes/Save/{id}`. Creates a `SavedRecipe` record (`RecipeId`, `SavedAt`). The button changes to "Saved ✓" (disabled) if already saved. Saving a second time is a no-op (unique constraint on `RecipeId` in `SavedRecipes`).

**FR-6.4** "Add Missing Ingredients to Shopping List" button: POSTs to `/Recipes/AddMissing/{id}`. Server-side logic: for each `RecipeIngredient` in the recipe, check if a pantry item with the same name exists (case-insensitive). For each ingredient NOT in the pantry, add a `ShoppingListItem` with the recipe's ingredient name, quantity (scaled if user has changed servings — servings count passed as a form field), and category `Other` (since pantry category is unknown at this point). Duplicate shopping list entries are allowed. Displays a flash message: "Added {n} missing ingredients to your shopping list."

**FR-6.5** "Log This Meal" button: opens a modal with:
- Servings input (number, min 0.25, default = recipe's `DefaultServings`)
- Meal type selector (pre-selected by time of day per FR-5.6 rule)
- Confirm / Cancel buttons

Submitting POSTs to `/MealLog/LogRecipe`. Creates a `MealLogEntry` with macros scaled by servings. Redirects to `/MealLog` on success.

---

### 3.7 Stretch Goal — LLM Recipe Generation (Feature-Flagged)

**FR-7.1** A boolean flag `EnableLlmRecipes` in `appsettings.json` (default: `false`) controls this feature. When `false`, all UI elements and endpoints for this feature are hidden/disabled.

**FR-7.2** When enabled, a "Generate Recipe" button appears on the Recipe Browser page. It opens a form collecting: meal type (select), max prep time (number), dietary notes (optional free text, max 200 chars). The user's health profile goal and current pantry contents are included automatically.

**FR-7.3** On form submit, the application calls an external LLM API (provider and API key configured in `appsettings.json` under `LlmSettings:ApiKey` and `LlmSettings:ApiEndpoint`). The prompt instructs the LLM to return a recipe in the same JSON schema as `recipes.json`. The application parses the response into a `Recipe` object with `IsUserCreated = true` and displays it as a recipe card without persisting it to the database unless the user clicks "Save Recipe."

**FR-7.4** All data model interfaces and service interfaces for this feature shall be defined in the codebase even when the feature is disabled, so the implementation can be added without structural changes. The `ILlmRecipeService` interface shall exist in the Services layer.

---

## 4. Out of Scope

The following are explicitly excluded from this application:

- **Authentication or multi-user support** — no login, no sessions, no user accounts
- **Cloud sync or remote data storage** — all data stays in local SQLite
- **Calorie/nutrition database API integration** — no USDA, Nutritionix, or similar API lookups; all nutrition data is user-entered or seeded
- **Barcode scanning or food label parsing**
- **Meal planning calendar** (scheduling recipes for future days)
- **Notifications or reminders** (push, email, or in-app)
- **Social features** — no sharing, no export to social platforms
- **Dark mode or theme customization**
- **Export/import of user data** (CSV, PDF, etc.)
- **Third-party OAuth / Single Sign-On**
- **Nutritional goal recommendations from a medical source** — the calorie calculator is informational, not medical advice
- **Custom macro split configuration** — splits are fixed per goal (FR-1.5)
- **Recipe ratings or comments**
- **Undo functionality** for delete actions (delete is permanent after confirmation)

---

## 5. Success Criteria

The application is considered complete ("done") when all of the following are true:

| # | Criterion |
|---|---|
| SC-1 | A new user can set up their health profile and immediately see their calculated calorie and macro targets |
| SC-2 | A user can browse the seeded recipe library and filter by meal type, prep time, and ingredient availability |
| SC-3 | A user can log at least one meal per day for the current day and see live progress bars updating against their targets |
| SC-4 | A user can maintain a pantry inventory (add, edit, delete ingredients) grouped by category |
| SC-5 | A user can build a shopping list, check off purchased items, and move them to the pantry |
| SC-6 | "Add missing ingredients" correctly identifies and adds only the ingredients not in the pantry |
| SC-7 | Recipe scaling correctly recalculates ingredient quantities and macros for any serving count |
| SC-8 | All forms pass client-side HTML5 validation and server-side ModelState validation; invalid submissions do not persist data |
| SC-9 | All pages load under 2 seconds on localhost |
| SC-10 | All pages render usably on a 375px wide mobile viewport |
| SC-11 | Meal log entries older than 7 days are removed on startup |
| SC-12 | The `EnableLlmRecipes: false` default means zero LLM-related UI is visible in the default deployment |
