# Acceptance Criteria — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved (Pre-implementation)

> This document defines what "correct" looks like for every user story before any code is written.
> The Developer and Architect agents must implement against these criteria. The QA Agent will
> verify each criterion in Phase 5.

---

## Feature Area 1: User Health Profile

---

## Profile: US-1.1 — Initial Profile Setup
**Story**: As a new user, I want to be guided to a setup page on first launch so I can enter my health data before using the app.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN no `UserProfile` row exists in the database WHEN a user navigates to `/MealLog` THEN they are redirected (302) to `/Profile/Setup`
- [ ] GIVEN no `UserProfile` row exists WHEN a user navigates to `/Pantry` THEN they are redirected to `/Profile/Setup`
- [ ] GIVEN no `UserProfile` row exists WHEN a user navigates to `/Recipes` THEN they are redirected to `/Profile/Setup`
- [ ] GIVEN no `UserProfile` row exists WHEN a user navigates to `/ShoppingList` THEN they are redirected to `/Profile/Setup`
- [ ] GIVEN the user is on `/Profile/Setup` THEN the form displays all required fields: Age, Sex, Height, Height Unit, Weight, Weight Unit, Activity Level, Goal
- [ ] GIVEN the form is displayed THEN Sex is a select with options: Male, Female (no default selected)
- [ ] GIVEN the form is displayed THEN Activity Level is a select with options: Sedentary, Lightly Active, Moderately Active, Very Active, Extra Active
- [ ] GIVEN the form is displayed THEN Goal is a select with options: Lose Weight, Maintain, Gain Weight
- [ ] GIVEN the form is displayed THEN Height Unit is a select with options: Inches (in), Centimeters (cm)
- [ ] GIVEN the form is displayed THEN Weight Unit is a select with options: Pounds (lbs), Kilograms (kg)
- [ ] GIVEN the user submits a fully valid form THEN a `UserProfile` row is created in the database
- [ ] GIVEN the user submits a valid form THEN they are redirected to `/Profile` (not `/MealLog`)
- [ ] GIVEN the user is redirected to `/Profile` after setup THEN a success flash message reads: "Profile created! Here are your targets."
- [ ] GIVEN the user submits an invalid form (e.g., Age left blank) THEN no data is persisted and the form is re-displayed with field-level error messages
- [ ] GIVEN `UserProfile` already exists WHEN a user navigates to any page THEN they are NOT redirected to `/Profile/Setup`
- [ ] GIVEN the user is on `/Profile/Setup` THEN the navbar is NOT shown

### Edge Cases
- [ ] GIVEN the user submits Age = 9 (below minimum) THEN an error message is shown: "Age must be between 10 and 120." and no data is saved
- [ ] GIVEN the user submits Age = 121 (above maximum) THEN an error message is shown and no data is saved
- [ ] GIVEN the user selects Height Unit = "in" and submits Height = 23 THEN an error reads: "Height must be between 24–120 in."
- [ ] GIVEN the user selects Height Unit = "cm" and submits Height = 60 THEN an error reads: "Height must be between 61–305 cm."
- [ ] GIVEN the user selects Weight Unit = "lbs" and submits Weight = 49 THEN an error reads: "Weight must be between 50–1000 lbs."
- [ ] GIVEN the user selects Weight Unit = "kg" and submits Weight = 22 THEN an error reads: "Weight must be between 23–454 kg."
- [ ] GIVEN the user leaves Sex unselected and submits THEN an error reads: "Please select a sex."
- [ ] GIVEN the user leaves Activity Level unselected and submits THEN an error reads: "Please select an activity level."
- [ ] GIVEN the user leaves Goal unselected and submits THEN an error reads: "Please select a goal."

---

## Profile: US-1.2 — Calorie and Macro Target Calculation
**Story**: As a user, I want the app to calculate my daily calorie and macro targets automatically from my profile.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN a user profile is saved THEN the Mifflin-St Jeor equation is used server-side to calculate BMR
- [ ] GIVEN sex = Male THEN BMR = (10 × weight_kg) + (6.25 × height_cm) − (5 × age) + 5
- [ ] GIVEN sex = Female THEN BMR = (10 × weight_kg) + (6.25 × height_cm) − (5 × age) − 161
- [ ] GIVEN height is entered in inches THEN it is converted to cm (× 2.54) before the BMR calculation
- [ ] GIVEN weight is entered in lbs THEN it is converted to kg (÷ 2.20462) before the BMR calculation
- [ ] GIVEN TDEE is calculated THEN it equals BMR × the activity multiplier for the selected level
- [ ] GIVEN Goal = Lose Weight THEN adjusted calories = TDEE − 500
- [ ] GIVEN Goal = Maintain THEN adjusted calories = TDEE (no adjustment)
- [ ] GIVEN Goal = Gain Weight THEN adjusted calories = TDEE + 300
- [ ] GIVEN Goal = Lose Weight THEN macro split is 40% protein / 30% carbs / 30% fat
- [ ] GIVEN Goal = Maintain THEN macro split is 30% protein / 40% carbs / 30% fat
- [ ] GIVEN Goal = Gain Weight THEN macro split is 30% protein / 45% carbs / 25% fat
- [ ] GIVEN macro grams are computed THEN protein and carbs use 4 kcal/g and fat uses 9 kcal/g
- [ ] GIVEN macro grams are computed THEN each is rounded to the nearest whole gram
- [ ] GIVEN a profile is saved THEN `/Profile` displays: TDEE, adjusted calorie target, protein grams target, carbs grams target, fat grams target

**Specific numeric test cases (must match exactly ±1 unit due to rounding):**

