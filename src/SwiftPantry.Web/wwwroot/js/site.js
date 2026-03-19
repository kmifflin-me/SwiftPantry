// SwiftPantry site.js

// ── Dark mode toggle ───────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    var btn  = document.getElementById('darkModeToggle');
    var icon = document.getElementById('darkModeIcon');
    if (!btn || !icon) return;

    var SUN_PATH  = '<path d="M8 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM8 0a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-1 0v-2A.5.5 0 0 1 8 0zm0 13a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-1 0v-2A.5.5 0 0 1 8 13zm8-5a.5.5 0 0 1-.5.5h-2a.5.5 0 0 1 0-1h2a.5.5 0 0 1 .5.5zM3 8a.5.5 0 0 1-.5.5h-2a.5.5 0 0 1 0-1h2A.5.5 0 0 1 3 8zm10.657-5.657a.5.5 0 0 1 0 .707l-1.414 1.415a.5.5 0 1 1-.707-.708l1.414-1.414a.5.5 0 0 1 .707 0zm-9.193 9.193a.5.5 0 0 1 0 .707L3.05 13.657a.5.5 0 0 1-.707-.707l1.414-1.414a.5.5 0 0 1 .707 0zm9.193 2.121a.5.5 0 0 1-.707 0l-1.414-1.414a.5.5 0 0 1 .707-.707l1.414 1.414a.5.5 0 0 1 0 .707zM4.464 4.465a.5.5 0 0 1-.707 0L2.343 3.05a.5.5 0 1 1 .707-.707l1.414 1.414a.5.5 0 0 1 0 .708z"/>';
    var MOON_PATH = '<path d="M6 .278a.768.768 0 0 1 .08.858 7.208 7.208 0 0 0-.878 3.46c0 4.021 3.278 7.277 7.318 7.277.527 0 1.04-.055 1.533-.16a.787.787 0 0 1 .81.316.733.733 0 0 1-.031.893A8.349 8.349 0 0 1 8.344 16C3.734 16 0 12.286 0 7.71 0 4.266 2.114 1.312 5.124.06A.752.752 0 0 1 6 .278z"/>';

    function isDark() {
        return document.documentElement.getAttribute('data-bs-theme') === 'dark';
    }

    function applyIcon() {
        icon.innerHTML = isDark() ? SUN_PATH : MOON_PATH;
        btn.title = isDark() ? 'Switch to light mode' : 'Switch to dark mode';
    }

    applyIcon();

    btn.addEventListener('click', function () {
        var next = isDark() ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('sp-theme', next);
        applyIcon();
    });
});

// ── Pantry inline edit toggle ──────────────────────────────────────────────
function showPantryEdit(id) {
    document.getElementById('pantry-view-' + id).style.display = 'none';
    document.getElementById('pantry-edit-form-' + id).style.display = 'block';
}

function cancelPantryEdit(id) {
    document.getElementById('pantry-view-' + id).style.display = '';
    document.getElementById('pantry-edit-form-' + id).style.display = 'none';
}

// ── Quick Log modal population ─────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    var quickLogModal = document.getElementById('quickLogModal');
    if (quickLogModal) {
        quickLogModal.addEventListener('show.bs.modal', function (event) {
            var btn = event.relatedTarget;
            if (!btn) return;

            var recipeId       = btn.getAttribute('data-recipe-id');
            var recipeName     = btn.getAttribute('data-recipe-name');
            var defaultSrv     = btn.getAttribute('data-default-servings');
            var mealType       = btn.getAttribute('data-meal-type') || getDefaultMealType();

            // Populate hidden fields and labels
            var idField = document.getElementById('qlRecipeId');
            if (idField) idField.value = recipeId;

            var nameField = document.getElementById('qlRecipeName');
            if (nameField) nameField.value = recipeName;

            var srvField = document.getElementById('qlServings');
            if (srvField) srvField.value = defaultSrv;

            var typeField = document.getElementById('qlMealType');
            if (typeField) typeField.value = capitalizeFirst(mealType);

            var titleEl = document.getElementById('quickLogModalLabel');
            if (titleEl) titleEl.textContent = 'Log — ' + recipeName;
        });
    }
});

