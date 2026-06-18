using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;
using Moodora.Services;
using Moodora.ViewModels.Products;

namespace Moodora.Controllers;

[Authorize]
public class ProductsController(IProductService productService, ApplicationDbContext context) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly ApplicationDbContext _context = context;
    public async Task<IActionResult> Index([FromQuery] ProductQueryParameters query)
    {
        var viewModel = await _productService.GetCatalogAsync(query);
        await LoadBlockedStatusAsync();
        return View(viewModel);
    }

    public async Task<IActionResult> ByMoodCategory(int id, [FromQuery] ProductQueryParameters query)
    {
        query.MoodCategoryId = id;
        var viewModel = await _productService.GetCatalogAsync(query);
        await LoadBlockedStatusAsync();
        return View("Index", viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();
        await LoadBlockedStatusAsync();
        return View(product);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Create()
    {
        var product = new Product { IsActive = true };
        await LoadMoodCategoriesAsync(product.SelectedMoodCategoryIds);
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        //ValidateMoodCategories(product);
        if (!ModelState.IsValid)
        {
            await LoadMoodCategoriesAsync(product.SelectedMoodCategoryIds);
            return View(product);
        }

        await _productService.CreateAsync(product);
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
        await LoadBlockedStatusAsync();
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
    private async Task LoadBlockedStatusAsync()
    {
        if (User.IsInRole(ApplicationRoles.Admin))
        {
            ViewBag.IsBlockedUser = false;
            return;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.IsBlockedUser = userId is not null
            && await _context.Users.AnyAsync(x => x.Id == userId && x.IsBlocked);
    }   

    private async Task LoadMoodCategoriesAsync(IEnumerable<int>? selectedIds = null)
    {
        var categories = await _productService.GetMoodCategoriesAsync();
        ViewBag.MoodCategories = new MultiSelectList(categories, nameof(MoodCategory.Id), nameof(MoodCategory.Name), selectedIds);
    }
}