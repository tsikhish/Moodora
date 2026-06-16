using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moodora.Models;
using Moodora.Services;
using Moodora.ViewModels.Products;

namespace Moodora.Controllers;

[Authorize]
public class ProductsController(IProductService productService) : Controller
{
    private readonly IProductService _productService = productService;

    public async Task<IActionResult> Index([FromQuery] ProductQueryParameters query)
    {
        var viewModel = await _productService.GetCatalogAsync(query);
        return View(viewModel);
    }

    public async Task<IActionResult> ByMoodCategory(int id, [FromQuery] ProductQueryParameters query)
    {
        query.MoodCategoryId = id;
        var viewModel = await _productService.GetCatalogAsync(query);
        return View("Index", viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        return View(product);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Create()
    {
        await LoadMoodCategoriesAsync();
        return View(new Product { IsActive = true });
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(new Product { IsActive = true, SelectedMoodCategoryIds = [] });
            return View(product);
        }

        await LoadMoodCategoriesAsync(product.SelectedMoodCategoryIds);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        await LoadMoodCategoriesAsync(product.SelectedMoodCategoryIds);
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return NotFound();
        ValidateMoodCategories(product);
        if (!ModelState.IsValid)
        {
            await LoadMoodCategoriesAsync(product.SelectedMoodCategoryIds);
            return View(product);
        }

        await _productService.UpdateAsync(product);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadMoodCategoriesAsync(IEnumerable<int>? selectedCategoryIds = null)
    {
        var categories = await _productService.GetMoodCategoriesAsync();
        ViewBag.MoodCategories = new MultiSelectList(categories, nameof(MoodCategory.Id), nameof(MoodCategory.Name), selectedCategoryIds);
    }

    private void ValidateMoodCategories(Product product)
    {
        if (product.SelectedMoodCategoryIds.Count == 0)
        {
            ModelState.AddModelError(nameof(Product.SelectedMoodCategoryIds), "Choose at least one mood category.");
        }
    }
}