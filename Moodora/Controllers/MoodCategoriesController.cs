using Microsoft.AspNetCore.Mvc;
using Moodora.Models;
using Moodora.Services;

namespace Moodora.Controllers;

public class MoodCategoriesController(IMoodCategoryService service) : Controller
{
    private readonly IMoodCategoryService _service = service;

    public async Task<IActionResult> Index()
    {
        var categories = await _service.GetAllAsync();
        return View(categories);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var moodCategory = await _service.GetByIdAsync(id.Value);
        if (moodCategory is null)
        {
            return NotFound();
        }

        return View(moodCategory);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,ImageUrl,IsActive")] MoodCategory moodCategory)
    {
        if (!ModelState.IsValid)
        {
            return View(moodCategory);
        }

        await _service.CreateAsync(moodCategory);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var moodCategory = await _service.GetByIdAsync(id.Value);
        if (moodCategory is null)
        {
            return NotFound();
        }

        return View(moodCategory);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageUrl,IsActive,CreatedAt")] MoodCategory moodCategory)
    {
        if (id != moodCategory.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(moodCategory);
        }

        var updated = await _service.UpdateAsync(moodCategory);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var moodCategory = await _service.GetByIdAsync(id.Value);
        if (moodCategory is null)
        {
            return NotFound();
        }

        return View(moodCategory);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}