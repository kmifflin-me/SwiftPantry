# UX Flows — SwiftPantry (SwiftPantry)

**Version:** 1.0
**Date:** 2026-03-19
**Status:** Approved

---

## Part 1 — Page Inventory

| URL | Page Name | Notes |
|-----|-----------|-------|
| `/` | Root | Permanent redirect (302) to `/MealLog` |
| `/Profile/Setup` | Profile Setup | Only accessible when no UserProfile exists; all other pages redirect here if no profile |
| `/Profile` | Health Profile | View profile values + calculated targets; links to Edit |
| `/Profile/Edit` | Edit Profile | Pre-populated edit form; redirects to `/Profile` on save |
| `/MealLog` | Meal Log (Dashboard) | Home page; daily progress + meal entries + quick log |
| `/Pantry` | My Pantry | Ingredient inventory grouped by category |
| `/ShoppingList` | Shopping List | Shopping list grouped by category |
| `/Recipes` | Recipe Browser | Filterable recipe grid |
| `/Recipes/{id}` | Recipe Detail | Full recipe view with scaling and actions |
| `/SavedRecipes` | Saved Recipes | User's saved recipe collection |

---

## Part 2 — Global Components

### Navbar
Present on every page after profile setup. Hidden on `/Profile/Setup`.

```
[navbar navbar-expand-lg navbar-dark bg-dark fixed-top]
  [navbar-brand href="/MealLog": SwiftPantry]
  [navbar-toggler (hamburger, data-bs-target="#mainNav")]
  [collapse navbar-collapse id="mainNav"]
    [navbar-nav ms-auto]
      [nav-item: a.nav-link href="/MealLog"    > Meal Log  ]
      [nav-item: a.nav-link href="/Recipes"    > Recipes   ]
      [nav-item: a.nav-link href="/Pantry"     > Pantry    ]
      [nav-item: a.nav-link href="/ShoppingList" > Shopping ]
      [nav-item: a.nav-link href="/SavedRecipes" > Saved    ]
      [nav-item: a.nav-link href="/Profile"    > Profile   ]
```

- Active nav-link gets `active` class based on current URL prefix.
- On mobile (< lg breakpoint): hamburger collapses all links into a vertical stack.
- Body must have `padding-top: 70px` to clear the fixed navbar.

### Flash Message Banner
Displayed directly below the navbar when a TempData["Success"] or TempData["Error"] key is set.

```
[container-fluid px-0]
  [alert alert-success alert-dismissible fade show role="alert"] (if TempData["Success"] set)
    {TempData["Success"]}
    [button.btn-close data-bs-dismiss="alert"]
  [alert alert-danger  alert-dismissible fade show role="alert"] (if TempData["Error"] set)
    {TempData["Error"]}
    [button.btn-close data-bs-dismiss="alert"]
```

---

## Part 3 — Per-Page Specifications

---

### Page: `/Profile/Setup`

**Purpose:** Collect the user's health data on first launch so daily calorie and macro targets can be calculated.

**When shown:** Any page navigation when `UserProfile` table is empty redirects here. This page itself is accessible whether or not a profile exists (to allow direct navigation for testing purposes), but profile-checking middleware always sends new users here first.

**Layout sketch:**

```
[body bg-light]
  [container] (max-width enforced by container, centered)
    [row justify-content-center mt-5]
      [col-12 col-md-8 col-lg-6]

        [card shadow-sm]
          [card-header bg-primary text-white]
            [h4.mb-0: Set Up Your Health Profile]
          [card-body p-4]

            [form method="post" action="/Profile/Setup"]
              [antiforgery token hidden input]

              /* Age + Sex row */
              [row g-3 mb-3]
                [col-md-6]
                  [label.form-label for="Age": Age]
                  [input#Age.form-control type="number" name="Age"
                    min="10" max="120" required placeholder="e.g. 28"]
                  [div.invalid-feedback: Age must be between 10 and 120.]
                [col-md-6]
                  [label.form-label for="Sex": Sex]
                  [select#Sex.form-select name="Sex" required]
                    [option value="" disabled selected: — Select —]
                    [option value="Male": Male]
                    [option value="Female": Female]
                  [div.invalid-feedback: Please select a sex.]

              /* Height row */
              [row g-3 mb-3]
                [col-md-6]
                  [label.form-label for="Height": Height]
                  [input#Height.form-control type="number" name="Height"
                    step="0.1" required placeholder="e.g. 68"]
                  [div.invalid-feedback id="heightError": (dynamic — set by server based on unit)]
                [col-md-6]
                  [label.form-label for="HeightUnit": Unit]
                  [select#HeightUnit.form-select name="HeightUnit" required]
                    [option value="in": Inches (in)]
                    [option value="cm": Centimeters (cm)]
                  [div.invalid-feedback: Please select a unit.]

              /* Weight row */
              [row g-3 mb-3]
                [col-md-6]
                  [label.form-label for="Weight": Weight]
                  [input#Weight.form-control type="number" name="Weight"
                    step="0.1" required placeholder="e.g. 175"]
                  [div.invalid-feedback id="weightError": (dynamic — set by server)]
                [col-md-6]
                  [label.form-label for="WeightUnit": Unit]
                  [select#WeightUnit.form-select name="WeightUnit" required]
                    [option value="lbs": Pounds (lbs)]
                    [option value="kg": Kilograms (kg)]
                  [div.invalid-feedback: Please select a unit.]

              /* Activity Level */
              [row g-3 mb-3]
                [col-12]
                  [label.form-label for="ActivityLevel": Activity Level]
                  [select#ActivityLevel.form-select name="ActivityLevel" required]
                    [option value="" disabled selected: — Select —]
                    [option value="Sedentary":        Sedentary (little or no exercise)]
                    [option value="LightlyActive":    Lightly Active (1–3 days/week)]
                    [option value="ModeratelyActive": Moderately Active (3–5 days/week)]
                    [option value="VeryActive":       Very Active (6–7 days/week)]
                    [option value="ExtraActive":      Extra Active (hard exercise + physical job)]
                  [div.invalid-feedback: Please select an activity level.]

              /* Goal */
              [row g-3 mb-4]
                [col-12]
                  [label.form-label for="Goal": Your Goal]
                  [select#Goal.form-select name="Goal" required]
                    [option value="" disabled selected: — Select —]
                    [option value="LoseWeight":  Lose Weight  (−500 kcal/day)]
                    [option value="Maintain":    Maintain     (TDEE)]
                    [option value="GainWeight":  Gain Weight  (+300 kcal/day)]
                  [div.invalid-feedback: Please select a goal.]

              [button.btn.btn-primary.w-100.btn-lg type="submit": Calculate & Save Profile]

          [card-footer text-muted text-center small]
            You can edit this at any time from the Profile page.
```

**Interactive behaviors:**
- On submit (POST `/Profile/Setup`): server validates all fields (ModelState). If invalid, re-renders the form with `.is-invalid` classes on failing inputs and text in the corresponding `.invalid-feedback` divs. No data is written.
- On success: creates `UserProfile` row, calculates targets, sets `TempData["Success"] = "Profile created! Here are your targets."`, redirects to `/Profile` (not `/MealLog`) so the user immediately sees their calculated numbers before diving in.

**Navigation:**
- No navbar shown on this page.
- Success → `/Profile` (which shows targets, then user can navigate to `/MealLog`).

**Validation feedback:**
- Height range error text changes based on selected unit: "Height must be between 24–120 in." or "Height must be between 61–305 cm." Set server-side via ModelState.
- Weight range error: "Weight must be between 50–1000 lbs." or "Weight must be between 23–454 kg."

---

### Page: `/Profile`