| # | Input | Expected Output |
|---|-------|----------------|
| TC-CALC-1 | Male, age 30, weight 180 lbs (81.65 kg), height 70 in (177.8 cm), Moderately Active (×1.55), Maintain | BMR = 1,783 kcal · TDEE = 2,763 kcal · Target = 2,763 kcal · Protein = 207 g · Carbs = 276 g · Fat = 92 g |
| TC-CALC-2 | Female, age 25, weight 130 lbs (58.97 kg), height 64 in (162.56 cm), Lightly Active (×1.375), Lose Weight | BMR = 1,320 kcal · TDEE = 1,815 kcal · Target = 1,315 kcal · Protein = 132 g · Carbs = 99 g · Fat = 44 g |
| TC-CALC-3 | Male, age 45, weight 220 lbs (99.79 kg), height 72 in (182.88 cm), Very Active (×1.725), Gain Weight | BMR = 1,921 kcal · TDEE = 3,314 kcal · Target = 3,614 kcal · Protein = 271 g · Carbs = 407 g · Fat = 100 g |

### Edge Cases
- [ ] GIVEN a female user with very low calorie target (e.g., Sedentary, Lose Weight) THEN adjusted calories can be below 1,200 kcal — no floor is applied; the value is displayed as calculated
- [ ] GIVEN a Sedentary activity level THEN multiplier = 1.2 is applied
- [ ] GIVEN an Extra Active activity level THEN multiplier = 1.9 is applied

---

## Profile: US-1.3 — Edit Health Profile
**Story**: As a user, I want to update my health profile whenever my stats or goals change.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/Profile/Edit` THEN all form fields are pre-populated with the currently stored values
- [ ] GIVEN the user submits valid changes THEN the profile is updated, targets are recalculated, and the user is redirected to `/Profile`
- [ ] GIVEN the user saves a valid edit THEN a success flash message is displayed on `/Profile`
- [ ] GIVEN the user has meal log entries WHEN they save an edited profile THEN those existing meal log entries are NOT modified
- [ ] GIVEN the user submits an invalid form on `/Profile/Edit` THEN the same validation rules as setup apply and no data is persisted
- [ ] GIVEN the user changes Goal from Maintain to Lose Weight THEN the new calorie target displayed = new TDEE − 500 and macros reflect the 40/30/30 split

### Edge Cases
- [ ] GIVEN the user changes weight unit from lbs to kg and edits the value THEN the new stored value reflects the kg conversion correctly
- [ ] GIVEN no changes are made and the form is resubmitted THEN values remain unchanged and targets are stable

---

## Feature Area 2: Daily Meal Log

---

## Meal Log: US-2.1 — View Daily Nutrition Summary
**Story**: As a user, I want to see how many calories and macros I've consumed today vs. my targets.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/MealLog` THEN four progress bars are displayed: Calories, Protein, Carbs, Fat
- [ ] GIVEN the user has no meal entries for today THEN all four bars show "0 / {target}" with zero fill width
- [ ] GIVEN the user has logged meals for today THEN each bar shows "{consumed} / {target}" with the correct unit label (kcal for calories, g for macros)
- [ ] GIVEN consumed calories = 1,400 and target = 2,763 THEN the calorie bar is filled to approximately 50.7% width and shows "1,400 / 2,763 kcal"
- [ ] GIVEN consumed calories < target THEN the progress bar uses Bootstrap's default color (not `bg-danger`)
- [ ] GIVEN consumed calories ≥ target THEN the progress bar gains the CSS class `bg-danger` (red) and is visually capped at 100% width
- [ ] GIVEN consumed protein = 250 g and target = 207 g THEN the protein bar is capped at 100% visual width, has class `bg-danger`, and shows "250 / 207 g"
- [ ] GIVEN the user switches the date picker to a different day THEN the four bars reflect the totals for that selected day

**Specific calculation test:**
- [ ] GIVEN a user profile with TC-CALC-1 values (target 2,763 kcal) AND two logged entries: (1) Grilled Chicken Salad, 1 serving, 325 kcal and (2) Overnight Oats, 1 serving, 350 kcal THEN calories bar shows "675 / 2,763 kcal" and is approximately 24% filled

### Edge Cases
- [ ] GIVEN no `UserProfile` exists THEN `/MealLog` redirects to `/Profile/Setup` before attempting to render targets
- [ ] GIVEN target macros result in 0 g (edge case in calculation) THEN the progress bar does not divide by zero and displays "0 / 0 g" safely

---

## Meal Log: US-2.2 — View Meal Log for a Specific Day
**Story**: As a user, I want to view my meal log for any of the past 7 days.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user is on `/MealLog` THEN a date picker is visible that allows selecting dates
- [ ] GIVEN today is 2026-03-19 THEN the date picker allows selecting any date from 2026-03-13 to 2026-03-19 (7 days inclusive)
- [ ] GIVEN the user selects 2026-03-16 THEN the page reloads showing only entries where `LoggedAt` (local time) falls on March 16
- [ ] GIVEN the user selects a date THEN a heading above the log list displays that date (e.g., "March 16, 2026")
- [ ] GIVEN the user selects today (the default) THEN the heading reads today's date
- [ ] GIVEN the user attempts to select a date 8 days ago THEN that date is not selectable in the date picker

### Edge Cases
- [ ] GIVEN the user switches to a past day that has no entries THEN an empty state is shown (e.g., "No meals logged on this day.") and all four progress bars show 0 / target
- [ ] GIVEN entries exist in UTC across a midnight boundary THEN they are grouped by server local date, not UTC date

---

