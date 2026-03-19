# Test Plan — SwiftPantry

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved (Pre-implementation)

> This document is written before any code exists. The QA Agent will execute this plan in Phase 5.
> All test IDs are stable references — do not renumber.

---

## Section A: Unit Test Plan

Unit tests run against isolated service classes with no browser, no HTTP stack, and no real database (use an in-memory SQLite or a plain in-memory `List<T>` where appropriate).

---

### Unit Test Suite: MacroCalculatorService
**Scope**: Mifflin-St Jeor BMR calculation, activity multipliers, goal calorie adjustments, macro gram conversion, unit conversion (lbs→kg, in→cm)
**Target Service**: `MacroCalculatorService` (or equivalent static/instance service)

| ID | Description | Input | Expected Output | Priority |
|----|-------------|-------|-----------------|----------|
| UT-001 | Male BMR calculation | age=30, weightKg=81.65, heightCm=177.8, sex=Male | BMR ≈ 1,783 kcal | P0 |
| UT-002 | Female BMR calculation | age=25, weightKg=58.97, heightCm=162.56, sex=Female | BMR ≈ 1,320 kcal | P0 |
| UT-003 | TDEE with Moderately Active multiplier | BMR=1,783, activityLevel=ModeratelyActive | TDEE ≈ 2,763 kcal (×1.55) | P0 |
| UT-004 | TDEE with Sedentary multiplier | BMR=1,500, activityLevel=Sedentary | TDEE = 1,800 kcal (×1.2) | P0 |
| UT-005 | TDEE with Extra Active multiplier | BMR=1,500, activityLevel=ExtraActive | TDEE = 2,850 kcal (×1.9) | P0 |
| UT-006 | Goal adjustment — Lose Weight | TDEE=2,000 | Adjusted = 1,500 kcal (−500) | P0 |
| UT-007 | Goal adjustment — Maintain | TDEE=2,000 | Adjusted = 2,000 kcal (no change) | P0 |
| UT-008 | Goal adjustment — Gain Weight | TDEE=2,000 | Adjusted = 2,300 kcal (+300) | P0 |
| UT-009 | Macro split — Lose Weight | calories=1,315, goal=LoseWeight | Protein=132 g, Carbs=99 g, Fat=44 g | P0 |
| UT-010 | Macro split — Maintain | calories=2,763, goal=Maintain | Protein=207 g, Carbs=276 g, Fat=92 g | P0 |
| UT-011 | Macro split — Gain Weight | calories=3,614, goal=GainWeight | Protein=271 g, Carbs=407 g, Fat=100 g | P0 |
| UT-012 | Pounds to kilograms conversion | weightLbs=180 | weightKg=81.647 (±0.001) | P0 |
| UT-013 | Inches to centimeters conversion | heightIn=70 | heightCm=177.8 | P0 |
| UT-014 | Full end-to-end TC-CALC-1 (Male, 30, 180 lbs, 70 in, Mod. Active, Maintain) | (see ACCEPTANCE_CRITERIA TC-CALC-1) | Target=2,763 kcal, P=207 g, C=276 g, F=92 g | P0 |
| UT-015 | Full end-to-end TC-CALC-2 (Female, 25, 130 lbs, 64 in, Lightly Active, Lose) | (see ACCEPTANCE_CRITERIA TC-CALC-2) | Target=1,315 kcal, P=132 g, C=99 g, F=44 g | P0 |
| UT-016 | Full end-to-end TC-CALC-3 (Male, 45, 220 lbs, 72 in, Very Active, Gain) | (see ACCEPTANCE_CRITERIA TC-CALC-3) | Target=3,614 kcal, P=271 g, C=407 g, F=100 g | P0 |
| UT-017 | Macro gram rounding — protein rounds up at .5 | calories=2,000, goal=Maintain, protein%=30% → 2000×0.3/4=150.0 | Protein=150 g | P1 |
| UT-018 | Activity multiplier — Lightly Active | BMR=1,319.67, activityLevel=LightlyActive | TDEE=1,814.55 → rounded = 1,815 kcal | P0 |
| UT-019 | Activity multiplier — Very Active | BMR=1,920.90, activityLevel=VeryActive | TDEE=3,313.56 → rounded = 3,314 kcal | P0 |
| UT-020 | Calorie target can be below 1,200 (no floor) | Very low profile → Adjusted < 1,200 | Returns calculated value without clamping | P1 |