**Purpose:** Display the user's current health profile and their calculated TDEE, calorie target, and macro targets in grams.

**Layout sketch:**

```
[Navbar]
[Flash message banner (if TempData set)]

[container mt-4]
  [row mb-3]
    [col]
      [h2: My Health Profile]
      [a.btn.btn-outline-primary href="/Profile/Edit": Edit Profile]

  [row g-4]

    /* Left col: profile values */
    [col-md-6]
      [card]
        [card-header: Profile Details]
        [card-body]
          [dl.row.mb-0]
            [dt.col-sm-5: Age]            [dd.col-sm-7: {Age}]
            [dt.col-sm-5: Sex]            [dd.col-sm-7: {Sex}]
            [dt.col-sm-5: Height]         [dd.col-sm-7: {Height} {HeightUnit}]
            [dt.col-sm-5: Weight]         [dd.col-sm-7: {Weight} {WeightUnit}]
            [dt.col-sm-5: Activity Level] [dd.col-sm-7: {ActivityLevel display name}]
            [dt.col-sm-5: Goal]           [dd.col-sm-7: {Goal display name}]

    /* Right col: calculated targets */
    [col-md-6]
      [card border-primary]
        [card-header bg-primary text-white: Calculated Daily Targets]
        [card-body]
          [dl.row.mb-0]
            [dt.col-sm-6: TDEE]             [dd.col-sm-6: {TDEE} kcal]
            [dt.col-sm-6: Daily Target]     [dd.col-sm-6: [strong: {CalorieTarget} kcal]]
            [dt.col-sm-6: Protein]          [dd.col-sm-6: {ProteinTarget} g]
            [dt.col-sm-6: Carbohydrates]    [dd.col-sm-6: {CarbsTarget} g]
            [dt.col-sm-6: Fat]              [dd.col-sm-6: {FatTarget} g]

          [hr]
          [small.text-muted:
            Targets calculated using the Mifflin-St Jeor equation.
            These are estimates — not medical advice.]

  [row mt-3]
    [col]
      [a.btn.btn-link href="/MealLog": ← Back to Meal Log]
```

**Interactive behaviors:**
- "Edit Profile" link navigates to `/Profile/Edit`.
- No form on this page — read-only view.

**Navigation:**
- Reached from: `/Profile/Setup` redirect, Navbar "Profile" link, `/Profile/Edit` save redirect.
- Links out to: `/Profile/Edit`, `/MealLog`.

**Empty states:** N/A — this page is only reachable when a profile exists (middleware ensures it).

---

### Page: `/Profile/Edit`

**Purpose:** Allow the user to update their health profile; recalculates targets on save.

**Layout sketch:**

```
[Navbar]

[container mt-4]
  [row mb-3]
    [col]
      [h2: Edit Health Profile]

  [row justify-content-center]
    [col-12 col-md-8 col-lg-6]
      [card shadow-sm]
        [card-body p-4]

          [form method="post" action="/Profile/Edit"]
            [antiforgery token]
            /* Identical field layout to /Profile/Setup, but all inputs pre-populated
               with current profile values */
            [row g-3 mb-3]
              [col-md-6]
                [label: Age]
                [input#Age.form-control type="number" name="Age" value="{Age}" ...]
                [div.invalid-feedback]
              [col-md-6]
                [label: Sex]
                [select#Sex.form-select name="Sex" (current value selected)]
                [div.invalid-feedback]
            [row g-3 mb-3]
              [col-md-6]
                [label: Height]
                [input#Height.form-control value="{Height}" ...]
                [div.invalid-feedback]
              [col-md-6]
                [label: Unit]
                [select#HeightUnit (current value selected)]
            [row g-3 mb-3]
              [col-md-6]
                [label: Weight]
                [input#Weight.form-control value="{Weight}" ...]
              [col-md-6]
                [label: Unit]
                [select#WeightUnit (current value selected)]
            [row g-3 mb-3]
              [col-12]
                [label: Activity Level]
                [select#ActivityLevel (current value selected)]
            [row g-3 mb-4]
              [col-12]
                [label: Goal]
                [select#Goal (current value selected)]

            [row g-2]
              [col-sm-8]
                [button.btn.btn-primary.w-100 type="submit": Save Changes]
              [col-sm-4]
                [a.btn.btn-outline-secondary.w-100 href="/Profile": Cancel]
```

**Interactive behaviors:**
- On POST to `/Profile/Edit`: validate all fields. If invalid, re-render with `.is-invalid` + `.invalid-feedback`.
- On success: recalculate and store targets, set `TempData["Success"] = "Profile updated successfully."`, redirect to `/Profile`.
- "Cancel" link returns to `/Profile` without saving.

**Validation feedback:** Same rules as `/Profile/Setup`.

---

### Page: `/MealLog`