## Meal Log: US-2.3 — Log a Meal Manually
**Story**: As a user, I want to manually log a meal by entering its name and nutrition info.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the manual log form is present on `/MealLog` THEN it includes fields: Recipe Name (text), Meal Type (select), Servings (number), Calories (number), Protein (number), Carbs (number), Fat (number)
- [ ] GIVEN the user fills all fields validly and submits THEN a `MealLogEntry` row is created with `LoggedAt` = current UTC time and `RecipeId` = null
- [ ] GIVEN a new entry is created THEN it immediately appears in the log list for today without requiring a full manual reload (redirect-after-POST is acceptable)
- [ ] GIVEN the Calories field contains a value per serving and Servings = 2 THEN the displayed total for that entry = Calories × 2
- [ ] GIVEN the form inputs are per-serving values THEN `CaloriesPerServing`, `ProteinPerServing`, `CarbsPerServing`, `FatPerServing` are stored exactly as entered; totals are calculated on display
- [ ] GIVEN Meal Type select is shown THEN options are: Breakfast, Lunch, Dinner, Snack
- [ ] GIVEN Servings < 0.25 is entered THEN a validation error is shown and no entry is created
- [ ] GIVEN any numeric field is negative THEN a validation error is shown: "Value must be 0 or greater."
- [ ] GIVEN Recipe Name is left blank THEN a validation error is shown: "Recipe name is required."
- [ ] GIVEN Meal Type is left unselected THEN a validation error is shown

### Edge Cases
- [ ] GIVEN Servings = 0.25 (minimum valid) THEN the entry is accepted and created
- [ ] GIVEN Calories = 0 THEN the entry is accepted (valid; zero-calorie items are allowed)
- [ ] GIVEN the user submits the form with invalid data THEN the form is re-displayed with previously entered valid values retained in the fields

---

## Meal Log: US-2.4 — Delete a Meal Log Entry
**Story**: As a user, I want to delete a logged meal if I made a mistake.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN a meal entry is in the list THEN a Delete button is visible on that entry row
- [ ] GIVEN the user clicks Delete THEN a `confirm()` dialog appears with the text containing the meal name (e.g., "Delete 'Grilled Chicken Salad'?")
- [ ] GIVEN the user confirms the dialog THEN the entry is deleted from the database and disappears from the list
- [ ] GIVEN the entry is deleted THEN the four daily totals / progress bars update to reflect the removed entry's values
- [ ] GIVEN the user cancels the dialog THEN no data is changed and the entry remains in the list
- [ ] GIVEN the entry is deleted THEN there is no undo option

### Edge Cases
- [ ] GIVEN the last entry for a day is deleted THEN the empty state message is shown and all progress bars show 0 / target

---

