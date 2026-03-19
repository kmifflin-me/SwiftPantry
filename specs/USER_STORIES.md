# User Stories — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19

Priority Legend:
- **P0** — Must have (MVP blocker)
- **P1** — Should have (important but not a launch blocker)
- **P2** — Stretch / nice to have

---

## Feature Area 1: User Health Profile

---

### US-1.1 — Initial Profile Setup
**Priority: P0**

As a new user, I want to be guided to a setup page on first launch so that I can enter my health data before using the app.

**Acceptance Criteria:**
- If no `UserProfile` row exists in the database, any navigation to a non-profile page redirects to `/Profile/Setup`
- The setup page displays a form with all required fields: Age, Sex, Height, Height Unit, Weight, Weight Unit, Activity Level, Goal
- Submitting a valid form creates the profile and redirects to `/MealLog` (the home page)
- Submitting an invalid form re-displays the form with field-level error messages; no data is persisted

---

### US-1.2 — Calorie and Macro Target Calculation
**Priority: P0**

As a user, I want the app to calculate my daily calorie and macro targets automatically from my profile so that I don't have to do the math myself.

**Acceptance Criteria:**
- Calorie target uses the Mifflin-St Jeor equation (see PRD FR-1.3 through FR-1.5)
- Calorie target is adjusted by goal: Lose Weight = TDEE − 500, Maintain = TDEE, Gain Weight = TDEE + 300
- Macro targets in grams are derived from the adjusted calories using the goal-specific percentage splits (40/30/30, 30/40/30, 30/45/25)
- Calculated values are displayed on the profile confirmation page after saving
- Height and weight inputs stored and used in metric; unit conversion is applied when user selects imperial

---

### US-1.3 — Edit Health Profile
**Priority: P0**

As a user, I want to update my health profile whenever my stats or goals change so that my calorie and macro targets stay accurate.

**Acceptance Criteria:**
- `/Profile/Edit` pre-populates all fields with current stored values
- Saving valid changes recalculates and updates the stored calorie and macro targets
- A success message is displayed after saving
- Existing meal log entries are not modified when the profile changes
- All validation rules from initial setup apply on edit

---

## Feature Area 2: Daily Meal Log

---

### US-2.1 — View Daily Nutrition Summary
**Priority: P0**

As a user, I want to see how many calories and grams of each macro I've consumed today compared to my targets so that I know how much more I can eat.

**Acceptance Criteria:**
- `/MealLog` displays four progress bars: Calories, Protein, Carbs, Fat
- Each bar shows `consumed / target` with numeric labels (e.g., "1,450 / 2,100 kcal")
- Bars fill proportionally to the consumed/target ratio (capped visually at 100%)
- Bars display in Bootstrap `bg-danger` (red) when consumed exceeds target
- Totals update to reflect all entries for the selected date

---

### US-2.2 — View Meal Log for a Specific Day
**Priority: P0**

As a user, I want to view my meal log for any of the past 7 days so that I can review recent eating patterns.

**Acceptance Criteria:**
- A date picker on `/MealLog` allows selecting any date within the last 7 calendar days (today inclusive)
- Selecting a date reloads the page showing only entries for that date
- Dates older than 7 days from today are not selectable
- The selected date is shown as a heading above the log entries

---

### US-2.3 — Log a Meal Manually
**Priority: P0**

As a user, I want to manually log a meal by entering its name and nutrition info so that I can track meals that aren't in the recipe library.

**Acceptance Criteria:**
- A form on `/MealLog` accepts: Recipe Name (text), Meal Type (select), Servings (number ≥ 0.25), Calories (integer ≥ 0), Protein (decimal ≥ 0), Carbs (decimal ≥ 0), Fat (decimal ≥ 0)
- All fields are required; validation errors display inline
- Submitting creates a `MealLogEntry` with `LoggedAt` set to the current UTC time
- The new entry immediately appears in the log list
- Total macros for the entry are stored as per-serving values (the form inputs ARE per-serving values)

---

### US-2.4 — Delete a Meal Log Entry
**Priority: P0**

As a user, I want to delete a logged meal if I made a mistake so that my daily totals remain accurate.

**Acceptance Criteria:**
- Each meal log entry has a Delete button
- Clicking Delete shows a `confirm()` dialog with the meal name
- Confirming deletes the entry and refreshes the totals
- Canceling does nothing
- Deletion is permanent (no undo)

---

### US-2.5 — Quick Log a Saved Recipe
**Priority: P1**

As a user, I want to log a saved recipe with one click so that re-logging meals I eat regularly is fast.