---

### Unit Test Suite: RecipeService
**Scope**: Ingredient ownership % calculation, time-of-day meal type default, recipe filtering logic, recipe data access
**Target Service**: `RecipeService` (or `RecipeRepository` + filtering layer)

| ID | Description | Input | Expected Output | Priority |
|----|-------------|-------|-----------------|----------|
| UT-021 | Ownership % — exact match, 4 of 8 owned | Recipe with 8 ingredients; pantry = ["chicken breast", "olive oil", "salt", "black pepper"] | Ownership = 50% (`floor(4/8 × 100)`) | P0 |
| UT-022 | Ownership % — case-insensitive match | Recipe ingredient = "Chicken Breast"; pantry = ["chicken breast"] | Counts as owned (1/1 = 100%) | P0 |
| UT-023 | Ownership % — zero ingredients | Recipe with 0 ingredients | Ownership = 0% | P1 |
| UT-024 | Ownership % — all owned | Recipe with 6 ingredients; pantry contains all 6 exact names | Ownership = 100% | P0 |
| UT-025 | Ownership % — none owned | Recipe with 6 ingredients; pantry is empty | Ownership = 0% | P0 |
| UT-026 | Time-of-day default — 9:59 AM | serverTime=09:59 | MealType = Breakfast | P0 |
| UT-027 | Time-of-day default — 10:00 AM | serverTime=10:00 | MealType = Lunch | P0 |
| UT-028 | Time-of-day default — 1:59 PM | serverTime=13:59 | MealType = Lunch | P0 |
| UT-029 | Time-of-day default — 2:00 PM | serverTime=14:00 | MealType = Snack | P0 |
| UT-030 | Time-of-day default — 4:59 PM | serverTime=16:59 | MealType = Snack | P0 |
| UT-031 | Time-of-day default — 5:00 PM | serverTime=17:00 | MealType = Dinner | P0 |
| UT-032 | Filter by meal type — Dinner | 18 recipes, filter=Dinner | Returns 5 recipes (IDs 11, 12, 13, 14, 15) | P0 |
| UT-033 | Filter by meal type — multiple types | filter=[Breakfast, Snack] | Returns 8 recipes (5 breakfast + 3 snack) | P1 |
| UT-034 | Filter by prep time — ≤15 min | filter=maxPrepTime=15 | Returns 7 recipes with PrepTimeMinutes ≤ 15 | P1 |
| UT-035 | Filter by ownership — ≥50% | pantry = all ingredients of recipe 3 (6 items); filter=minOwned=50 | Recipe 3 is included; recipe with 0% is excluded | P1 |

---

### Unit Test Suite: ShoppingListService
**Scope**: Add-missing-from-recipe diff logic, duplicate prevention for pantry (not shopping list), quantity scaling
**Target Service**: `ShoppingListService`

| ID | Description | Input | Expected Output | Priority |
|----|-------------|-------|-----------------|----------|
| UT-036 | AddMissing — identifies missing ingredients | Recipe with ingredients [A, B, C, D]; pantry = [A, C] | Adds B and D to shopping list (2 items) | P0 |
| UT-037 | AddMissing — no duplicates with pantry | Recipe with ingredients [A, B]; pantry = [A, B] | Adds 0 items; returns count = 0 | P0 |
| UT-038 | AddMissing — case-insensitive pantry check | Recipe ingredient = "Chicken Breast"; pantry = ["chicken breast"] | Ingredient is NOT added (counts as owned) | P0 |
| UT-039 | AddMissing — quantity scaling with numeric prefix | Ingredient = "1 lb chicken breast", defaultServings=3, requestedServings=6 | Added item quantity = "2.00 lb" | P0 |
| UT-040 | AddMissing — quantity scaling non-integer | Ingredient = "3 tbsp soy sauce", defaultServings=3, requestedServings=9 | Added item quantity = "9.00 tbsp" | P0 |
| UT-041 | AddMissing — no leading numeric in quantity | Ingredient = "to taste salt", defaultServings=3, requestedServings=6 | Added item quantity = "to taste" (unchanged) | P0 |
| UT-042 | AddMissing — mixed fraction prefix (e.g., "1/2 cup") | Ingredient = "1/2 cup oats", defaultServings=1, requestedServings=2 | Note: if "1/2" is not extractable as a decimal, quantity is left unchanged; document the behavior | P1 |
| UT-043 | Shopping list allows duplicate names | Add "Chicken Breast" twice to shopping list | Both items exist in the list (2 rows) | P0 |
| UT-044 | AddMissing — category is always "Other" | Any missing ingredient added via AddMissing | The `ShoppingListItem.Category` = "Other" | P0 |