function getDefaultMealType() {
    var h = new Date().getHours();
    if (h < 10)  return 'Breakfast';
    if (h < 14)  return 'Lunch';
    if (h < 17)  return 'Snack';
    return 'Dinner';
}

function capitalizeFirst(s) {
    if (!s) return s;
    return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
}

// ── Recipe detail: serving scaling ────────────────────────────────────────
(function () {
    var servingsInput = document.getElementById('servingsInput');
    if (!servingsInput) return;

    var defaultServings = parseFloat(servingsInput.getAttribute('data-default-servings') || servingsInput.value);

    // Cache original quantities
    var qtySpans = document.querySelectorAll('[data-original-qty]');

    function updateScaling() {
        var newServings = parseFloat(servingsInput.value);
        if (!newServings || newServings <= 0) newServings = defaultServings;

        var factor = newServings / defaultServings;

        // Update ingredient quantities
        qtySpans.forEach(function (span) {
            var original = span.getAttribute('data-original-qty');
            span.textContent = scaleQuantity(original, factor);
        });

        // Update macro totals
        updateMacroDisplay('totalCalories', 'data-base-calories', factor, 0);
        updateMacroDisplay('totalProtein',  'data-base-protein',  factor, 1);
        updateMacroDisplay('totalCarbs',    'data-base-carbs',    factor, 1);
        updateMacroDisplay('totalFat',      'data-base-fat',      factor, 1);

        // Sync hidden servings fields in any forms on the page
        document.querySelectorAll('.servings-sync').forEach(function (f) {
            f.value = newServings;
        });
    }

    function updateMacroDisplay(elId, attr, factor, decimals) {
        var el = document.getElementById(elId);
        if (!el) return;
        var base = parseFloat(el.getAttribute(attr) || '0');
        var scaled = base * factor;
        el.textContent = decimals === 0 ? Math.round(scaled).toLocaleString() : scaled.toFixed(decimals);
    }

    function scaleQuantity(qty, factor) {
        if (!qty) return qty;
        var m = qty.match(/^(\d+\.?\d*)\s*(.*)$/);
        if (!m) return qty;
        var num = parseFloat(m[1]);
        var unit = m[2] ? m[2].trim() : '';
        var scaled = (num * factor).toFixed(2);
        return unit ? scaled + ' ' + unit : scaled;
    }

    servingsInput.addEventListener('input', updateScaling);
    servingsInput.addEventListener('change', updateScaling);
})();

// ── Shopping list checkbox (mark/unmark purchased via fetch) ──────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.shopping-check').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            var itemId   = this.getAttribute('data-item-id');
            var isCheck  = this.checked;
            var handler  = isCheck ? 'MarkPurchased' : 'UnmarkPurchased';
            var token    = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenVal = token ? token.value : '';

            fetch('/ShoppingList?handler=' + handler, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': tokenVal,
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: 'id=' + encodeURIComponent(itemId) + '&__RequestVerificationToken=' + encodeURIComponent(tokenVal)
            })
            .then(function (resp) {
                if (resp.ok) {
                    var li = document.getElementById('shopping-item-' + itemId);
                    if (li) {
                        var nameEl    = li.querySelector('.item-name');
                        var pantryBtn = li.querySelector('.move-to-pantry-inline');
                        if (isCheck) {
                            li.classList.add('shopping-item-purchased');
                            if (nameEl) { nameEl.style.textDecoration = 'line-through'; nameEl.style.opacity = '0.5'; }
                            if (pantryBtn) pantryBtn.style.setProperty('display', 'inline', 'important');
                        } else {
                            li.classList.remove('shopping-item-purchased');
                            if (nameEl) { nameEl.style.textDecoration = ''; nameEl.style.opacity = ''; }
                            if (pantryBtn) pantryBtn.style.setProperty('display', 'none', 'important');
                        }
                    }
                } else {
                    this.checked = !isCheck;
                }
            }.bind(this))
            .catch(function () {
                this.checked = !isCheck;
            }.bind(this));
        });
    });
});