**Acceptance Criteria:**
- A "Quick Log" section on `/MealLog` lists all saved recipes as cards
- Each card has a "Log" button that opens a modal
- The modal shows the recipe name, a servings input (default = recipe's `DefaultServings`), and a meal type selector (pre-selected by time of day)
- Submitting the modal creates a `MealLogEntry` linked to the recipe (`RecipeId` set)
- Macros are scaled proportionally by the servings entered
- The modal closes and the new entry appears in the log list

---

### US-2.6 — Automatic Cleanup of Old Log Entries
**Priority: P1**

As a user, I want the app to automatically clean up meal log entries older than 7 days so that the database doesn't grow indefinitely.

**Acceptance Criteria:**
- On application startup, all `MealLogEntry` rows with `LoggedAt` more than 7 days before the current server date are deleted
- This runs once per startup, not on a recurring schedule
- No user-visible notification is shown for this cleanup

---

## Feature Area 3: Ingredients Inventory (My Pantry)

---

### US-3.1 — View Pantry Inventory
**Priority: P0**

As a user, I want to see all my pantry ingredients grouped by category so that I can quickly find what I have.

**Acceptance Criteria:**
- `/Pantry` displays all ingredients grouped under their category headings
- Categories are sorted in this fixed order: Produce, Protein, Dairy, Grains, Pantry Staples, Frozen, Other
- Within each category, ingredients are sorted alphabetically by name
- Empty categories are not shown
- Each ingredient row shows: name, quantity, category, Edit button, Delete button

---

### US-3.2 — Add a Pantry Ingredient
**Priority: P0**

As a user, I want to add an ingredient to my pantry so that my inventory reflects what I actually have.

**Acceptance Criteria:**
- An add form on `/Pantry` accepts: Name (text), Quantity (text), Category (select)
- All fields are required
- Submitting saves the ingredient and refreshes the page showing the new item in its category group
- If the name already exists in the pantry (case-insensitive), an error message is shown and nothing is saved

---

### US-3.3 — Edit a Pantry Ingredient
**Priority: P0**

As a user, I want to edit a pantry ingredient's name, quantity, or category so that I can keep my inventory accurate.

**Acceptance Criteria:**
- Clicking Edit loads the ingredient's current values into an edit form
- All three fields (Name, Quantity, Category) are editable
- Saving posts to `/Pantry/Edit/{id}` and refreshes the pantry page
- Renaming to a name that already exists (case-insensitive, excluding itself) shows a validation error

---

### US-3.4 — Delete a Pantry Ingredient
**Priority: P0**

As a user, I want to remove an ingredient from my pantry so that the inventory stays current.

**Acceptance Criteria:**
- Each ingredient has a Delete button
- Clicking shows a `confirm()` dialog
- Confirming deletes the item and refreshes the page
- The ingredient disappears from the category group; the group heading is hidden if now empty

---

## Feature Area 4: Shopping List

---

### US-4.1 — View Shopping List
**Priority: P0**

As a user, I want to see my shopping list grouped by category so that I can shop efficiently.

**Acceptance Criteria:**
- `/ShoppingList` displays all items grouped by category (same fixed order as pantry)
- Within each category, items are sorted alphabetically
- Unchecked items appear before purchased items within each category group
- Purchased items display with strikethrough text and 50% opacity

---

### US-4.2 — Add an Item to the Shopping List
**Priority: P0**

As a user, I want to manually add items to my shopping list so that I can track what I need to buy.

**Acceptance Criteria:**
- An add form accepts: Name (text), Quantity (text), Category (select)
- All fields are required
- Duplicate names in the shopping list are permitted
- Submitting saves the item and refreshes the list

---

### US-4.3 — Mark a Shopping List Item as Purchased
**Priority: P0**

As a user, I want to check off items as I shop so that I can track my progress in the store.

**Acceptance Criteria:**
- Each unchecked item has a checkbox
- Checking it sends a POST to `/ShoppingList/MarkPurchased/{id}` without a full page reload (vanilla JS `fetch`)
- The item's visual state updates immediately (strikethrough + opacity) without a page reload
- An "Add to Pantry" button appears next to the purchased item

---

### US-4.4 — Move a Purchased Item to Pantry
**Priority: P1**

As a user, I want to automatically add a purchased item to my pantry so that I don't have to enter it twice.

**Acceptance Criteria:**
- Clicking "Add to Pantry" on a purchased item POSTs to `/ShoppingList/MoveToPantry/{id}`
- If the ingredient name does NOT exist in pantry (case-insensitive), it is added with the same name, quantity, and category, and the shopping list item is deleted
- If the ingredient name DOES exist in pantry, a confirmation or notice is shown asking the user to confirm overwrite or cancel; no data is changed until confirmed
- On success, the item disappears from the shopping list and appears in the pantry

---

### US-4.5 — Delete a Shopping List Item
**Priority: P0**

As a user, I want to delete an item from my shopping list so that I can remove things I no longer need.

**Acceptance Criteria:**
- Each item has a Delete button with a `confirm()` dialog
- Confirmed deletion removes the item permanently
- The page refreshes and the item is gone

---

### US-4.6 — Clear All Purchased Items
**Priority: P1**

As a user, I want to clear all checked-off items at once so that my shopping list only shows what I still need.

**Acceptance Criteria:**
- A "Clear Purchased" button at the top of the page is only visible if at least one purchased item exists
- Clicking shows a `confirm()` dialog: "Remove all {n} purchased items?"
- Confirming bulk-deletes all `IsPurchased = true` items
- Page refreshes showing only unpurchased items

---

### US-4.7 — Add Missing Recipe Ingredients to Shopping List
**Priority: P0**

As a user, I want to add only the ingredients I'm missing for a recipe directly to my shopping list so that I can quickly build my grocery list.

**Acceptance Criteria:**
- The "Add Missing Ingredients to Shopping List" button appears on the Recipe Detail page (`/Recipes/{id}`)
- Clicking POSTs to `/Recipes/AddMissing/{id}` with the current servings count from the page's servings input
- Server-side: for each recipe ingredient, if no pantry item exists with the same name (case-insensitive), it is added to the shopping list with quantity scaled to the requested servings and category = `Other`
- Ingredients already in the pantry are NOT added
- A flash message appears: "Added {n} missing ingredient(s) to your shopping list."
- If all ingredients are already in pantry, message reads: "All ingredients are already in your pantry."

---

## Feature Area 5: Recipe Browser & Filtering

---

### US-5.1 — Browse the Recipe Library
**Priority: P0**

As a user, I want to browse the seeded recipe library so that I can discover meal ideas.

**Acceptance Criteria:**
- `/Recipes` displays all 18 seeded recipes as Bootstrap cards in a responsive grid
- Grid is 3 columns on desktop (≥992px), 2 on tablet (768px–991px), 1 on mobile (<768px)
- Default sort order is recipe name, ascending
- Each card shows: name, meal type badge(s), prep time, calories per serving, % ingredients owned, "View Recipe" button
- Page loads all recipes; there is no pagination

---

### US-5.2 — Filter Recipes by Meal Type
**Priority: P0**

As a user, I want to filter recipes by meal type so that I see options relevant to the meal I'm preparing.

**Acceptance Criteria:**
- Filter checkboxes above the grid: All, Breakfast, Lunch, Dinner, Snack
- "All" deselects the others; selecting any individual type deselects "All"
- Default selection is determined by server local time at page load (see PRD FR-5.6)
- Selecting multiple types shows recipes matching ANY of the selected types
- Changing the filter submits a GET request and re-renders the filtered recipe grid
- The filter state persists in the URL query string (e.g., `?mealTypes=Breakfast,Lunch`)

---

### US-5.3 — Filter Recipes by Prep Time
**Priority: P1**

As a user, I want to filter recipes by maximum prep time so that I only see meals I have time to make.

**Acceptance Criteria:**
- A dropdown above the grid: Any / ≤15 min / ≤30 min / ≤45 min / ≤60 min; default = Any
- Selecting a value hides recipes with `PrepTimeMinutes` greater than the selected threshold
- Filter is applied server-side; the selected value persists in the URL query string

---

### US-5.4 — Filter Recipes by Ingredient Availability
**Priority: P1**

As a user, I want to filter recipes by how many ingredients I already own so that I can find meals I can make right now.

**Acceptance Criteria:**
- A dropdown: Any / ≥25% / ≥50% / ≥75% / 100%; default = Any
- Percentage is calculated server-side per PRD FR-5.7
- Selecting a threshold hides recipes below that ownership percentage
- "100%" shows only recipes where every ingredient is in the pantry
- Each recipe card always shows its current ownership percentage regardless of filter

---

### US-5.5 — Recipe Seed Data Loaded at Startup
**Priority: P0**

As a user, I want the app to come pre-loaded with recipes so that I can start using it immediately without manual data entry.

**Acceptance Criteria:**
- `Data/recipes.json` contains exactly 18 recipes covering a variety of meal types (at least 4 Breakfast, 5 Lunch/Dinner, 2 Snack, and mixed-type recipes)
- On startup, if the `Recipes` table is empty, all 18 records are seeded
- If the table already has data, seeding is skipped (idempotent)
- All seeded recipes have valid, non-zero nutrition values

---

## Feature Area 6: Recipe Detail & Actions

---

### US-6.1 — View Full Recipe Detail
**Priority: P0**

As a user, I want to view the full details of a recipe so that I know exactly how to make it.

**Acceptance Criteria:**
- `/Recipes/{id}` displays: name, description, meal type badge(s), prep time, default servings, ingredients list with quantities, numbered instruction steps, macros per serving (calories, protein, carbs, fat)
- If the recipe ID does not exist, a 404 page is returned
- A "Back to Recipes" link returns to `/Recipes` (preserving filter state if possible via Referer or query params)

---

### US-6.2 — Scale a Recipe by Servings
**Priority: P0**

As a user, I want to adjust the number of servings so that the ingredient quantities and macros update to reflect what I'm actually making.

**Acceptance Criteria:**
- A servings input (number, min 1, default = recipe's `DefaultServings`) is present on the recipe detail page
- Changing the value triggers a vanilla JS handler that:
  - Multiplies each macro per-serving value by the new servings total and updates the displayed totals
  - For each ingredient quantity: extracts the leading numeric prefix, multiplies by `newServings / defaultServings`, rounds to 2 decimal places, reattaches the unit suffix; if no numeric prefix is found, the quantity is displayed unchanged
- All scaling is client-side only; it does not persist to the database
- The servings input value is passed to any "Add Missing Ingredients" or "Log This Meal" action as a hidden form field

---

### US-6.3 — Save a Recipe
**Priority: P1**

As a user, I want to save a recipe to my personal list so that I can find it quickly and use it for quick logging.

**Acceptance Criteria:**
- A "Save Recipe" button is visible on the recipe detail page
- Clicking POSTs to `/Recipes/Save/{id}`
- If not yet saved, a `SavedRecipe` record is created and the button changes to "Saved ✓" (disabled)
- If already saved, the POST is a no-op and the button remains disabled with "Saved ✓"
- Saved recipes appear in the "Quick Log" section on `/MealLog`

---

### US-6.4 — Log a Recipe as a Meal from the Detail Page
**Priority: P0**

As a user, I want to log a recipe as a meal directly from its detail page so that I can track it without navigating away to the meal log.

**Acceptance Criteria:**
- A "Log This Meal" button opens a modal on the recipe detail page
- Modal fields: Servings (number, min 0.25, default = recipe's `DefaultServings`), Meal Type (select, pre-selected by time of day)
- Submitting POSTs to `/MealLog/LogRecipe`
- Creates a `MealLogEntry` with macros scaled by servings, `RecipeId` set, `LoggedAt` = current UTC time
- On success, redirects to `/MealLog`
- The modal can be closed/cancelled without logging anything

---

## Feature Area 7: LLM Recipe Generation (Stretch Goal)

---

### US-7.1 — Feature Flag Controls LLM UI Visibility
**Priority: P2**

As an operator, I want the LLM recipe generation feature to be hidden by default so that the app works without an API key configured.

**Acceptance Criteria:**
- `appsettings.json` contains `"EnableLlmRecipes": false` as the default
- When `false`, no "Generate Recipe" button, form, or endpoint is accessible
- When `true`, the generate recipe UI appears on `/Recipes`
- The flag is read at runtime (not compile-time); changing it requires an app restart

---

### US-7.2 — Generate a Recipe via LLM
**Priority: P2**

As a user, I want to generate a custom recipe using AI based on my pantry and preferences so that I can get meal ideas tailored to what I have.

**Acceptance Criteria:**
- A "Generate Recipe" button on `/Recipes` (visible only when `EnableLlmRecipes: true`) opens a form
- Form fields: Meal Type (select), Max Prep Time (number, optional), Dietary Notes (text, max 200 chars, optional)
- Submitting sends a prompt to the configured LLM API including: user goal, pantry ingredient names, and form inputs
- The response is parsed into a `Recipe` object (same schema as seeded recipes); if parsing fails, an error message is shown
- The generated recipe is displayed as a recipe card marked with a "Generated" badge
- The recipe is NOT saved to the database until the user explicitly clicks "Save Recipe"

---

### US-7.3 — Save an LLM-Generated Recipe
**Priority: P2**

As a user, I want to save a generated recipe so that I can use it for meal logging and quick log in the future.

**Acceptance Criteria:**
- The generated recipe card has a "Save Recipe" button
- Clicking saves the recipe to the `Recipes` table with `IsUserCreated = true`
- Saved generated recipes appear in the recipe browser alongside seeded recipes
- The save action uses the same `/Recipes/Save/{id}` flow; the recipe is assigned an ID upon first save

---

### US-7.4 — ILlmRecipeService Interface Exists
**Priority: P2**

As a developer, I want a defined `ILlmRecipeService` interface in the Services layer so that the LLM feature can be implemented without structural changes.

**Acceptance Criteria:**
- `ILlmRecipeService` is defined in the Services layer with at minimum a method: `Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request)`
- A stub implementation (`NoOpLlmRecipeService`) is registered in DI when `EnableLlmRecipes: false`
- A real implementation (`LlmRecipeService`) is registered when `EnableLlmRecipes: true`
- The `LlmRecipeRequest` model includes: MealType, MaxPrepTimeMinutes (nullable int), DietaryNotes (nullable string), PantryIngredients (list of strings), UserGoal (string)