---

### Unit Test Suite: MealLogService
**Scope**: Daily summary aggregation, date boundary handling, old entry cleanup logic
**Target Service**: `MealLogService`

| ID | Description | Input | Expected Output | Priority |
|----|-------------|-------|-----------------|----------|
| UT-045 | Daily total — calories aggregation | 3 entries: 350 kcal × 1 srv, 325 kcal × 1.5 srv, 430 kcal × 2 srv | Total calories = 350 + 487.5 + 860 = 1,697.5 kcal | P0 |
| UT-046 | Daily total — macro aggregation | Entry: ProteinPerServing=38, Servings=1.5 | Total protein for that entry = 57 g | P0 |
| UT-047 | Date boundary — entries for today only | Entries on today, yesterday, and 3 days ago | `GetEntriesForDate(today)` returns only today's entries | P0 |
| UT-048 | Date boundary — uses server local date | Entry logged at 11:58 PM UTC, server in UTC-5 | Entry appears on the previous calendar day (local) | P1 |
| UT-049 | Cleanup — deletes entries older than 7 days | Entries on day-8, day-7, day-6, day-0 | After cleanup: day-8 deleted; day-7, day-6, day-0 retained | P0 |
| UT-050 | Cleanup — idempotent | Run cleanup with no old entries | Completes without error; 0 rows deleted | P0 |
| UT-051 | Progress bar percentage | consumed=1400, target=2763 | Percentage = (1400/2763) × 100 ≈ 50.67% | P0 |
| UT-052 | Progress bar over 100% | consumed=3000, target=2763 | Percentage ≥ 100%; bar capped at 100% width; `bg-danger` flag = true | P0 |

---

## Section B: Playwright End-to-End Test Plan

All E2E tests run against a live instance of the app using a dedicated test database. See Section C for infrastructure details.

**Base URL**: `http://localhost:5000` (configurable via environment variable)

---

### E2E Test Suite 1: First-Time User Flow
**Scope**: New user lands on dashboard → redirected to setup → completes profile → sees calculated macros

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-001 | Redirect to setup when no profile exists | 1. Start app with empty DB  2. `page.goto('/MealLog')` | `page.url()` should contain `/Profile/Setup`; navbar should NOT be visible | P0 |
| E2E-002 | Profile setup form renders all fields | 1. `page.goto('/Profile/Setup')` | Page contains inputs: `#Age`, `#Sex`, `#Height`, `#Weight`, `#HeightUnit`, `#WeightUnit`, `#ActivityLevel`, `#Goal`; submit button reads "Calculate & Save Profile" | P0 |
| E2E-003 | Valid profile submission redirects to /Profile with correct targets (TC-CALC-1) | 1. `page.goto('/Profile/Setup')`  2. `page.fill('#Age', '30')`  3. `page.selectOption('#Sex', 'Male')`  4. `page.fill('#Height', '70')`  5. `page.selectOption('#HeightUnit', 'in')`  6. `page.fill('#Weight', '180')`  7. `page.selectOption('#WeightUnit', 'lbs')`  8. `page.selectOption('#ActivityLevel', 'ModeratelyActive')`  9. `page.selectOption('#Goal', 'Maintain')`  10. `page.click('button[type="submit"]')` | URL is `/Profile`; page contains "2,763 kcal" (or "2763"); page contains "207" (protein g); page contains "276" (carbs g); page contains "92" (fat g); flash message contains "Profile created" | P0 |
| E2E-004 | Invalid profile submission re-renders form with errors | 1. `page.goto('/Profile/Setup')`  2. Leave Age blank  3. `page.click('button[type="submit"]')` | URL remains `/Profile/Setup`; page contains an error message for Age; no `UserProfile` row created | P0 |
| E2E-005 | After profile creation, navigation to /MealLog works | 1. Complete E2E-003 setup  2. `page.goto('/MealLog')` | URL is `/MealLog`; navbar is visible; page shows 4 progress bars with "0 / 2,763 kcal" | P0 |

---

