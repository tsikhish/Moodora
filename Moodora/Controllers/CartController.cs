using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;

namespace Moodora.Controllers;

[Authorize]
public class CartController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();

        var cartItems = await _context.Carts
            .Include(x => x.Product)
                .ThenInclude(x => x!.ProductMoodCategories)
                    .ThenInclude(x => x.MoodCategory)
            .Where(x => x.UserId == userId
                && x.Product != null
                && x.Product.DeleteDate == null
                && x.DeletedDate == null)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync();

        return View(cartItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (quantity < 1)
        {
            TempData["CartError"] = "Please choose a quantity of at least 1.";
            return RedirectToSafeReturnUrl(returnUrl, productId);
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == productId && x.DeleteDate == null && x.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        if (product.Stock <= 0)
        {
            TempData["CartError"] = $"{product.Name} is currently out of stock.";
            return RedirectToSafeReturnUrl(returnUrl, productId);
        }

        var userId = GetCurrentUserId();
        var cartItem = await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

        var requestedQuantity = quantity;
        if (cartItem is null)
        {
            cartItem = new Cart
            {
                ProductId = productId,
                UserId = userId,
                Quantity = Math.Min(requestedQuantity, product.Stock),
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cartItem);
        }
        else
        {
            if (cartItem.DeletedDate is null)
            {
                requestedQuantity += cartItem.Quantity;
            }
            cartItem.Quantity = Math.Min(requestedQuantity, product.Stock);
            cartItem.DeletedDate = null;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["CartMessage"] = cartItem.Quantity < requestedQuantity
            ? $"Only {cartItem.Quantity} {product.Name} item(s) are available, so your cart was updated to the stock limit."
            : $"{quantity} {product.Name} item(s) added to your cart.";

        return RedirectToSafeReturnUrl(returnUrl, productId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        var userId = GetCurrentUserId();
        var cartItem = await _context.Carts
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (cartItem is null)
        {
            return NotFound();
        }

        if (quantity < 1)
        {
            cartItem.DeletedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["CartMessage"] = "Item removed from your cart.";
        }
        else
        {
            var stock = cartItem.Product?.Stock ?? 0;
            if (stock <= 0)
            {
                cartItem.DeletedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = "Item removed because it is no longer in stock.";
            }
            else
            {
                cartItem.Quantity = Math.Min(quantity, stock);
                cartItem.UpdatedAt = DateTime.UtcNow;
                TempData["CartMessage"] = cartItem.Quantity < quantity
                    ? "Cart quantity was updated to match available stock."
                    : "Cart quantity updated.";
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = GetCurrentUserId();
        var cartItem = await _context.Carts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (cartItem is null)
        {
            return NotFound();
        }

        cartItem.DeletedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["CartMessage"] = "Item removed from your cart.";

        return RedirectToAction(nameof(Index));
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
    }

    private IActionResult RedirectToSafeReturnUrl(string? returnUrl, int productId)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Details", "Products", new { id = productId });
    }
}