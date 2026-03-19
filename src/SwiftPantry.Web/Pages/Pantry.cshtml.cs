using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftPantry.Web.Models;
using SwiftPantry.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace SwiftPantry.Web.Pages;

public class PantryModel : PageModel
{
    private readonly IPantryService _pantryService;

    public PantryModel(IPantryService pantryService)
    {
        _pantryService = pantryService;
    }

    public List<PantryItem> Items { get; set; } = new();

    [BindProperty]
    public NewItemInput NewItem { get; set; } = new();

    [BindProperty]
    public EditItemInput EditItem { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _pantryService.GetAllItemsAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        ModelState.Remove(nameof(EditItem) + "." + nameof(EditItemInput.Id));
        ModelState.Remove(nameof(EditItem) + "." + nameof(EditItemInput.Name));
        ModelState.Remove(nameof(EditItem) + "." + nameof(EditItemInput.Quantity));
        ModelState.Remove(nameof(EditItem) + "." + nameof(EditItemInput.Category));

        if (!ModelState.IsValid)
        {
            Items = await _pantryService.GetAllItemsAsync();
            return Page();
        }

        if (await _pantryService.NameExistsAsync(NewItem.Name))
        {
            ModelState.AddModelError($"{nameof(NewItem)}.{nameof(NewItemInput.Name)}", "An item with that name already exists.");
            Items = await _pantryService.GetAllItemsAsync();
            return Page();
        }

        await _pantryService.AddItemAsync(new PantryItem
        {
            Name     = NewItem.Name,
            Quantity = NewItem.Quantity,
            Category = NewItem.Category
        });

        TempData["Success"] = $"\"{NewItem.Name}\" added to pantry.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        ModelState.Remove(nameof(NewItem) + "." + nameof(NewItemInput.Name));
        ModelState.Remove(nameof(NewItem) + "." + nameof(NewItemInput.Quantity));
        ModelState.Remove(nameof(NewItem) + "." + nameof(NewItemInput.Category));

        if (!ModelState.IsValid)
        {
            Items = await _pantryService.GetAllItemsAsync();
            return Page();
        }

        var existing = await _pantryService.GetByIdAsync(EditItem.Id);
        if (existing is null)
            return NotFound();

        if (!existing.Name.Equals(EditItem.Name.Trim(), StringComparison.OrdinalIgnoreCase)
            && await _pantryService.NameExistsAsync(EditItem.Name, EditItem.Id))
        {
            ModelState.AddModelError($"{nameof(EditItem)}.{nameof(EditItemInput.Name)}", "An item with that name already exists.");
            Items = await _pantryService.GetAllItemsAsync();
            return Page();
        }

        existing.Name     = EditItem.Name;
        existing.Quantity = EditItem.Quantity;
        existing.Category = EditItem.Category;

        await _pantryService.UpdateItemAsync(existing);

        TempData["Success"] = $"\"{existing.Name}\" updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _pantryService.GetByIdAsync(id);
        if (item is not null)
        {
            await _pantryService.DeleteItemAsync(id);
            TempData["Success"] = $"\"{item.Name}\" removed from pantry.";
        }
        return RedirectToPage();
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

    public class EditItemInput
    {
        public int Id { get; set; }

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