### E2E Test Suite 2: Profile Management
**Scope**: Edit existing profile, verify recalculation

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-006 | Edit profile link is present on /Profile | 1. `page.goto('/Profile')` (with TC-CALC-1 fixture) | Page contains `a[href="/Profile/Edit"]` or button linking to Edit Profile | P0 |
| E2E-007 | Edit profile pre-populates current values | 1. `page.goto('/Profile/Edit')` | `#Age` value = "30"; `#Weight` value = "180"; `#Sex` selected = "Male" | P0 |
| E2E-008 | Changing goal recalculates targets | 1. `page.goto('/Profile/Edit')`  2. `page.selectOption('#Goal', 'LoseWeight')`  3. `page.click('button[type="submit"]')` | URL is `/Profile`; page now contains "2,263 kcal" (2763−500); protein target = 226 g (2263×0.4/4); carbs = 170 g (2263×0.3/4); fat = 75 g (2263×0.3/9) | P1 |

---

### E2E Test Suite 3: Recipe Browsing & Filtering
**Scope**: Navigate to recipes, apply filters, verify card content

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-009 | All 18 recipes appear on /Recipes | 1. `page.goto('/Recipes')` | `page.locator('.card').count()` should equal 18 | P0 |
| E2E-010 | Meal type filter — Dinner shows 5 recipes | 1. `page.goto('/Recipes')`  2. Uncheck pre-selected meal type  3. `page.check('#mt-dinner')`  4. `page.click('button[type="submit"]')` (or filter form submit) | Card count = 5; all visible cards show "Dinner" badge | P0 |
| E2E-011 | Prep time filter — ≤15 min | 1. `page.goto('/Recipes')`  2. `page.selectOption('#maxPrepTime', '15')`  3. Submit filter | Card count = 7; no visible card shows prep time > 15 min | P1 |
| E2E-012 | Time-of-day default — Lunch pre-selected at noon | 1. Mock server time to 12:00 PM (or run test at that time)  2. `page.goto('/Recipes')` | `#mt-lunch` checkbox is checked; `#mt-all` is unchecked | P0 |
| E2E-013 | Ownership % displays on recipe card | 1. Seed pantry: ["chicken breast", "olive oil", "salt", "black pepper"]  2. `page.goto('/Recipes')` | Grilled Chicken Salad card shows "50%" ownership | P1 |
| E2E-014 | Filter state persists in URL | 1. `page.goto('/Recipes?mealTypes=Snack')` | `#mt-snack` is checked; card count = 3 (recipes 16, 17, 18) | P1 |

---

### E2E Test Suite 4: Recipe Detail & Actions
**Scope**: View detail, adjust servings, save, add missing to shopping list, log meal

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-015 | Recipe detail page shows correct content | 1. `page.goto('/Recipes/6')` (Grilled Chicken Salad) | Page contains "Grilled Chicken Salad"; contains "38" (protein); contains "325"; contains "Lunch" badge; contains all 8 ingredient names | P0 |
| E2E-016 | Servings scaling updates macros in real time | 1. `page.goto('/Recipes/12')` (Chicken Stir Fry, default 3 srv)  2. `page.fill('#servingsInput', '6')` (or trigger change event) | `#totalCalories` displays 2,580 (430×6); `#totalProtein` displays 210 (35×6); ingredient "chicken breast" quantity displays "2.00 lb" | P0 |
| E2E-017 | "to taste" ingredients are unchanged by scaling | 1. `page.goto('/Recipes/12')`  2. Change servings to 6 | Ingredient "salt" quantity still displays "to taste" | P0 |
| E2E-018 | Save recipe button changes to Saved ✓ | 1. `page.goto('/Recipes/6')`  2. `page.click('button:has-text("Save Recipe")')` | Button now displays "Saved ✓" and is disabled; re-navigating to `/Recipes/6` still shows disabled "Saved ✓" | P1 |
| E2E-019 | Add Missing Ingredients — flash message and shopping list count | 1. Seed pantry: ["chicken breast", "olive oil", "salt"] (3 of 8 for recipe 6)  2. `page.goto('/Recipes/6')`  3. `page.click('button:has-text("Add Missing Ingredients")')` | Flash message reads "Added 5 missing ingredient(s) to your shopping list."; navigating to `/ShoppingList` shows 5 new items with category "Other" | P0 |
| E2E-020 | Log This Meal modal and redirect | 1. `page.goto('/Recipes/11')` (Baked Salmon, 385 kcal, 2 default srv)  2. `page.click('button:has-text("Log This Meal")')` (opens modal)  3. Verify servings input default = 2  4. `page.click('button:has-text("Confirm")')` (or modal submit) | URL is `/MealLog`; entry "Baked Salmon with Roasted Vegetables" is visible; entry shows 770 kcal total (385×2) | P0 |
| E2E-021 | 404 for non-existent recipe ID | 1. `page.goto('/Recipes/9999')` | HTTP response status = 404 | P1 |

