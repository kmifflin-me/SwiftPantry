// SwiftPantry site.js

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

// ── Shopping list checkbox (mark purchased via fetch) ─────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.shopping-check').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            if (!this.checked) return;
            var itemId = this.getAttribute('data-item-id');
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenVal = token ? token.value : '';

            fetch('/ShoppingList?handler=MarkPurchased', {
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
                        li.classList.add('shopping-item-purchased');
                        var nameEl = li.querySelector('.item-name');
                        if (nameEl) {
                            nameEl.style.textDecoration = 'line-through';
                            nameEl.style.opacity = '0.5';
                        }
                        var addBtn = li.querySelector('.add-to-pantry-btn');
                        if (addBtn) addBtn.style.display = '';
                        checkbox.disabled = true;
                    }
                } else {
                    checkbox.checked = false;
                }
            })
            .catch(function () {
                checkbox.checked = false;
            });
        });
    });
});

