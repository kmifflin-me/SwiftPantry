using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Pages;

public class ShoppingListModel : PageModel
{
    private readonly IShoppingListService _shoppingListService;

    public ShoppingListModel(IShoppingListService shoppingListService)
    {
        _shoppingListService = shoppingListService;
    }

    public async Task<IActionResult> OnPostMoveToPantryAsync(int id)
    {
        var moved = await _shoppingListService.MoveToPantryAsync(id);
        TempData[moved ? "Success" : "Error"] = moved
            ? "Item moved to pantry."
            : "An item with that name already exists in your pantry.";
        return RedirectToPage();
    }

    public List<ShoppingListItem> Items { get; set; } = new();

    [BindProperty]
    public NewItemInput NewItem { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _shoppingListService.GetAllItemsAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            Items = await _shoppingListService.GetAllItemsAsync();
            return Page();
        }

        await _shoppingListService.AddItemAsync(new ShoppingListItem
        {
            Name     = NewItem.Name,
            Quantity = NewItem.Quantity,
            Category = NewItem.Category
        });

        TempData["Success"] = $"\"{NewItem.Name}\" added to shopping list.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _shoppingListService.DeleteItemAsync(id);
        TempData["Success"] = "Item removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearPurchasedAsync()
    {
        await _shoppingListService.DeleteAllPurchasedAsync();
        TempData["Success"] = "Purchased items cleared.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkPurchasedAsync(int id)
    {
        await _shoppingListService.MarkPurchasedAsync(id);
        return new OkResult();
    }

    public class NewItemInput
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Quantity is required.")]
        [MaxLength(100)]
        public string Quantity { get; set; } = "";

        [Required(ErrorMessage = "Please select a category.")]
        public string Category { get; set; } = "";
    }
}