---

### E2E Test Suite 5: Pantry Management
**Scope**: Add items, verify grouping, edit, delete

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-022 | Add a pantry ingredient | 1. `page.goto('/Pantry')`  2. `page.fill('input[name="Name"]', 'Chicken Breast')`  3. `page.fill('input[name="Quantity"]', '2 lbs')`  4. `page.selectOption('select[name="Category"]', 'Protein')`  5. `page.click('button[type="submit"]')` | Page shows "Chicken Breast" under "Protein" heading; the Protein group is visible | P0 |
| E2E-023 | Duplicate pantry name rejected (case-insensitive) | 1. Seed pantry with "chicken breast"  2. `page.goto('/Pantry')`  3. Add "CHICKEN BREAST"  4. Submit | Page re-renders with error message containing "already exists"; pantry still has only 1 entry for chicken breast | P0 |
| E2E-024 | Edit pantry ingredient | 1. Seed pantry with "apple" in Produce  2. `page.goto('/Pantry')`  3. Click Edit for "apple"  4. Change Quantity to "5 lbs"  5. Submit | Pantry shows "apple" with quantity "5 lbs"; still in Produce group | P0 |
| E2E-025 | Delete pantry ingredient removes it from the list | 1. Seed pantry with one item "garlic"  2. `page.goto('/Pantry')`  3. `page.click('button:has-text("Delete")')` (accept dialog)  4. Accept the confirm() dialog | "garlic" is no longer in the pantry list; if it was the only item, empty state is shown | P0 |

---

### E2E Test Suite 6: Shopping List Workflow
**Scope**: Add items, check off, auto-add to pantry, clear purchased

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-026 | Add item to shopping list | 1. `page.goto('/ShoppingList')`  2. Fill Name="Oat Milk", Quantity="1 carton", Category="Dairy"  3. Submit | "Oat Milk" appears in the Dairy group as unchecked | P0 |
| E2E-027 | Mark item purchased — visual update | 1. Seed shopping list with 1 unpurchased item "Bananas"  2. `page.goto('/ShoppingList')`  3. `page.check('input[type="checkbox"]')` for "Bananas" (triggers fetch) | Within 2 seconds: "Bananas" item has strikethrough style and opacity-50 class; "Add to Pantry" button appears | P0 |
| E2E-028 | Move to pantry — happy path | 1. Seed shopping list with purchased item "Eggs" (Protein)  2. `page.goto('/ShoppingList')`  3. `page.click('button:has-text("Add to Pantry")')` for "Eggs" | Flash message: "Added 'Eggs' to your pantry."; "Eggs" no longer in shopping list; navigating to `/Pantry` shows "Eggs" in Protein group | P1 |
| E2E-029 | Move to pantry — conflict shows error | 1. Seed pantry with "Eggs" AND shopping list with purchased item "Eggs"  2. `page.goto('/ShoppingList')`  3. Click "Add to Pantry" for "Eggs" | Flash error message: "'Eggs' is already in your pantry."; shopping list item still present; pantry still has original single entry | P1 |
| E2E-030 | Clear Purchased removes all purchased items | 1. Seed shopping list: 2 purchased + 1 unpurchased  2. `page.goto('/ShoppingList')`  3. `page.click('button:has-text("Clear Purchased")')` (accept dialog) | Page shows only the 1 unpurchased item; "Clear Purchased" button is gone | P1 |

---