**Purpose:** Home page showing today's nutrition progress, all logged meals, a quick log panel for saved recipes, and a manual log form.

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]

  /* ── Page Header ── */
  [row align-items-center mb-3]
    [col-md-8]
      [h2.mb-0: Meal Log]
    [col-md-4 text-md-end mt-2 mt-md-0]
      [form method="get" action="/MealLog" class="d-flex gap-2 justify-content-md-end"]
        [label.form-label.mb-0.align-self-center for="date": Date:]
        [input#date.form-control type="date" name="date"
          value="{selectedDate:yyyy-MM-dd}"
          max="{today:yyyy-MM-dd}"
          min="{7DaysAgo:yyyy-MM-dd}"
          style="width:160px"]
        [button.btn.btn-outline-secondary type="submit": Go]

  [h5.text-muted: {selectedDate displayed as "Monday, March 18, 2026"} (or "Today" if selected date = today)]

  /* ── Daily Progress Summary ── */
  [row mb-4]
    [col-12]
      [card]
        [card-header: Daily Nutrition Progress]
        [card-body]

          /* Calories row */
          [div.mb-3]
            [div.d-flex.justify-content-between.mb-1]
              [span: [strong: Calories]]
              [span: {consumed} / {target} kcal]
            [div.progress style="height:20px"]
              [div.progress-bar.{bg-success|bg-danger} role="progressbar"
                style="width:{pct}%" (capped at 100 for width)
                aria-valuenow="{consumed}" aria-valuemin="0" aria-valuemax="{target}"]

          /* Protein row */
          [div.mb-3]
            [div.d-flex.justify-content-between.mb-1]
              [span: [strong: Protein]]
              [span: {consumed}g / {target}g]
            [div.progress style="height:14px"]
              [div.progress-bar.bg-info.{or bg-danger} ...]

          /* Carbs row */
          [div.mb-3]
            [div.d-flex.justify-content-between.mb-1]
              [span: [strong: Carbs]]
              [span: {consumed}g / {target}g]
            [div.progress style="height:14px"]
              [div.progress-bar.bg-warning.{or bg-danger} ...]

          /* Fat row */
          [div.mb-0]
            [div.d-flex.justify-content-between.mb-1]
              [span: [strong: Fat]]
              [span: {consumed}g / {target}g]
            [div.progress style="height:14px"]
              [div.progress-bar.bg-secondary.{or bg-danger} ...]

  /* ── Two-column layout ── */
  [row g-4]

    /* ── Left column: Today's entries + Manual Log ── */
    [col-lg-8]

      /* Today's Meals */
      [card mb-4]
        [card-header.d-flex.justify-content-between.align-items-center]
          [span: Today's Meals]
          [small.text-muted: {totalEntries} entries · {totalCal} kcal]
        [card-body p-0]

          /* EMPTY STATE — shown when no entries for selected date */
          [IF no entries]
            [div.text-center.py-5.text-muted]
              [p.mb-2: No meals logged for this day.]
              [a.btn.btn-sm.btn-outline-primary href="/Recipes": Browse Recipes]

          /* ENTRY LIST — shown when entries exist */
          [ELSE]
            [ul.list-group.list-group-flush]
              [FOR each entry]
                [li.list-group-item.d-flex.justify-content-between.align-items-center]

                  [div]
                    [strong: {RecipeName}]
                    [br]
                    [span.badge.bg-secondary.me-1: {MealType}]
                    [small.text-muted: {Servings} serving(s) · logged at {LoggedAt local time HH:mm}]

                  [div.d-flex.align-items-center.gap-3]
                    [span.fw-semibold: {totalCal for entry} kcal]
                    [form method="post" action="/MealLog/Delete/{entry.Id}"]
                      [antiforgery token]
                      [button.btn.btn-sm.btn-outline-danger
                        onclick="return confirm('Delete {RecipeName}?')": Delete]

      /* Manual Log Form */
      [card]
        [card-header: Log a Meal Manually]
        [card-body]
          [form method="post" action="/MealLog/LogManual"]
            [antiforgery token]
            [row g-3]
              [col-md-6]
                [label.form-label: Recipe / Meal Name]
                [input.form-control name="RecipeName" required maxlength="200"
                  placeholder="e.g. Chicken and Rice"]
                [div.invalid-feedback: Name is required.]
              [col-md-6]
                [label.form-label: Meal Type]
                [select.form-select name="MealType" required]
                  [option value="" disabled selected: — Select —]
                  [option value="Breakfast": Breakfast]
                  [option value="Lunch": Lunch]
                  [option value="Dinner": Dinner]
                  [option value="Snack": Snack]
                [div.invalid-feedback: Please select a meal type.]
            [row g-3 mt-1]
              [col-6 col-md-2]
                [label.form-label: Servings]
                [input.form-control type="number" name="Servings"
                  min="0.25" max="20" step="0.25" required value="1"]
                [div.invalid-feedback: Min 0.25.]
              [col-6 col-md-2]
                [label.form-label: Calories]
                [input.form-control type="number" name="CaloriesPerServing"
                  min="0" required placeholder="kcal"]
                [div.invalid-feedback: Required, ≥ 0.]
              [col-4 col-md-3]
                [label.form-label: Protein (g)]
                [input.form-control type="number" name="ProteinPerServing"
                  min="0" step="0.1" required]
                [div.invalid-feedback: Required, ≥ 0.]
              [col-4 col-md-3]
                [label.form-label: Carbs (g)]
                [input.form-control type="number" name="CarbsPerServing"
                  min="0" step="0.1" required]
                [div.invalid-feedback: Required, ≥ 0.]
              [col-4 col-md-2]
                [label.form-label: Fat (g)]
                [input.form-control type="number" name="FatPerServing"
                  min="0" step="0.1" required]
                [div.invalid-feedback: Required, ≥ 0.]
            [row mt-3]
              [col]
                [button.btn.btn-primary type="submit": Log Meal]

    /* ── Right column: Quick Log + Quick Actions ── */
    [col-lg-4]

      /* Quick Log */
      [card mb-4]
        [card-header: Quick Log]
        [card-body p-0]

          /* EMPTY STATE */
          [IF no saved recipes]
            [div.text-center.py-4.px-3.text-muted]
              [p.mb-2: No saved recipes yet.]
              [a.btn.btn-sm.btn-outline-primary href="/Recipes": Browse Recipes]

          /* LIST */
          [ELSE]
            [ul.list-group.list-group-flush]
              [FOR each saved recipe]
                [li.list-group-item.d-flex.justify-content-between.align-items-center]
                  [div]
                    [div.fw-semibold: {RecipeName}]
                    [small.text-muted: {CaloriesPerServing} kcal/serving]
                  [button.btn.btn-sm.btn-primary
                    data-bs-toggle="modal"
                    data-bs-target="#quickLogModal"
                    data-recipe-id="{id}"
                    data-recipe-name="{name}"
                    data-default-servings="{defaultServings}"
                    data-calories="{cal}"
                    data-protein="{prot}"
                    data-carbs="{carbs}"
                    data-fat="{fat}": Log]

      /* Quick Actions */
      [card]
        [card-header: Quick Actions]
        [list-group.list-group-flush]
          [a.list-group-item.list-group-item-action href="/Recipes":
            Browse Recipes →]
          [a.list-group-item.list-group-item-action href="/Pantry":
            My Pantry →]
          [a.list-group-item.list-group-item-action href="/ShoppingList":
            Shopping List →]
          [a.list-group-item.list-group-item-action href="/SavedRecipes":
            Saved Recipes →]

/* ── Quick Log Modal ── */
[div#quickLogModal.modal.fade tabindex="-1" aria-labelledby="quickLogModalLabel" aria-hidden="true"]
  [div.modal-dialog.modal-sm]
    [div.modal-content]
      [div.modal-header]
        [h5.modal-title#quickLogModalLabel: Log — {recipe name filled by JS}]
        [button.btn-close data-bs-dismiss="modal"]
      [div.modal-body]
        [form#quickLogForm method="post" action="/MealLog/LogRecipe"]
          [antiforgery token]
          [input type="hidden" name="RecipeId" id="qlRecipeId"]
          [div.mb-3]
            [label.form-label: Servings]
            [input#qlServings.form-control type="number"
              name="Servings" min="0.25" max="20" step="0.25" required]
            [div.invalid-feedback: Minimum 0.25 servings.]
          [div.mb-3]
            [label.form-label: Meal Type]
            [select#qlMealType.form-select name="MealType" required]
              [option value="Breakfast": Breakfast]
              [option value="Lunch": Lunch]
              [option value="Dinner": Dinner]
              [option value="Snack": Snack]
            [div.invalid-feedback: Please select a meal type.]
      [div.modal-footer]
        [button.btn.btn-secondary data-bs-dismiss="modal": Cancel]
        [button.btn.btn-primary form="quickLogForm" type="submit": Confirm Log]
```

**Interactive behaviors:**

1. **Date picker:** Changing the date and clicking "Go" submits a GET to `/MealLog?date=YYYY-MM-DD`. Page reloads with entries and progress bars for that date. The `min` attribute is `today - 6 days`; `max` is today — browser prevents selecting outside range.

2. **Progress bars:** Rendered server-side. Width = `Math.Min(100, (int)((consumed / target) * 100))`. Class is `bg-danger` when `consumed > target`, otherwise the per-macro color (see sketch). The numeric label always shows the raw `consumed / target` (not capped).

3. **Delete entry:** Each Delete button is inside a `<form method="post">` so no JS is required for the POST. The `onclick="return confirm(...)"` prevents the submit if cancelled. On success: server sets `TempData["Success"]`, redirects back to `/MealLog?date=...`.

4. **Quick Log modal:** When a "Log" button is clicked, a JS event listener reads all `data-*` attributes from the button and populates the modal: sets `#qlRecipeId` value, sets `#quickLogModalLabel` text to the recipe name, sets `#qlServings` value to the default servings, and pre-selects meal type by time of day (Before 10 AM → Breakfast; 10 AM–1:59 PM → Lunch; 2–4:59 PM → Snack; 5 PM+ → Dinner). The modal then shows. On form submit: POST `/MealLog/LogRecipe`. On success: redirect to `/MealLog` with updated totals.

**Navigation:**
- Reached from: any page via navbar "Meal Log", redirect after profile setup.
- Links out to: `/Recipes`, `/Pantry`, `/ShoppingList`, `/SavedRecipes`, `/MealLog?date=X`.

**Empty states:**
- No meals logged: "No meals logged for this day." + Browse Recipes button.
- No saved recipes in Quick Log: "No saved recipes yet." + Browse Recipes button.
- No profile: middleware redirects to `/Profile/Setup` before this page renders.

---

### Page: `/Pantry`

**Purpose:** View, add, edit, and delete pantry ingredients grouped by the fixed category order.

**Category display order (fixed):** Produce → Protein → Dairy → Grains → Pantry Staples → Frozen → Other

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]
  [row mb-3]
    [col]
      [h2: My Pantry]

  [row g-4]

    /* ── Left: Add Ingredient Form ── */
    [col-md-4 col-lg-3]
      [card position-sticky top (top: 80px to clear navbar)]
        [card-header: Add Ingredient]
        [card-body]
          [form method="post" action="/Pantry/Add"]
            [antiforgery token]
            [div.mb-3]
              [label.form-label: Name]
              [input.form-control name="Name" required maxlength="200"
                placeholder="e.g. spinach"]
              [div.invalid-feedback: {server error — e.g. "spinach already exists in your pantry."}]
            [div.mb-3]
              [label.form-label: Quantity]
              [input.form-control name="Quantity" required maxlength="100"
                placeholder="e.g. 2 cups"]
              [div.invalid-feedback: Quantity is required.]
            [div.mb-3]
              [label.form-label: Category]
              [select.form-select name="Category" required]
                [option value="" disabled selected: — Select —]
                [option value="Produce":        Produce]
                [option value="Protein":        Protein]
                [option value="Dairy":          Dairy]
                [option value="Grains":         Grains]
                [option value="PantryStaples":  Pantry Staples]
                [option value="Frozen":         Frozen]
                [option value="Other":          Other]
              [div.invalid-feedback: Please select a category.]
            [button.btn.btn-primary.w-100 type="submit": Add to Pantry]

    /* ── Right: Grouped Ingredient List ── */
    [col-md-8 col-lg-9]

      /* EMPTY STATE — no pantry items at all */
      [IF pantry is empty]
        [div.text-center.py-5.text-muted]
          [h5: Your pantry is empty.]
          [p: Add ingredients using the form to get started.]

      /* GROUPED LIST — iterate categories in fixed order, skip empty categories */
      [ELSE]
        [FOR each category that has items]
          [h5.mt-3.mb-2.text-uppercase.text-muted.small.fw-bold.border-bottom.pb-1:
            {Category display name}]
          [ul.list-group.mb-3]
            [FOR each ingredient in category, sorted alphabetically by name]
              [li.list-group-item]

                /* VIEW STATE — default */
                [div.d-flex.justify-content-between.align-items-center
                  id="pantry-view-{id}"]
                  [div]
                    [strong: {Name}]
                    [span.text-muted.ms-2: {Quantity}]
                  [div.btn-group.btn-group-sm]
                    [button.btn.btn-outline-secondary
                      onclick="showPantryEdit({id})": Edit]
                    [form method="post" action="/Pantry/Delete/{id}" class="d-inline"]
                      [antiforgery token]
                      [button.btn.btn-outline-danger
                        onclick="return confirm('Remove {Name} from pantry?')": Delete]

                /* EDIT STATE — hidden by default, toggled by JS */
                [form method="post" action="/Pantry/Edit/{id}"
                  id="pantry-edit-{id}" class="d-none mt-2"]
                  [antiforgery token]
                  [row g-2]
                    [col-sm-4]
                      [input.form-control.form-control-sm name="Name"
                        value="{Name}" required maxlength="200"]
                      [div.invalid-feedback: {server error if duplicate name}]
                    [col-sm-3]
                      [input.form-control.form-control-sm name="Quantity"
                        value="{Quantity}" required maxlength="100"]
                    [col-sm-3]
                      [select.form-select.form-select-sm name="Category" required]
                        [... same options as add form, current value selected ...]
                    [col-sm-2]
                      [button.btn.btn-sm.btn-primary type="submit": Save]
                      [button.btn.btn-sm.btn-link type="button"
                        onclick="hidePantryEdit({id})": Cancel]
```

**Interactive behaviors:**

1. **Add:** POST `/Pantry/Add`. If name already exists (case-insensitive): server returns the page with ModelState error; input gets `.is-invalid`; `.invalid-feedback` shows "'{name}' is already in your pantry." On success: redirect to `/Pantry` with TempData["Success"] = "Ingredient added."

2. **Inline Edit:** Clicking "Edit" calls `showPantryEdit(id)` (vanilla JS) which hides `#pantry-view-{id}` (adds `d-none`) and shows `#pantry-edit-{id}` (removes `d-none`). "Cancel" reverses. On POST to `/Pantry/Edit/{id}`: if name conflict with another item, server returns page with the edit form expanded and `.is-invalid` on the Name input. On success: redirect to `/Pantry`.

3. **Delete:** Inline `<form>` with `confirm()`. POST `/Pantry/Delete/{id}`. Redirect to `/Pantry`. If the category now has no items, it is simply absent from the re-rendered list.

**Navigation:**
- Reached from: Navbar "Pantry", Quick Actions on MealLog.

**Validation feedback:**
- Duplicate name: `.is-invalid` on Name field, `.invalid-feedback` text: "'{Name}' is already in your pantry."
- Empty fields: standard `.is-invalid` + `.invalid-feedback`.

---

### Page: `/ShoppingList`

**Purpose:** Manage a shopping list; check off purchased items; move purchased items to the pantry.

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]

  /* ── Page Header ── */
  [row align-items-center mb-3]
    [col]
      [h2: Shopping List]
    [col-auto]
      /* Only rendered if at least one IsPurchased = true item exists */
      [IF any purchased items]
        [form method="post" action="/ShoppingList/ClearPurchased" class="d-inline"]
          [antiforgery token]
          [button.btn.btn-outline-danger
            onclick="return confirm('Remove all {n} purchased items?')":
            Clear Purchased ({n})]

  [row g-4]

    /* ── Left: Add Item Form ── */
    [col-md-4 col-lg-3]
      [card position-sticky (top: 80px)]
        [card-header: Add Item]
        [card-body]
          [form method="post" action="/ShoppingList/Add"]
            [antiforgery token]
            [div.mb-3]
              [label.form-label: Name]
              [input.form-control name="Name" required maxlength="200"
                placeholder="e.g. chicken breast"]
              [div.invalid-feedback: Name is required.]
            [div.mb-3]
              [label.form-label: Quantity]
              [input.form-control name="Quantity" required maxlength="100"
                placeholder="e.g. 2 lbs"]
              [div.invalid-feedback: Quantity is required.]
            [div.mb-3]
              [label.form-label: Category]
              [select.form-select name="Category" required]
                [... same 7 options as Pantry add form ...]
              [div.invalid-feedback: Please select a category.]
            [button.btn.btn-primary.w-100 type="submit": Add to List]

    /* ── Right: Grouped Shopping List ── */
    [col-md-8 col-lg-9]

      /* EMPTY STATE */
      [IF list is empty]
        [div.text-center.py-5.text-muted]
          [h5: Your shopping list is empty.]
          [p: Add items manually or use "Add Missing Ingredients" on a recipe page.]
          [a.btn.btn-outline-primary href="/Recipes": Browse Recipes]

      /* GROUPED LIST */
      [ELSE]
        [FOR each category with items, fixed order]
          [h5.mt-3.mb-2.text-uppercase.text-muted.small.fw-bold.border-bottom.pb-1:
            {Category display name}]

          /* Unpurchased items first, then purchased */
          [ul.list-group.mb-3]
            [FOR each unpurchased item in category, sorted alpha by name]
              [li.list-group-item.d-flex.justify-content-between.align-items-center
                id="sl-item-{id}"]
                [div.d-flex.align-items-center.gap-3]
                  [input.form-check-input type="checkbox"
                    id="check-{id}"
                    onchange="markPurchased({id}, this)"]
                  [label.form-check-label for="check-{id}"]
                    [strong: {Name}]
                    [span.text-muted.ms-2: {Quantity}]
                [div.btn-group.btn-group-sm]
                  [form method="post" action="/ShoppingList/Delete/{id}"]
                    [antiforgery token]
                    [button.btn.btn-sm.btn-outline-danger
                      onclick="return confirm('Remove {Name}?')": Delete]

            [FOR each purchased item in category, sorted alpha by name]
              [li.list-group-item.d-flex.justify-content-between.align-items-center
                id="sl-item-{id}" class="opacity-50"]
                [div.d-flex.align-items-center.gap-3]
                  [input.form-check-input type="checkbox" checked disabled
                    id="check-{id}"]
                  [label.form-check-label.text-decoration-line-through for="check-{id}"]
                    [strong: {Name}]
                    [span.text-muted.ms-2: {Quantity}]
                [div.d-flex.gap-1]
                  [form method="post" action="/ShoppingList/MoveToPantry/{id}"]
                    [antiforgery token]
                    [button.btn.btn-sm.btn-outline-success: Add to Pantry]
                  [form method="post" action="/ShoppingList/Delete/{id}"]
                    [antiforgery token]
                    [button.btn.btn-sm.btn-outline-danger
                      onclick="return confirm('Remove {Name}?')": Delete]
```

**Interactive behaviors:**

1. **Check off item (mark purchased):** The checkbox `onchange` fires `markPurchased(id, checkbox)`.
   ```js
   function markPurchased(id, checkbox) {
     fetch('/ShoppingList/MarkPurchased/' + id, {
       method: 'POST',
       headers: { 'RequestVerificationToken': getAntiForgeryToken() }
     }).then(r => {
       if (r.ok) {
         const li = document.getElementById('sl-item-' + id);
         // Add strikethrough and opacity
         li.classList.add('opacity-50');
         const label = li.querySelector('label');
         label.classList.add('text-decoration-line-through');
         // Disable the checkbox (prevent unchecking)
         checkbox.disabled = true;
         // Show "Add to Pantry" button (swap delete-only controls for purchased controls)
         // Simplest approach: reload the section — or swap innerHTML
         // Recommended: full page reload via window.location.reload() for simplicity
         window.location.reload();
       }
     });
   }
   ```
   > **Developer note:** The POST to `/ShoppingList/MarkPurchased/{id}` must accept the anti-forgery token in both form-body and as a request header (`X-CSRF-Token` or `RequestVerificationToken`). Use `[ValidateAntiForgeryToken]` or `[IgnoreAntiforgeryToken]` + manual validation as appropriate for AJAX handlers. The simplest implementation is a full page reload after the fetch succeeds — this avoids complex DOM surgery.

2. **Add to Pantry:** POST to `/ShoppingList/MoveToPantry/{id}`.
   - If name does NOT exist in pantry: creates `PantryItem` (name, quantity, category), deletes `ShoppingListItem`, redirects to `/ShoppingList` with TempData["Success"] = "Added '{name}' to your pantry."
   - If name DOES exist in pantry: redirects to `/ShoppingList` with TempData["Error"] = "'{name}' is already in your pantry. Remove it from the pantry first, or delete this shopping list item." No data is changed.

3. **Clear Purchased:** `confirm()` shows count. POST `/ShoppingList/ClearPurchased`. Bulk deletes all IsPurchased rows. Redirect to `/ShoppingList`. The button is only rendered when `Model.Items.Any(i => i.IsPurchased)`.

**Navigation:**
- Reached from: Navbar "Shopping", Quick Actions on MealLog, Recipe detail "Add Missing Ingredients."

---

### Page: `/Recipes`

**Purpose:** Browse all recipes in a filterable, responsive card grid.

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]
  [row mb-3]
    [col]
      [h2: Recipe Browser]

  /* ── Filter Bar ── */
  [row mb-4]
    [col-12]
      [card]
        [card-body]
          [form method="get" action="/Recipes" id="filterForm"]

            [row g-3 align-items-end]

              /* Meal Type checkboxes */
              [col-md-5]
                [label.form-label.fw-semibold: Meal Type]
                [div.d-flex.flex-wrap.gap-3]
                  [div.form-check]
                    [input.form-check-input type="checkbox" name="mealTypes"
                      value="All" id="mt-all" {checked if no specific type selected}]
                    [label.form-check-label for="mt-all": All]
                  [div.form-check]
                    [input.form-check-input type="checkbox" name="mealTypes"
                      value="Breakfast" id="mt-breakfast" {checked per time-of-day logic}]
                    [label.form-check-label for="mt-breakfast": Breakfast]
                  [div.form-check]
                    [input.form-check-input type="checkbox" name="mealTypes"
                      value="Lunch" id="mt-lunch"]
                    [label.form-check-label for="mt-lunch": Lunch]
                  [div.form-check]
                    [input.form-check-input type="checkbox" name="mealTypes"
                      value="Dinner" id="mt-dinner"]
                    [label.form-check-label for="mt-dinner": Dinner]
                  [div.form-check]
                    [input.form-check-input type="checkbox" name="mealTypes"
                      value="Snack" id="mt-snack"]
                    [label.form-check-label for="mt-snack": Snack]

              /* Max Prep Time */
              [col-md-3]
                [label.form-label.fw-semibold for="maxPrepTime": Max Prep Time]
                [select#maxPrepTime.form-select name="maxPrepTime"]
                  [option value="": Any]
                  [option value="15": ≤ 15 min]
                  [option value="30": ≤ 30 min]
                  [option value="45": ≤ 45 min]
                  [option value="60": ≤ 60 min]

              /* Min % Ingredients Owned */
              [col-md-3]
                [label.form-label.fw-semibold for="minOwned": Ingredients Owned]
                [select#minOwned.form-select name="minOwned"]
                  [option value="": Any]
                  [option value="25": ≥ 25%]
                  [option value="50": ≥ 50%]
                  [option value="75": ≥ 75%]
                  [option value="100": 100% (all owned)]

              /* Submit */
              [col-md-1 d-flex align-items-end]
                [button.btn.btn-primary.w-100 type="submit": Filter]

  /* ── Recipe Grid ── */
  [row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4]

    /* EMPTY STATE — shown when no recipes match filters */
    [IF no recipes]
      [col-12]
        [div.text-center.py-5.text-muted]
          [h5: No recipes match your filters.]
          [p: Try removing some filters.]
          [a.btn.btn-outline-secondary href="/Recipes": Clear Filters]

    /* RECIPE CARDS */
    [FOR each recipe in filtered list, sorted by name ASC]
      [col]
        [div.card.h-100.shadow-sm]
          [div.card-body]
            [h5.card-title: {Name}]
            [div.mb-2]
              [FOR each meal type in recipe.MealTypes]
                [span.badge.bg-primary.me-1: {MealType}]
            [ul.list-unstyled.text-muted.small.mb-0]
              [li: ⏱ {PrepTimeMinutes} min prep]
              [li: 🔥 {CaloriesPerServing} kcal / serving]
              [li]
                /* Ingredient ownership badge */
                [IF ownershipPct == 100]
                  [span.badge.bg-success: ✓ All ingredients owned]
                [ELSE IF ownershipPct >= 50]
                  [span.badge.bg-warning.text-dark: {ownershipPct}% ingredients owned]
                [ELSE]
                  [span.badge.bg-secondary: {ownershipPct}% ingredients owned]
          [div.card-footer bg-transparent border-top-0]
            [a.btn.btn-outline-primary.w-100 href="/Recipes/{id}": View Recipe]
```

**Interactive behaviors:**

1. **"All" checkbox logic (vanilla JS):** When "All" is checked, uncheck and disable all individual type checkboxes. When any individual type is checked, uncheck "All". Wire via `change` event listeners on all meal-type checkboxes on `DOMContentLoaded`.

2. **Filter submission:** The `<form method="get">` submits all checked `mealTypes` values as repeated query params (e.g., `?mealTypes=Breakfast&mealTypes=Lunch&maxPrepTime=30`). Server reads `Request.Query["mealTypes"]` as a `StringValues` and filters accordingly.

3. **Filter persistence:** Server pre-selects controls from query string values when re-rendering. If no query string is present (fresh page load), server applies time-of-day default for meal type.

**Navigation:**
- Reached from: Navbar "Recipes", Quick Actions on MealLog.
- Links out to: `/Recipes/{id}`.

---

### Page: `/Recipes/{id}`

**Purpose:** Display full recipe details with servings scaling, and provide Save, Log, and Add-Missing actions.

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]

  /* Breadcrumb */
  [nav aria-label="breadcrumb"]
    [ol.breadcrumb]
      [li.breadcrumb-item: [a href="/MealLog": Home]]
      [li.breadcrumb-item: [a href="/Recipes": Recipes]]
      [li.breadcrumb-item.active aria-current="page": {Recipe Name}]

  [row g-4]

    /* ── Left: Recipe Content ── */
    [col-lg-8]

      [h1.mb-1: {Name}]
      [div.mb-2]
        [FOR each meal type]
          [span.badge.bg-primary.me-1: {MealType}]
        [span.badge.bg-light.text-dark.border.me-1: ⏱ {PrepTimeMinutes} min]
      [p.lead.text-muted: {Description}]
      [hr]

      /* Servings Control */
      [div.card.mb-4]
        [div.card-body.py-2]
          [div.d-flex.align-items-center.gap-3]
            [label.form-label.mb-0.fw-semibold for="servingsInput": Servings:]
            [input#servingsInput.form-control type="number"
              min="1" value="{DefaultServings}" style="width:80px"
              oninput="scaleRecipe(this.value)"]
            [span#servingsNote.text-muted.small:
              (Base: {DefaultServings} serving(s))]

      /* Ingredients */
      [div.card.mb-4]
        [div.card-header: Ingredients]
        [ul#ingredientsList.list-group.list-group-flush]
          [FOR each ingredient]
            [li.list-group-item.d-flex.justify-content-between]
              [span: {Name}]
              [span.text-muted
                data-base-quantity="{numeric prefix}"
                data-unit="{unit suffix}"
                data-original="{full original quantity string}"
                id="qty-{index}": {Quantity}]

      /* Instructions */
      [div.card.mb-4]
        [div.card-header: Instructions]
        [div.card-body]
          [ol]
            [FOR each step]
              [li.mb-2: {Step text}]

      /* Macros per Serving */
      [div.card.mb-4]
        [div.card-header]
          [span: Nutrition Info]
          [span.text-muted.small.float-end: per serving · [span#servingCount: {DefaultServings}] total serving(s)]
        [div.card-body]
          [div.row.text-center.g-3]
            [div.col-6.col-md-3]
              [div.display-6.fw-bold#totalCalories: {CaloriesPerServing × DefaultServings}]
              [div.text-muted.small: kcal total]
            [div.col-6.col-md-3]
              [div.display-6.fw-bold#totalProtein: {ProteinPerServing × DefaultServings}g]
              [div.text-muted.small: protein]
            [div.col-6.col-md-3]
              [div.display-6.fw-bold#totalCarbs: {CarbsPerServing × DefaultServings}g]
              [div.text-muted.small: carbs]
            [div.col-6.col-md-3]
              [div.display-6.fw-bold#totalFat: {FatPerServing × DefaultServings}g]
              [div.text-muted.small: fat]
          [p.text-muted.small.mt-2.mb-0:
            Per serving: {CaloriesPerServing} kcal ·
            P: {ProteinPerServing}g ·
            C: {CarbsPerServing}g ·
            F: {FatPerServing}g]

    /* ── Right: Action Sidebar ── */
    [col-lg-4]
      [div.sticky-top style="top:80px"]

        /* Ingredient Ownership */
        [div.card.mb-3]
          [div.card-body.text-center]
            [IF ownershipPct == 100]
              [span.badge.bg-success.fs-6: ✓ All ingredients in pantry]
            [ELSE]
              [p.mb-1.text-muted.small: Pantry coverage]
              [div.progress.mb-1]
                [div.progress-bar.bg-success style="width:{ownershipPct}%"]
              [p.mb-0.small: {ownedCount} of {totalCount} ingredients owned ({ownershipPct}%)]

        /* Action Buttons */
        [div.card]
          [div.card-header: Actions]
          [div.card-body.d-grid.gap-2]

            /* Save Recipe Button */
            [IF already saved]
              [button.btn.btn-success disabled: ✓ Saved]
            [ELSE]
              [form method="post" action="/Recipes/Save/{id}"]
                [antiforgery token]
                [button.btn.btn-outline-primary.w-100 type="submit": Save Recipe]

            /* Log This Meal Button */
            [button.btn.btn-primary.w-100
              data-bs-toggle="modal"
              data-bs-target="#logMealModal": Log This Meal]

            /* Add Missing Ingredients Button */
            [IF ownershipPct < 100]
              [form method="post" action="/Recipes/AddMissing/{id}"]
                [antiforgery token]
                [input type="hidden" name="Servings" id="hiddenServings"
                  value="{DefaultServings}"]
                [button.btn.btn-outline-secondary.w-100 type="submit":
                  Add Missing Ingredients to Shopping List]
            [ELSE]
              [button.btn.btn-outline-secondary.w-100 disabled:
                All Ingredients in Pantry]

        /* Back link */
        [div.mt-3]
          [a.btn.btn-link.ps-0 href="/Recipes": ← Back to Recipes]

/* ── Log Meal Modal ── */
[div#logMealModal.modal.fade tabindex="-1"]
  [div.modal-dialog.modal-sm]
    [div.modal-content]
      [div.modal-header]
        [h5.modal-title: Log — {Recipe Name}]
        [button.btn-close data-bs-dismiss="modal"]
      [div.modal-body]
        [form#logMealForm method="post" action="/MealLog/LogRecipe"]
          [antiforgery token]
          [input type="hidden" name="RecipeId" value="{id}"]
          [input type="hidden" name="RecipeName" value="{Name}"]
          [input type="hidden" name="CaloriesPerServing" value="{CaloriesPerServing}"]
          [input type="hidden" name="ProteinPerServing" value="{ProteinPerServing}"]
          [input type="hidden" name="CarbsPerServing" value="{CarbsPerServing}"]
          [input type="hidden" name="FatPerServing" value="{FatPerServing}"]
          [div.mb-3]
            [label.form-label: Servings]
            [input#logServings.form-control type="number"
              name="Servings" min="0.25" step="0.25"
              value="{DefaultServings}" required]
            [div.invalid-feedback: Minimum 0.25.]
          [div.mb-3]
            [label.form-label: Meal Type]
            [select#logMealType.form-select name="MealType" required]
              [option value="Breakfast" {selected if time < 10}: Breakfast]
              [option value="Lunch"     {selected if 10–13}: Lunch]
              [option value="Dinner"    {selected if ≥17}: Dinner]
              [option value="Snack"     {selected if 14–16}: Snack]
            [div.invalid-feedback: Required.]
      [div.modal-footer]
        [button.btn.btn-secondary data-bs-dismiss="modal": Cancel]
        [button.btn.btn-primary form="logMealForm" type="submit": Confirm]
```

**Interactive behaviors:**

1. **Servings scaling (vanilla JS — `scaleRecipe(newServings)`):**
   ```
   function scaleRecipe(newVal) {
     const n = parseFloat(newVal);
     if (isNaN(n) || n < 1) return;
     const base = {DefaultServings}; // rendered as literal by Razor

     // Update ingredient quantities
     document.querySelectorAll('#ingredientsList [data-base-quantity]').forEach(el => {
       const baseQty = parseFloat(el.dataset.baseQuantity);
       const unit = el.dataset.unit;
       if (!isNaN(baseQty)) {
         const scaled = Math.round((baseQty * n / base) * 100) / 100;
         el.textContent = scaled + (unit ? ' ' + unit : '');
       }
       // If no numeric prefix (data-base-quantity is empty/NaN), leave textContent as data-original
     });

     // Update macro totals
     document.getElementById('totalCalories').textContent =
       Math.round({CaloriesPerServing} * n);  // Razor-rendered base values
     document.getElementById('totalProtein').textContent =
       (Math.round({ProteinPerServing} * n * 10) / 10) + 'g';
     document.getElementById('totalCarbs').textContent =
       (Math.round({CarbsPerServing} * n * 10) / 10) + 'g';
     document.getElementById('totalFat').textContent =
       (Math.round({FatPerServing} * n * 10) / 10) + 'g';

     // Update serving count label
     document.getElementById('servingCount').textContent = n;

     // Sync hidden servings input for "Add Missing" form
     document.getElementById('hiddenServings').value = n;
     // Sync modal servings input
     document.getElementById('logServings').value = n;
   }
   ```
   > Razor must render `data-base-quantity` as the extracted numeric prefix (e.g., `"2"` from `"2 cups"`) and `data-unit` as the remainder (e.g., `"cups"`). If quantity has no numeric prefix (e.g., `"to taste"`), set `data-base-quantity=""`. This extraction should happen server-side during page render.

2. **Save Recipe:** POST `/Recipes/Save/{id}`. On success: redirect back to `/Recipes/{id}` with TempData["Success"] = "Recipe saved!" and the Save button renders as disabled "✓ Saved".

3. **Add Missing Ingredients:** The hidden `Servings` input is kept in sync by `scaleRecipe()`. POST `/Recipes/AddMissing/{id}`. Redirect back to `/Recipes/{id}` with flash message.

4. **Log This Meal modal:** Modal servings input is kept in sync by `scaleRecipe()`. On submit, POST `/MealLog/LogRecipe`. On success: redirect to `/MealLog`.

**Navigation:**
- Reached from: Recipe Browser card "View Recipe", Saved Recipes card "View Recipe".
- Links out to: `/MealLog` (after Log), `/Recipes` (breadcrumb / Back link).

**Empty states:** N/A (404 returned for unknown recipe IDs).

---

### Page: `/SavedRecipes`

**Purpose:** View and manage the user's saved recipes; quick access to view or log them.

**Layout sketch:**

```
[Navbar]
[Flash message banner]

[container mt-4]
  [row mb-3 align-items-center]
    [col]
      [h2: Saved Recipes]
    [col-auto]
      [small.text-muted: {count} recipe(s) saved]

  /* EMPTY STATE */
  [IF no saved recipes]
    [row]
      [col-12]
        [div.text-center.py-5.text-muted]
          [h5: No saved recipes yet.]
          [p: Browse recipes and click "Save Recipe" to add them here.]
          [a.btn.btn-primary href="/Recipes": Browse Recipes]

  /* SAVED RECIPE GRID — same 3/2/1 col grid as /Recipes */
  [ELSE]
    [row.row-cols-1.row-cols-md-2.row-cols-lg-3.g-4]
      [FOR each saved recipe, sorted by SavedAt DESC (most recently saved first)]
        [col]
          [div.card.h-100.shadow-sm]
            [div.card-body]
              [h5.card-title: {Name}]
              [div.mb-2]
                [FOR each meal type]
                  [span.badge.bg-primary.me-1: {MealType}]
              [ul.list-unstyled.text-muted.small.mb-0]
                [li: ⏱ {PrepTimeMinutes} min]
                [li: 🔥 {CaloriesPerServing} kcal / serving]
                [li: Saved {SavedAt:MMM d, yyyy}]
            [div.card-footer.bg-transparent.border-top-0.d-flex.gap-2]
              [a.btn.btn-outline-primary.flex-grow-1 href="/Recipes/{id}": View Recipe]
              [form method="post" action="/SavedRecipes/Remove/{id}"]
                [antiforgery token]
                [button.btn.btn-outline-danger
                  onclick="return confirm('Remove {Name} from saved recipes?')": Remove]
```

**Interactive behaviors:**
- "Remove" button: POST `/SavedRecipes/Remove/{id}`. Deletes `SavedRecipe` row (does NOT delete the underlying `Recipe`). Redirect to `/SavedRecipes` with TempData["Success"] = "Recipe removed from saved list."

**Navigation:**
- Reached from: Navbar "Saved", Quick Actions on MealLog.
- Links out to: `/Recipes/{id}` for each card.

---

## Part 4 — User Flow Diagrams

---

### Flow 1: First-Time User Setup

```
┌─────────────────────────────────┐
│  User opens app for first time  │
└───────────────┬─────────────────┘
                │
                ▼
┌───────────────────────────────────────────┐
│  Middleware: Does UserProfile row exist?  │
└───────────────┬─────────────────┬─────────┘
                │ NO              │ YES
                ▼                 ▼
┌──────────────────────┐   ┌──────────────────────┐
│  Redirect to         │   │  Continue to          │
│  /Profile/Setup      │   │  requested page       │
└──────────┬───────────┘   └──────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────┐
│  /Profile/Setup                              │
│  User fills in: Age, Sex, Height + Unit,     │
│  Weight + Unit, Activity Level, Goal         │
└──────────┬───────────────────────────────────┘
           │
           ▼ POST /Profile/Setup
┌──────────────────────────────────┐
│  Server validates ModelState     │
└──────────┬──────────────┬────────┘
           │ INVALID       │ VALID
           ▼               ▼
┌──────────────────┐  ┌────────────────────────────────────┐
│  Re-render form  │  │  Create UserProfile row             │
│  with .is-invalid│  │  Calculate TDEE, targets, macros    │
│  error messages  │  │  TempData["Success"] = "Profile     │
└──────────────────┘  │  created! Here are your targets."  │
                       └─────────────────┬──────────────────┘
                                         │
                                         ▼ Redirect
                              ┌──────────────────────────┐
                              │  /Profile                 │
                              │  User sees their          │
                              │  calculated targets       │
                              └────────────┬─────────────┘
                                           │ Clicks "Back to Meal Log"
                                           ▼
                              ┌──────────────────────────┐
                              │  /MealLog                 │
                              │  Empty progress bars      │
                              │  show 0 / target          │
                              └──────────────────────────┘
```

---

### Flow 2: Browse → Filter → Select → Log a Meal

```
┌──────────────────────────────┐
│  User visits /Recipes        │
│  (via Navbar or Quick Action)│
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────┐
│  Page loads with time-of-day default filter pre-selected  │
│  (e.g., 12:30 PM → "Lunch" checkbox checked)              │
│  All 18 recipe cards displayed (or filtered subset)       │
└──────────────────────┬───────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│  User adjusts filters:                           │
│  - Checks "Dinner", unchecks "Lunch"            │
│  - Selects "≤ 30 min" from Max Prep Time        │
│  - Leaves Ingredients Owned at "Any"             │
│  Clicks "Filter" button                          │
└──────────────────────┬──────────────────────────┘
                        │ GET /Recipes?mealTypes=Dinner&maxPrepTime=30
                        ▼
┌──────────────────────────────────────────────────┐
│  Page reloads with filtered cards                 │
│  Filter controls reflect query string values      │
└──────────────────────┬───────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────┐
│  User clicks "View Recipe" on a card              │
└──────────────────────┬───────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────┐
│  /Recipes/{id}                                    │
│  Full recipe shown; Default servings in input     │
└────────┬──────────────────────────────────────────┘
         │
         ├──── User changes servings input ──────────────────────────────────────────┐
         │                                                                            │
         │                                                              scaleRecipe() fires
         │                                                              Quantities + macros
         │                                                              update live in DOM
         │◄───────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────┐
│  User clicks "Log This Meal"                      │
└──────────────────────┬───────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────┐
│  Modal opens:                                     │
│  - Servings (synced from page input)              │
│  - Meal Type (pre-selected by time of day)        │
│  - Confirm / Cancel                               │
└────────┬──────────────────────────────────────────┘
         │ Cancel                    │ Confirm
         ▼                           ▼
┌───────────────────┐   POST /MealLog/LogRecipe
│  Modal closes     │              │
│  Stay on /Recipes/│              ▼
│  {id}             │   ┌──────────────────────────────┐
└───────────────────┘   │  MealLogEntry created         │
                         │  Macros scaled by servings    │
                         │  Redirect to /MealLog         │
                         └──────────────────────────────┘
                                      │
                                      ▼
                         ┌──────────────────────────────┐
                         │  /MealLog                     │
                         │  New entry visible in list    │
                         │  Progress bars updated        │
                         └──────────────────────────────┘
```

---

### Flow 3: Browse → Select → Add Missing Ingredients to Shopping List

```
┌──────────────────────────────┐
│  User visits /Recipes/{id}   │
└──────────────┬───────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────┐
│  Pantry coverage shown in sidebar:                       │
│  e.g., "3 of 8 ingredients owned (37%)"                  │
└──────────────────────┬──────────────────────────────────┘
                        │
                        ▼ (optional)
┌──────────────────────────────────────────────────────────┐
│  User adjusts servings input (e.g., 4 instead of 2)      │
│  scaleRecipe() updates quantities in ingredient list      │
│  hiddenServings input is synced to "4"                    │
└──────────────────────┬───────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│  User clicks "Add Missing Ingredients to         │
│  Shopping List"                                  │
└──────────────────────┬──────────────────────────┘
                        │ POST /Recipes/AddMissing/{id}
                        │ Body: Servings=4
                        ▼
┌───────────────────────────────────────────────────────────────────┐
│  Server: for each RecipeIngredient                                 │
│    - Check pantry for matching name (case-insensitive)            │
│    - If NOT found: scale quantity by (4 / DefaultServings),       │
│      add ShoppingListItem (name, scaledQty, category=Other)       │
│    - If found: skip                                                │
└──────────────────────────────────────────┬────────────────────────┘
                                            │
                          ┌─────────────────┴─────────────────┐
                          │ n > 0                              │ n == 0
                          ▼                                    ▼
          ┌──────────────────────────────┐  ┌──────────────────────────────────┐
          │  Redirect to /Recipes/{id}   │  │  Redirect to /Recipes/{id}        │
          │  Flash: "Added 5 missing     │  │  Flash: "All ingredients are      │
          │  ingredient(s) to your       │  │  already in your pantry."          │
          │  shopping list."             │  └──────────────────────────────────┘
          └──────────────────────────────┘
```

---

### Flow 4: Shopping List → Check Off → Move to Pantry

```
┌──────────────────────────────────┐
│  User visits /ShoppingList        │
│  Sees unchecked items by category │
└──────────────┬────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────┐
│  User checks the checkbox on "chicken breast"     │
│  JS: markPurchased(42, checkbox)                  │
└──────────────────────┬───────────────────────────┘
                        │ fetch POST /ShoppingList/MarkPurchased/42
                        │ (anti-forgery token in header)
                        ▼
┌──────────────────────────────────────────────────┐
│  Server sets IsPurchased = true                   │
│  Returns 200 OK                                   │
└──────────────────────┬───────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────┐
│  JS: window.location.reload()                     │
│  Page refreshes; "chicken breast" now shows:      │
│  - Strikethrough text + 50% opacity               │
│  - Checkbox checked + disabled                    │
│  - "Add to Pantry" button visible                 │
│  - "Clear Purchased (1)" button in page header   │
└──────────────────────┬───────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────┐
│  User clicks "Add to Pantry"                      │
└──────────────────────┬───────────────────────────┘
                        │ POST /ShoppingList/MoveToPantry/42
                        ▼
┌───────────────────────────────────────────────────┐
│  Server: does "chicken breast" exist in pantry?   │
└───────────────────┬───────────────────────────────┘
                    │ NO                  │ YES
                    ▼                     ▼
  ┌─────────────────────────────┐  ┌──────────────────────────────────────┐
  │  Create PantryItem:          │  │  Redirect to /ShoppingList            │
  │  name, quantity, category    │  │  Flash error:                         │
  │  Delete ShoppingListItem     │  │  "'chicken breast' is already in your │
  │  Redirect to /ShoppingList   │  │  pantry. Remove it from the pantry   │
  │  Flash: "Added 'chicken      │  │  first, or delete this list item."   │
  │  breast' to your pantry."   │  └──────────────────────────────────────┘
  └─────────────────────────────┘
```

---

## Part 5 — Validation Rules Summary

| Page | Field | Rule | Error Message |
|------|-------|------|---------------|
| Profile Setup/Edit | Age | Integer 10–120, required | "Age must be between 10 and 120." |
| Profile Setup/Edit | Sex | Required enum | "Please select a sex." |
| Profile Setup/Edit | Height (in) | Decimal 24–120, required | "Height must be between 24 and 120 inches." |
| Profile Setup/Edit | Height (cm) | Decimal 61–305, required | "Height must be between 61 and 305 cm." |
| Profile Setup/Edit | Weight (lbs) | Decimal 50–1000, required | "Weight must be between 50 and 1000 lbs." |
| Profile Setup/Edit | Weight (kg) | Decimal 23–454, required | "Weight must be between 23 and 454 kg." |
| Profile Setup/Edit | ActivityLevel | Required enum | "Please select an activity level." |
| Profile Setup/Edit | Goal | Required enum | "Please select a goal." |
| MealLog Manual | RecipeName | Required, max 200 chars | "Name is required." |
| MealLog Manual | MealType | Required enum | "Please select a meal type." |
| MealLog Manual | Servings | Decimal ≥ 0.25, required | "Servings must be at least 0.25." |
| MealLog Manual | Calories | Integer ≥ 0, required | "Calories must be 0 or greater." |
| MealLog Manual | Protein/Carbs/Fat | Decimal ≥ 0, required | "{Field} must be 0 or greater." |
| Pantry Add/Edit | Name | Required, max 200, unique (case-insensitive) | "'{name}' is already in your pantry." |
| Pantry Add/Edit | Quantity | Required, max 100 | "Quantity is required." |
| Pantry Add/Edit | Category | Required enum | "Please select a category." |
| Shopping List Add | Name | Required, max 200 | "Name is required." |
| Shopping List Add | Quantity | Required, max 100 | "Quantity is required." |
| Shopping List Add | Category | Required enum | "Please select a category." |

All server-side validation errors are rendered by adding the `.is-invalid` CSS class to the input element and populating the sibling `.invalid-feedback` div with the error text. Forms must also use `asp-validation-for` tag helpers where applicable.