## Meal Log: US-2.5 — Quick Log a Saved Recipe
**Story**: As a user, I want to log a saved recipe with one click.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the user has at least one saved recipe THEN a "Quick Log" section appears on `/MealLog` listing saved recipe cards
- [ ] GIVEN the Quick Log section is displayed THEN each card shows the recipe name and a "Log" button
- [ ] GIVEN the user clicks "Log" on a saved recipe card THEN a modal opens showing: recipe name, servings input (default = recipe's `DefaultServings`), meal type select (pre-selected by time of day)
- [ ] GIVEN the modal is open and the user enters 2 servings for a recipe with 430 kcal/serving THEN submitting creates a `MealLogEntry` with `CaloriesPerServing` = 430, `Servings` = 2, displayed total = 860 kcal
- [ ] GIVEN the modal is submitted THEN `RecipeId` is set on the `MealLogEntry`
- [ ] GIVEN the modal is submitted THEN the new entry appears in the day's log list and progress bars update
- [ ] GIVEN the user clicks Cancel in the modal THEN no entry is created and the modal closes
- [ ] GIVEN the user has zero saved recipes THEN the Quick Log section is either hidden or shows a message: "No saved recipes yet. Browse the recipe library to save some."

### Edge Cases
- [ ] GIVEN the servings input in the modal is changed to 0.25 (minimum) THEN the entry is created successfully
- [ ] GIVEN the servings input is cleared/empty THEN the modal submit shows a validation error and does not close

---

## Meal Log: US-2.6 — Automatic Cleanup of Old Log Entries
**Story**: As a user, I want the app to automatically clean up meal log entries older than 7 days.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the application starts THEN all `MealLogEntry` rows with `LoggedAt` (UTC) older than 7 days from the current server date are deleted
- [ ] GIVEN an entry was logged exactly 7 days ago (same calendar date as today, 7 days prior) THEN it is NOT deleted (it falls within the retention window)
- [ ] GIVEN an entry was logged 8 days ago THEN it IS deleted on startup
- [ ] GIVEN the cleanup runs THEN no user-visible notification or confirmation is shown
- [ ] GIVEN the application starts a second time on the same day THEN the cleanup runs again but has no entries to delete (idempotent)

### Edge Cases
- [ ] GIVEN the database has no entries older than 7 days THEN startup completes normally with no errors

---

## Feature Area 3: Ingredients Inventory (My Pantry)

---

## Pantry: US-3.1 — View Pantry Inventory
**Story**: As a user, I want to see all my pantry ingredients grouped by category.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/Pantry` THEN all pantry ingredients are displayed grouped by category heading
- [ ] GIVEN categories are displayed THEN they appear in this fixed order: Produce, Protein, Dairy, Grains, Pantry Staples, Frozen, Other
- [ ] GIVEN a category has no ingredients THEN that category heading is NOT shown
- [ ] GIVEN a category has ingredients THEN they are sorted alphabetically by name within that group (case-insensitive)
- [ ] GIVEN an ingredient is displayed THEN its row shows: name, quantity, Edit button, Delete button

### Edge Cases
- [ ] GIVEN the pantry has no ingredients THEN an empty state is shown (e.g., "Your pantry is empty. Add your first ingredient above.") instead of category headings

---

## Pantry: US-3.2 — Add a Pantry Ingredient
**Story**: As a user, I want to add an ingredient to my pantry.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the add form on `/Pantry` is submitted with Name = "Chicken Breast", Quantity = "2 lbs", Category = "Protein" THEN the ingredient appears in the Protein group sorted correctly
- [ ] GIVEN the user submits a new ingredient THEN the page refreshes showing the ingredient in the correct category group
- [ ] GIVEN the pantry already contains "chicken breast" (lowercase) WHEN the user adds "Chicken Breast" (different case) THEN an error is shown: a duplicate name already exists (case-insensitive match)
- [ ] GIVEN Name is left blank THEN an error reads: "Name is required." and nothing is saved
- [ ] GIVEN Quantity is left blank THEN an error reads: "Quantity is required." and nothing is saved
- [ ] GIVEN Category is not selected THEN an error reads: "Please select a category." and nothing is saved

### Edge Cases
- [ ] GIVEN Category = "Other" is selected THEN the ingredient appears under the "Other" group
- [ ] GIVEN the same name with extra leading/trailing whitespace is submitted THEN it is treated as a duplicate (trimmed before comparison)

---

## Pantry: US-3.3 — Edit a Pantry Ingredient
**Story**: As a user, I want to edit a pantry ingredient's name, quantity, or category.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user clicks Edit on an ingredient THEN the edit form is pre-populated with that ingredient's current Name, Quantity, and Category
- [ ] GIVEN the user changes the Name to a new unique name and saves THEN the ingredient is updated and the pantry reflects the new name
- [ ] GIVEN the user changes the Category THEN the ingredient moves to the new category group on the pantry page after save
- [ ] GIVEN the user changes the Name to a name already used by a DIFFERENT pantry ingredient (case-insensitive) THEN an error is shown and no changes are saved
- [ ] GIVEN the user edits an ingredient's name to the same name (no change) THEN the save succeeds without a uniqueness error (self-exclusion in uniqueness check)
- [ ] GIVEN valid changes are saved THEN the page redirects to `/Pantry` with the updated values displayed

### Edge Cases
- [ ] GIVEN the edit form is submitted with Quantity left blank THEN a validation error is shown: "Quantity is required."

---

## Pantry: US-3.4 — Delete a Pantry Ingredient
**Story**: As a user, I want to remove an ingredient from my pantry.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user clicks Delete on an ingredient THEN a `confirm()` dialog appears
- [ ] GIVEN the user confirms THEN the ingredient is deleted from the database and removed from the list
- [ ] GIVEN the ingredient was the last one in its category THEN the category heading is no longer displayed
- [ ] GIVEN the user cancels the dialog THEN no data is changed

### Edge Cases
- [ ] GIVEN the deleted ingredient was used in a recipe's pantry match calculation THEN the next load of `/Recipes` reflects the updated ownership percentage for affected recipes

---

## Feature Area 4: Shopping List

---

## Shopping List: US-4.1 — View Shopping List
**Story**: As a user, I want to see my shopping list grouped by category.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/ShoppingList` THEN items are grouped under category headings in the same fixed order as the pantry (Produce, Protein, Dairy, Grains, Pantry Staples, Frozen, Other)
- [ ] GIVEN a category group is displayed THEN unpurchased items appear before purchased items within that group
- [ ] GIVEN items within a section (purchased or unpurchased) THEN they are sorted alphabetically by name
- [ ] GIVEN an item is marked as purchased THEN it displays with strikethrough text and 50% opacity (`text-decoration-line-through`, `opacity-50`)
- [ ] GIVEN a category has no items THEN that category heading is NOT shown

### Edge Cases
- [ ] GIVEN the shopping list has no items THEN an empty state is shown: "Your shopping list is empty." with a link to Browse Recipes

---

## Shopping List: US-4.2 — Add an Item to the Shopping List
**Story**: As a user, I want to manually add items to my shopping list.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the add form is submitted with Name, Quantity, and Category THEN the item is saved and appears in the correct category group
- [ ] GIVEN the shopping list already contains an item named "Chicken Breast" WHEN another item named "Chicken Breast" is added THEN both items appear (duplicates in the shopping list ARE permitted)
- [ ] GIVEN Name is blank THEN a validation error is shown and nothing is saved
- [ ] GIVEN Quantity is blank THEN a validation error is shown and nothing is saved
- [ ] GIVEN Category is not selected THEN a validation error is shown and nothing is saved

---

## Shopping List: US-4.3 — Mark a Shopping List Item as Purchased
**Story**: As a user, I want to check off items as I shop.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN an unpurchased item is in the list THEN a checkbox is visible next to it
- [ ] GIVEN the user checks the checkbox THEN a POST is sent to `/ShoppingList/MarkPurchased/{id}` via `fetch` (no full page reload)
- [ ] GIVEN the POST succeeds THEN the item's visual state updates immediately: strikethrough text and 50% opacity applied
- [ ] GIVEN the item is marked purchased THEN an "Add to Pantry" button becomes visible next to it
- [ ] GIVEN the item is marked purchased THEN the checkbox is disabled (cannot be unchecked via UI)
- [ ] GIVEN the POST request includes a valid anti-forgery token THEN the server returns 200 OK; without it the server returns 400 or 403

### Edge Cases
- [ ] GIVEN the server returns an error on the fetch THEN the UI does not update the item's visual state and the checkbox reverts to unchecked

---

## Shopping List: US-4.4 — Move a Purchased Item to Pantry
**Story**: As a user, I want to automatically add a purchased item to my pantry.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the user clicks "Add to Pantry" on a purchased item WHEN no pantry item exists with that name (case-insensitive) THEN a new `PantryItem` is created with the same name, quantity, and category; the shopping list item is deleted; the page redirects to `/ShoppingList` with flash message: "Added '{name}' to your pantry."
- [ ] GIVEN the user clicks "Add to Pantry" WHEN a pantry item already exists with the same name (case-insensitive) THEN the page redirects to `/ShoppingList` with error flash message: "'{name}' is already in your pantry. Remove it from the pantry first, or delete this shopping list item." — no data is changed
- [ ] GIVEN the item is successfully moved THEN it no longer appears in the shopping list
- [ ] GIVEN the item is successfully moved THEN it appears in the pantry under its category

### Edge Cases
- [ ] GIVEN the item's name is "chicken BREAST" and the pantry contains "Chicken Breast" THEN the duplicate is detected (case-insensitive) and the error message is shown

---

## Shopping List: US-4.5 — Delete a Shopping List Item
**Story**: As a user, I want to delete an item from my shopping list.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user clicks Delete on a shopping list item THEN a `confirm()` dialog appears
- [ ] GIVEN the user confirms THEN the item is permanently deleted and removed from the list
- [ ] GIVEN the user cancels THEN no data is changed
- [ ] GIVEN the item was the last one in its category group THEN the category heading is no longer displayed after deletion

---

## Shopping List: US-4.6 — Clear All Purchased Items
**Story**: As a user, I want to clear all checked-off items at once.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN at least one purchased item exists THEN a "Clear Purchased (n)" button is visible at the top of the page where n = count of purchased items
- [ ] GIVEN zero purchased items exist THEN the "Clear Purchased" button is NOT rendered
- [ ] GIVEN the user clicks "Clear Purchased (3)" THEN a `confirm()` dialog appears with text: "Remove all 3 purchased items?"
- [ ] GIVEN the user confirms THEN all `IsPurchased = true` items are deleted and the page refreshes showing only unpurchased items
- [ ] GIVEN the user cancels THEN no data is changed

### Edge Cases
- [ ] GIVEN exactly 1 purchased item exists THEN the dialog reads: "Remove all 1 purchased items?" (grammatical quirk is acceptable)

---

## Shopping List: US-4.7 — Add Missing Recipe Ingredients to Shopping List
**Story**: As a user, I want to add only the missing ingredients for a recipe to my shopping list.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user is on `/Recipes/{id}` THEN an "Add Missing Ingredients to Shopping List" button is visible (unless all ingredients are owned)
- [ ] GIVEN the button is clicked THEN a POST is sent to `/Recipes/AddMissing/{id}` including the current servings count from the page's servings input
- [ ] GIVEN the recipe has 8 ingredients and the pantry contains 3 of them (exact case-insensitive name match) THEN exactly 5 shopping list items are added with category = "Other"
- [ ] GIVEN an ingredient IS in the pantry (case-insensitive match) THEN it is NOT added to the shopping list
- [ ] GIVEN the current servings value is 6 and the recipe default is 3 servings WHEN a missing ingredient has quantity "1 lb" THEN the added shopping list item has quantity "2.00 lb"
- [ ] GIVEN items are successfully added THEN a flash message reads: "Added {n} missing ingredient(s) to your shopping list."
- [ ] GIVEN all recipe ingredients are already in the pantry THEN no items are added and the flash message reads: "All ingredients are already in your pantry."
- [ ] GIVEN all ingredients are owned THEN the "Add Missing Ingredients" button is replaced by a disabled button labeled "All Ingredients in Pantry"

**Specific test case — Grilled Chicken Salad (recipe 6, 8 ingredients):**
- [ ] GIVEN pantry contains: ["chicken breast", "olive oil", "salt"] (3 of 8) WHEN "Add Missing Ingredients" is clicked at 1 serving THEN 5 items are added: "mixed green" (3 cup), "cherry tomato" (1/2 cup), "cucumber" (1/2 medium), "red wine vinegar" (1 tbsp), "black pepper" (to taste) — all with category "Other"

**Specific test case — quantity scaling:**
- [ ] GIVEN recipe has ingredient "soy sauce" quantity "3 tbsp" and default servings = 3 WHEN "Add Missing Ingredients" is clicked at 6 servings THEN added item has quantity "6.00 tbsp"
- [ ] GIVEN an ingredient has quantity "to taste" (no leading numeric) THEN the added item quantity is "to taste" unchanged regardless of servings

### Edge Cases
- [ ] GIVEN the user clicks "Add Missing Ingredients" twice without changes THEN duplicate shopping list items ARE created for the missing ingredients (shopping list allows duplicates per FR-4.6)

---

## Feature Area 5: Recipe Browser & Filtering

---

## Recipes: US-5.1 — Browse the Recipe Library
**Story**: As a user, I want to browse the seeded recipe library.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/Recipes` THEN all 18 seeded recipes are displayed as Bootstrap cards
- [ ] GIVEN the page is viewed at ≥992px viewport width THEN cards are in a 3-column grid
- [ ] GIVEN the page is viewed at 768px–991px THEN cards are in a 2-column grid
- [ ] GIVEN the page is viewed at <768px THEN cards are in a 1-column grid
- [ ] GIVEN no filter is applied THEN recipes are sorted by name ascending (alphabetical)
- [ ] GIVEN a recipe card is displayed THEN it shows: recipe name, meal type badge(s), prep time (e.g., "10 min"), calories per serving (e.g., "350 kcal / serving"), % ingredients owned (e.g., "37%"), and a "View Recipe" button
- [ ] GIVEN the user clicks "View Recipe" THEN they are navigated to `/Recipes/{id}`
- [ ] GIVEN the recipes page loads THEN there is no pagination — all matching recipes are shown

### Edge Cases
- [ ] GIVEN the seeded data has a recipe with multiple meal types (e.g., Breakfast,Lunch) THEN the card displays both meal type badges

---

## Recipes: US-5.2 — Filter Recipes by Meal Type
**Story**: As a user, I want to filter recipes by meal type.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the page loads at 9:59 AM (server local time) THEN the "Breakfast" checkbox is pre-selected
- [ ] GIVEN the page loads at 10:00 AM THEN the "Lunch" checkbox is pre-selected
- [ ] GIVEN the page loads at 1:59 PM THEN the "Lunch" checkbox is pre-selected
- [ ] GIVEN the page loads at 2:00 PM THEN the "Snack" checkbox is pre-selected
- [ ] GIVEN the page loads at 4:59 PM THEN the "Snack" checkbox is pre-selected
- [ ] GIVEN the page loads at 5:00 PM THEN the "Dinner" checkbox is pre-selected
- [ ] GIVEN the user selects "All" THEN all individual type checkboxes are deselected and all 18 recipes are shown
- [ ] GIVEN the user selects "Dinner" THEN only recipes with "dinner" in their `MealTypes` field are shown (expected: recipes 11, 12, 13, 14, 15 = 5 recipes)
- [ ] GIVEN the user selects both "Breakfast" and "Lunch" THEN recipes matching EITHER type are shown
- [ ] GIVEN the filter form is submitted THEN the page re-renders via GET and the selected meal types appear in the URL query string (e.g., `?mealTypes=Breakfast`)
- [ ] GIVEN a query string with `?mealTypes=Dinner` THEN the "Dinner" checkbox is checked and the "All" checkbox is unchecked when the page renders

### Edge Cases
- [ ] GIVEN no meal type checkboxes are checked (edge state) THEN the behavior is equivalent to "All" (all recipes shown) or the "All" checkbox is auto-selected

---

## Recipes: US-5.3 — Filter Recipes by Prep Time
**Story**: As a user, I want to filter recipes by maximum prep time.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the user selects "≤ 15 min" from the Max Prep Time dropdown THEN only recipes with `PrepTimeMinutes` ≤ 15 are shown (expected from seed data: Overnight Oats (10), Greek Yogurt Parfait (5), Scrambled Eggs with Toast (10), Avocado Toast with Egg (10), Turkey and Avocado Wrap (10), Apple with Peanut Butter (5), Cottage Cheese with Berries (5) = 7 recipes)
- [ ] GIVEN the user selects "≤ 30 min" THEN recipes with `PrepTimeMinutes` > 30 are excluded
- [ ] GIVEN the user selects "Any" THEN no prep time filter is applied
- [ ] GIVEN the filter is applied THEN the selected value persists in the URL query string (e.g., `?maxPrepTime=15`)
- [ ] GIVEN the page is loaded with `?maxPrepTime=30` in the URL THEN the "≤ 30 min" option is selected in the dropdown

---

## Recipes: US-5.4 — Filter Recipes by Ingredient Availability
**Story**: As a user, I want to filter recipes by how many ingredients I already own.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the user selects "≥ 50%" THEN only recipes where ownership % ≥ 50 are shown
- [ ] GIVEN the user selects "100%" THEN only recipes where every ingredient is in the pantry are shown
- [ ] GIVEN the user selects "Any" THEN no ownership filter is applied
- [ ] GIVEN a recipe has 0 ingredients THEN its ownership % displays as 0%
- [ ] GIVEN the filter is applied THEN the ownership % displayed on each card is calculated fresh from the current pantry state

**Specific ownership calculation test:**
- [ ] GIVEN pantry contains: ["chicken breast", "olive oil", "salt", "black pepper"] WHEN `/Recipes` is loaded THEN the Grilled Chicken Salad card (recipe 6, 8 ingredients, owns: chicken breast, olive oil, salt, black pepper) shows "50%" ownership (`floor(4/8 × 100)` = 50)
- [ ] GIVEN pantry contains: ["egg", "whole wheat bread", "butter", "milk", "salt", "black pepper"] WHEN `/Recipes` is loaded THEN the Scrambled Eggs with Toast card (recipe 3, 6 ingredients, owns all 6) shows "100%" and is included in a "100%" filter

### Edge Cases
- [ ] GIVEN a recipe ingredient name is "Chicken Breast" (capitalized) and the pantry item is "chicken breast" (lowercase) THEN they count as a match (case-insensitive)

---

## Recipes: US-5.5 — Recipe Seed Data Loaded at Startup
**Story**: As a user, I want the app to come pre-loaded with recipes.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN a fresh database (empty `Recipes` table) WHEN the app starts THEN all 18 recipes from `Data/recipes.json` are seeded into the `Recipes` table
- [ ] GIVEN the `Recipes` table already has data WHEN the app starts THEN seeding is skipped (no duplicates created)
- [ ] GIVEN the app is started a second time THEN the recipe count remains 18 (idempotent)
- [ ] GIVEN the seeded data is loaded THEN the recipe type distribution includes: at least 4 Breakfast recipes (IDs 1–5), at least 5 Lunch/Dinner recipes, at least 2 Snack recipes
- [ ] GIVEN any seeded recipe is loaded THEN `CaloriesPerServing` > 0, `ProteinPerServing` ≥ 0, `CarbsPerServing` ≥ 0, `FatPerServing` ≥ 0 for all 18

---

## Feature Area 6: Recipe Detail & Actions

---

## Recipe Detail: US-6.1 — View Full Recipe Detail
**Story**: As a user, I want to view the full details of a recipe.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user navigates to `/Recipes/6` (Grilled Chicken Salad) THEN the page displays: name "Grilled Chicken Salad", description, meal type badge "Lunch", prep time "20 min", default servings "1", all 8 ingredient rows with quantities, numbered instruction steps (6 steps), and macro values: 325 kcal / 38 g protein / 12 g carbs / 14 g fat
- [ ] GIVEN the user navigates to `/Recipes/9999` (non-existent ID) THEN a 404 HTTP response is returned
- [ ] GIVEN the user is on a recipe detail page THEN a "Back to Recipes" link is present that returns to `/Recipes`
- [ ] GIVEN the recipe has multiple meal types THEN all applicable meal type badges are displayed

---

## Recipe Detail: US-6.2 — Scale a Recipe by Servings
**Story**: As a user, I want to adjust the number of servings so ingredient quantities and macros update.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user is on the Chicken and Vegetable Stir Fry detail page (recipe 12, default 3 servings, 430 kcal/srv, 35 g protein, 45 g carbs, 12 g fat) THEN the page initially shows total macro values: 1,290 kcal, 105 g protein, 135 g carbs, 36 g fat
- [ ] GIVEN the user changes the servings input to 6 THEN the displayed totals update to: 2,580 kcal, 210 g protein, 270 g carbs, 72 g fat (via JS, no page reload)
- [ ] GIVEN the user changes the servings input to 6 (from default 3) THEN the "chicken breast" quantity updates from "1 lb" to "2.00 lb"
- [ ] GIVEN the user changes the servings to 6 THEN "soy sauce" updates from "3 tbsp" to "6.00 tbsp"
- [ ] GIVEN the user changes the servings to 6 THEN "broccoli" updates from "2 cup floret" to "4.00 cup floret"
- [ ] GIVEN an ingredient quantity has no leading numeric (e.g., "to taste") THEN the quantity displays unchanged regardless of servings change
- [ ] GIVEN the user changes servings THEN the hidden servings field (used by "Add Missing Ingredients" and "Log This Meal" forms) is updated to the new value
- [ ] GIVEN the user changes servings to 1.5 for a recipe with default 2 THEN ingredient quantities scale by factor 0.75 (e.g., "2 cup" → "1.50 cup")

### Edge Cases
- [ ] GIVEN the servings input is changed to 0 or left blank THEN the scaling JS either ignores the input or defaults to 1 — no division by zero error
- [ ] GIVEN the servings input is changed to a decimal like 1.5 THEN scaling works correctly (the input accepts non-integer values)

---

## Recipe Detail: US-6.3 — Save a Recipe
**Story**: As a user, I want to save a recipe to my personal list.
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN the recipe has NOT been saved THEN a "Save Recipe" button is shown (enabled)
- [ ] GIVEN the user clicks "Save Recipe" THEN a POST is sent to `/Recipes/Save/{id}` and a `SavedRecipe` row is created
- [ ] GIVEN the recipe is saved THEN the button changes to "Saved ✓" and is disabled
- [ ] GIVEN the recipe is ALREADY saved and the user POSTs again (e.g., manually) THEN the operation is a no-op (no duplicate `SavedRecipe` row is created; unique constraint on `RecipeId` is enforced)
- [ ] GIVEN a recipe is saved THEN it appears in the Quick Log section on `/MealLog`
- [ ] GIVEN a recipe is saved THEN it appears in the `/SavedRecipes` page

### Edge Cases
- [ ] GIVEN the user saves a recipe from the detail page and then navigates back THEN the "Saved ✓" button state persists (server-rendered check against `SavedRecipes` table)

---

## Recipe Detail: US-6.4 — Log a Recipe as a Meal from the Detail Page
**Story**: As a user, I want to log a recipe as a meal directly from its detail page.
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user is on a recipe detail page THEN a "Log This Meal" button is visible
- [ ] GIVEN the user clicks "Log This Meal" THEN a modal opens with: servings input (default = recipe's `DefaultServings`, min 0.25), meal type select (pre-selected by time of day)
- [ ] GIVEN the modal is open at 6:30 PM THEN the meal type defaults to "Dinner"
- [ ] GIVEN the modal is open at 8:00 AM THEN the meal type defaults to "Breakfast"
- [ ] GIVEN the user submits the modal with 2 servings for Baked Salmon (recipe 11: 385 kcal/srv, 38 g protein, 18 g carbs, 18 g fat) THEN a `MealLogEntry` is created with `CaloriesPerServing` = 385, `ProteinPerServing` = 38, `CarbsPerServing` = 18, `FatPerServing` = 18, `Servings` = 2, `RecipeId` = 11
- [ ] GIVEN the entry is created THEN `LoggedAt` is set to the current UTC timestamp
- [ ] GIVEN the log entry is created THEN the user is redirected to `/MealLog`
- [ ] GIVEN the user is redirected to `/MealLog` THEN the new entry is visible in the list and totals (2 × 385 = 770 kcal) are reflected in the progress bar
- [ ] GIVEN the user clicks Cancel in the modal THEN no entry is created and the modal closes

### Edge Cases
- [ ] GIVEN the servings in the modal is changed to 0.25 THEN the entry is created with `Servings` = 0.25 and the total calories display = `CaloriesPerServing` × 0.25

---

## Feature Area 7: LLM Recipe Generation (Stretch Goal)

---

## LLM: US-7.1 — Feature Flag Controls LLM UI Visibility
**Story**: As an operator, I want the LLM recipe generation feature to be hidden by default.
**Priority**: P2

### Acceptance Criteria
- [ ] GIVEN `appsettings.json` contains `"EnableLlmRecipes": false` THEN no "Generate Recipe" button is visible on `/Recipes`
- [ ] GIVEN `"EnableLlmRecipes": false` THEN the generate recipe endpoint returns 404 or 403 if accessed directly
- [ ] GIVEN `"EnableLlmRecipes": true` THEN the "Generate Recipe" button appears on `/Recipes`
- [ ] GIVEN the flag is changed and the app is restarted THEN the new value takes effect (runtime read, not compile-time)

---

## LLM: US-7.2 — Generate a Recipe via LLM
**Story**: As a user, I want to generate a custom recipe using AI.
**Priority**: P2

### Acceptance Criteria
- [ ] GIVEN `EnableLlmRecipes: true` and the user clicks "Generate Recipe" THEN a form opens with: Meal Type (select), Max Prep Time (number, optional), Dietary Notes (text, max 200 chars, optional)
- [ ] GIVEN the form is submitted THEN the LLM prompt includes the user's goal and pantry ingredient names
- [ ] GIVEN the LLM returns a valid JSON recipe THEN it is displayed as a recipe card with a "Generated" badge
- [ ] GIVEN the LLM returns invalid/unparseable JSON THEN an error message is shown and no crash occurs
- [ ] GIVEN the generated recipe is displayed THEN it is NOT in the database until the user clicks "Save Recipe"

---

## LLM: US-7.3 — Save an LLM-Generated Recipe
**Story**: As a user, I want to save a generated recipe.
**Priority**: P2

### Acceptance Criteria
- [ ] GIVEN the user clicks "Save Recipe" on a generated recipe THEN the recipe is saved to the `Recipes` table with `IsUserCreated = true`
- [ ] GIVEN the recipe is saved THEN it appears in the recipe browser alongside seeded recipes
- [ ] GIVEN the recipe is saved THEN it can be used for meal logging

---

## LLM: US-7.4 — ILlmRecipeService Interface Exists
**Story**: As a developer, I want a defined `ILlmRecipeService` interface.
**Priority**: P2

### Acceptance Criteria
- [ ] GIVEN the codebase THEN `ILlmRecipeService` exists in the Services layer with method: `Task<Recipe> GenerateRecipeAsync(LlmRecipeRequest request)`
- [ ] GIVEN `EnableLlmRecipes: false` THEN `NoOpLlmRecipeService` is registered in DI
- [ ] GIVEN `EnableLlmRecipes: true` THEN `LlmRecipeService` is registered in DI
- [ ] GIVEN the `LlmRecipeRequest` model THEN it contains: MealType, MaxPrepTimeMinutes (nullable int), DietaryNotes (nullable string), PantryIngredients (list of strings), UserGoal (string)

---

## Cross-Cutting Concerns

---

## Cross-Cutting: Data Persistence
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user creates a health profile and then refreshes the browser THEN the profile data is still present
- [ ] GIVEN the user logs a meal and then restarts the application THEN the meal log entry is still present
- [ ] GIVEN the user adds a pantry item and the browser tab is closed and reopened THEN the pantry item is still present
- [ ] GIVEN the user adds a shopping list item and the app is restarted THEN the item is still present
- [ ] GIVEN the user saves a recipe and the app is restarted THEN the saved recipe is still present

---

## Cross-Cutting: Navigation
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN the user has a profile AND is on any page THEN the navbar is visible with links: Meal Log, Recipes, Pantry, Shopping, Saved, Profile
- [ ] GIVEN the user clicks "Meal Log" in the navbar THEN they navigate to `/MealLog`
- [ ] GIVEN the user visits `/` (root) THEN they are redirected (302) to `/MealLog`
- [ ] GIVEN the current page is `/MealLog` THEN the "Meal Log" nav link has the `active` CSS class

---

## Cross-Cutting: Empty States
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN `/MealLog` has no entries for today THEN the log list area shows a message (e.g., "No meals logged today. Use the form below or Quick Log to get started.")
- [ ] GIVEN `/Pantry` has no ingredients THEN the page body shows a message (e.g., "Your pantry is empty.") with prompt to add an ingredient
- [ ] GIVEN `/ShoppingList` has no items THEN the page body shows: "Your shopping list is empty." with a link to Browse Recipes
- [ ] GIVEN `/Recipes` filter returns 0 results THEN the grid area shows: "No recipes match your filters." with a prompt to adjust filters
- [ ] GIVEN `/SavedRecipes` has no saved recipes THEN the page shows: "No saved recipes yet." with a link to Browse Recipes

---

## Cross-Cutting: Input Validation
**Priority**: P0

### Acceptance Criteria
- [ ] GIVEN any form on any page THEN HTML5 `required` attributes are set on required fields
- [ ] GIVEN server-side ModelState is invalid THEN the form is re-rendered with `.is-invalid` CSS classes on failing fields and inline error messages in `.invalid-feedback` divs
- [ ] GIVEN any invalid form is submitted THEN NO data is written to the database
- [ ] GIVEN numeric fields accept min/max ranges THEN values outside the range produce error messages describing the valid range
- [ ] GIVEN a form is submitted with a missing anti-forgery token THEN the server returns 400 Bad Request

---

## Cross-Cutting: Responsive Layout
**Priority**: P1

### Acceptance Criteria
- [ ] GIVEN a 375px wide viewport THEN no page has horizontal scrollbar overflow
- [ ] GIVEN a 375px wide viewport THEN multi-column layouts stack vertically
- [ ] GIVEN a 375px wide viewport THEN the navbar collapses to a hamburger menu
- [ ] GIVEN a 375px wide viewport THEN all buttons and form inputs are large enough to tap (minimum 44px touch target)