### E2E Test Suite 7: Meal Logging & Daily Progress
**Scope**: Log a meal manually, verify progress bars update, quick-log a saved recipe

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-031 | Manual log — entry appears and totals update | 1. Load TC-CALC-1 profile fixture (target 2,763 kcal)  2. `page.goto('/MealLog')`  3. Fill manual log form: Name="Oatmeal", MealType=Breakfast, Servings=1, Calories=350, Protein=15, Carbs=54, Fat=8  4. Submit | Log list contains "Oatmeal"; calorie progress bar label reads "350 / 2,763 kcal"; protein bar reads "15 / 207 g" | P0 |
| E2E-032 | Progress bar turns red when over target | 1. Load TC-CALC-1 profile (calorie target 2,763)  2. Log 3 manual entries totaling 3,000 kcal  3. `page.goto('/MealLog')` | Calorie progress bar has CSS class `bg-danger`; label reads "3,000 / 2,763 kcal"; bar width is visually at 100% | P0 |
| E2E-033 | Delete log entry updates totals | 1. Seed 2 entries: 350 kcal + 325 kcal = 675 kcal total  2. `page.goto('/MealLog')`  3. Delete one entry (accept confirm dialog) | One entry remains; calorie bar label reads "350 / 2,763 kcal" (only the remaining entry) | P0 |
| E2E-034 | Quick Log section shows saved recipes | 1. Save recipe 6 (Grilled Chicken Salad)  2. `page.goto('/MealLog')` | "Quick Log" section is visible; card for "Grilled Chicken Salad" is present with a "Log" button | P1 |
| E2E-035 | Quick Log modal creates entry | 1. Save recipe 6 (325 kcal, 1 default serving)  2. `page.goto('/MealLog')`  3. Click "Log" button for Grilled Chicken Salad  4. Verify modal default servings = 1  5. Submit modal | New entry "Grilled Chicken Salad" appears in the log; calorie total updates by +325 kcal | P1 |

---

### E2E Test Suite 8: Data Persistence
**Scope**: Verify data survives page reload and app restart

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-036 | Profile persists after page reload | 1. Create profile via E2E-003  2. `page.reload()` | `/Profile` still shows correct values (2,763 kcal); no redirect to `/Profile/Setup` | P0 |
| E2E-037 | Meal log entry persists after page reload | 1. Log a meal  2. `page.goto('/MealLog')` (new navigation) | The logged meal entry is still visible in the list | P0 |
| E2E-038 | Pantry item persists after page reload | 1. Add pantry item "Salmon"  2. `page.reload()` | "Salmon" is still visible in the pantry | P0 |
| E2E-039 | Shopping list item persists after page reload | 1. Add shopping list item "Almond Milk"  2. `page.reload()` | "Almond Milk" is still visible in the shopping list | P0 |
| E2E-040 | Saved recipe persists after page reload | 1. Save recipe 6  2. `page.goto('/SavedRecipes')` | Recipe 6 is visible in the Saved Recipes page | P1 |

---

### E2E Test Suite 9: Navigation & Empty States
**Scope**: Visit every page with no data, verify empty state messages and links

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-041 | /MealLog empty state — no entries today | 1. Load profile fixture (no log entries)  2. `page.goto('/MealLog')` | Page shows an empty state message in the entries area; all 4 progress bars show "0 / {target}" | P1 |
| E2E-042 | /Pantry empty state | 1. Start with empty pantry  2. `page.goto('/Pantry')` | Page contains empty state message (e.g., "Your pantry is empty."); no category headings displayed | P1 |
| E2E-043 | /ShoppingList empty state | 1. Start with empty shopping list  2. `page.goto('/ShoppingList')` | Page contains "Your shopping list is empty." and a link to Browse Recipes | P1 |
| E2E-044 | /SavedRecipes empty state | 1. No saved recipes  2. `page.goto('/SavedRecipes')` | Page contains "No saved recipes yet." and a link to `/Recipes` | P1 |
| E2E-045 | /Recipes no filter results empty state | 1. `page.goto('/Recipes?mealTypes=Breakfast&maxPrepTime=1')` (no recipe under 1 min) | Grid area shows "No recipes match your filters." or similar; a prompt or button to clear filters is present | P1 |
| E2E-046 | Root redirect | 1. `page.goto('/')` | URL becomes `/MealLog` (302 redirect followed) | P1 |
| E2E-047 | Navbar active class on current page | 1. `page.goto('/Pantry')` | `a[href="/Pantry"]` or its parent `li` has CSS class `active` | P1 |

---

### E2E Test Suite 10: Responsive Layout
**Scope**: Verify layout at 375px mobile viewport

| ID | Description | Steps | Assertions | Priority |
|----|-------------|-------|------------|----------|
| E2E-048 | No horizontal scroll at 375px on /MealLog | 1. `page.setViewportSize({ width: 375, height: 812 })`  2. `page.goto('/MealLog')` | `document.body.scrollWidth` ≤ 375; no horizontal scrollbar | P1 |
| E2E-049 | No horizontal scroll at 375px on /Recipes | 1. `page.setViewportSize({ width: 375, height: 812 })`  2. `page.goto('/Recipes')` | Card grid is 1 column; `document.body.scrollWidth` ≤ 375 | P1 |
| E2E-050 | Navbar collapses to hamburger at 375px | 1. `page.setViewportSize({ width: 375, height: 812 })`  2. `page.goto('/MealLog')` | The `.navbar-toggler` (hamburger) button is visible; the nav links are NOT visible until toggler is clicked | P1 |
| E2E-051 | Recipe cards stack to 1 column at 375px | 1. `page.setViewportSize({ width: 375, height: 812 })`  2. `page.goto('/Recipes')` | Each card has width ≈ full container width (≥ 350px); no two cards appear side by side | P1 |

---

## Section C: Playwright Test Infrastructure Spec

---

### Playwright Infrastructure Requirements

#### Project Setup
```
tests/
  SwiftPantry.PlaywrightTests/
    SwiftPantry.PlaywrightTests.csproj
    playwright.config.json  (optional)
    Fixtures/
      TestFixtures.cs          ← DB seed helpers
    PageObjects/
      ProfilePage.cs
      MealLogPage.cs
      PantryPage.cs
      ShoppingListPage.cs
      RecipePage.cs
    Suites/
      FirstTimeUserFlowTests.cs
      ProfileTests.cs
      RecipeBrowsingTests.cs
      RecipeDetailTests.cs
      PantryTests.cs
      ShoppingListTests.cs
      MealLoggingTests.cs
      PersistenceTests.cs
      EmptyStateTests.cs
      ResponsiveLayoutTests.cs
    screenshots/              ← captured automatically on failure
```

- **NuGet packages required**: `Microsoft.Playwright.NUnit`, `Microsoft.AspNetCore.Mvc.Testing` (or a process-launch helper)
- **Target framework**: .NET 8.0 (matches main app)

---

#### App Startup for Tests

```csharp
// Option A — WebApplicationFactory (recommended for speed):
// Create a CustomWebApplicationFactory<Program> that overrides
// the SQLite connection string to use a temp file path:
//   Data Source=Data/swiftpantry_test.db
// This keeps test data isolated from dev data.

// Option B — process launch (simpler, less control):
// Start the app process on port 5001 before the test run,
// pointed at the test DB via environment variable.
```

- **Test database path**: `Data/swiftpantry_test.db` (relative to app working dir during tests)
- **DB reset strategy**: Before each test class's `[SetUp]`, delete and recreate the test DB, then seed known fixtures
- **Seeding must run before each suite's `[OneTimeSetUp]`**; individual test `[SetUp]` methods reset to the known state as needed

---

#### Standard Fixture Set

The following fixtures must be available as a baseline for most tests:

```
UserProfile:
  Age: 30, Sex: Male, Height: 177.8 cm, Weight: 81.65 kg,
  ActivityLevel: ModeratelyActive, Goal: Maintain
  → CalorieTarget: 2,763, ProteinTarget: 207 g, CarbsTarget: 276 g, FatTarget: 92 g

PantryItems (5 items):
  1. Name: "chicken breast",   Quantity: "6 oz",    Category: Protein
  2. Name: "olive oil",        Quantity: "1 bottle", Category: PantryStaples
  3. Name: "salt",             Quantity: "1 container", Category: PantryStaples
  4. Name: "black pepper",     Quantity: "1 jar",   Category: PantryStaples
  5. Name: "egg",              Quantity: "12 large", Category: Protein

MealLogEntries (for today):
  1. RecipeName: "Overnight Oats", MealType: Breakfast, Servings: 1,
     CaloriesPerServing: 350, ProteinPerServing: 15, CarbsPerServing: 54, FatPerServing: 8,
     LoggedAt: today at 08:00 UTC, RecipeId: 1

Recipes: all 18 seeded from Data/recipes.json (standard seed)

SavedRecipes: none (tests that need saved recipes add them in their own setup)

ShoppingList: empty (tests that need items add them in their own setup)
```

---

#### Test Helpers — Page Object Model

```csharp
// All helpers are async and wrap Playwright actions.

// ProfilePage.cs
public async Task CreateProfile(int age, string sex, decimal heightInches,
    decimal weightLbs, string activityLevel, string goal);
// Navigates to /Profile/Setup, fills and submits the form.

// PantryPage.cs
public async Task AddPantryItem(string name, string quantity, string category);
// Navigates to /Pantry, fills and submits the Add Item form.

public async Task<List<string>> GetPantryItemNames();
// Returns all visible ingredient names from the pantry list.

// ShoppingListPage.cs
public async Task AddShoppingListItem(string name, string quantity, string category);
// Fills and submits the Add Item form on /ShoppingList.

public async Task MarkItemPurchased(string itemName);
// Finds the checkbox for the named item and clicks it; waits for fetch to complete.

// RecipePage.cs
public async Task NavigateToRecipe(int recipeId);
// Navigates to /Recipes/{recipeId}.

public async Task SetServings(int servings);
// Sets the servings input and triggers the change event.

public async Task<string> GetIngredientQuantity(string ingredientName);
// Returns the displayed quantity for a named ingredient.

public async Task LogMeal(int servings, string mealType);
// Opens the Log This Meal modal, sets servings and meal type, confirms.

// MealLogPage.cs
public async Task LogMealManually(string recipeName, string mealType,
    decimal servings, int calories, decimal protein, decimal carbs, decimal fat);
// Fills and submits the manual log form.

public async Task<MacroSummary> GetDailyMacroSummary();
// Reads the 4 progress bar labels and returns:
// record MacroSummary(int CaloriesConsumed, int CaloriesTarget,
//                     int ProteinConsumed, int ProteinTarget,
//                     int CarbsConsumed, int CarbsTarget,
//                     int FatConsumed, int FatTarget);
```

---

#### Browser Configuration

```json
{
  "browser": "chromium",
  "headless": true,
  "headedModeEnvVar": "PLAYWRIGHT_HEADED=1",
  "defaultTimeout": 10000,
  "navigationTimeout": 15000,
  "screenshotOnFailure": true,
  "screenshotDir": "tests/SwiftPantry.PlaywrightTests/screenshots/",
  "screenshotNaming": "{testName}_{timestamp}.png",
  "viewportDefault": { "width": 1280, "height": 720 },
  "mobileViewport": { "width": 375, "height": 812 }
}
```

- Run headless by default; set `PLAYWRIGHT_HEADED=1` environment variable to run headed for debugging
- Use **Chromium only** — fastest, sufficient for all server-rendered Razor Pages
- Default action timeout: **10 seconds** (auto-retry until element is actionable)
- Navigation timeout: **15 seconds**
- On test failure: automatically capture a full-page screenshot to `screenshots/` using the test name and timestamp as the filename
- Do NOT use `page.waitForTimeout()` (fixed sleeps) — use `page.waitForSelector()` or Playwright's built-in auto-waiting
- All tests that use `confirm()` dialogs must register a `page.on('dialog', d => d.accept())` handler before triggering the action

---

#### Anti-Forgery Token Handling for AJAX Tests

When testing the `markPurchased` fetch endpoint (E2E-027), the test must:

1. Load the page normally (Playwright handles cookies automatically)
2. Extract the `__RequestVerificationToken` value from the page DOM or a cookie
3. Include it in the `fetch` request headers as `RequestVerificationToken`

The simplest approach: test the checkbox click as a normal Playwright action — Playwright will trigger the DOM's `onchange` handler, which fires the `fetch`. Wait for `page.waitForResponse('/ShoppingList/MarkPurchased/*')` to confirm the server responded 200 before asserting the visual state.

---

#### Priority Execution Order

When running the full suite in CI, execute suites in this order to fail fast on P0 blockers:

1. Suite 1: First-Time User Flow (E2E-001 through E2E-005)
2. Suite 2: Profile Management (E2E-006 through E2E-008)
3. Suite 5: Pantry Management (E2E-022 through E2E-025)
4. Suite 4: Recipe Detail & Actions (E2E-015 through E2E-021)
5. Suite 7: Meal Logging & Daily Progress (E2E-031 through E2E-035)
6. Suite 6: Shopping List Workflow (E2E-026 through E2E-030)
7. Suite 3: Recipe Browsing & Filtering (E2E-009 through E2E-014)
8. Suite 8: Data Persistence (E2E-036 through E2E-040)
9. Suite 9: Navigation & Empty States (E2E-041 through E2E-047)
10. Suite 10: Responsive Layout (E2E-048 through E2E-051)
