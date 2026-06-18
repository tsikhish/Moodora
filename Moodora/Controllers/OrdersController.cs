using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;
using Moodora.ViewModels.Orders;
using System.Text.RegularExpressions;

namespace Moodora.Controllers;

[Authorize]
public class OrdersController(ApplicationDbContext context) : Controller
{
    private static readonly string[] PaymentMethods = ["Cash on Delivery", "Demo Payment"];
    private readonly ApplicationDbContext _context = context;
    private static readonly Dictionary<string, (string Name, string Pattern)> PhoneRules = new()
    {
        ["US"] = ("United States", @"^\+?1?[2-9]\d{2}[2-9]\d{2}\d{4}$"),
        ["GB"] = ("United Kingdom", @"^\+?44?7\d{9}$|^0?7\d{9}$"),
        ["FR"] = ("France", @"^\+?33?[1-9]\d{8}$|^0[1-9]\d{8}$"),
        ["DE"] = ("Germany", @"^\+?49?[1-9]\d{6,13}$|^0[1-9]\d{6,13}$"),
        ["GE"] = ("Georgia", @"^(995)?[3-7]\d{8}$|^0[3-7]\d{8}$"),
        ["IN"] = ("India", @"^\+?91?[6-9]\d{9}$")
    };

    public async Task<IActionResult> Checkout()
    {
        if (User.IsInRole(ApplicationRoles.Admin))
        {
            return Forbid();
        }
        if (await IsCurrentUserBlockedAsync())
        {
            TempData["CartError"] = "Your account is blocked from buying products. You can still browse products and mood categories.";
            return RedirectToAction("Index", "Products");
        }
        var cartItems = await LoadCurrentCartAsync();
        if (cartItems.Count == 0)
        {
            TempData["CartError"] = "Your cart is empty. Add products before checkout.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.PaymentMethods = BuildPaymentMethods();
        ViewBag.Countries = BuildCountries();
        return View(new CheckoutViewModel
        {
            FullName = User.Identity?.Name ?? string.Empty,
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            CartItems = cartItems
        });
    }
    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserBlock(int id)
    {
        var order = await _context.Orders
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order?.User is null)
        {
            return NotFound();
        }

        order.User.IsBlocked = !order.User.IsBlocked;
        await _context.SaveChangesAsync();

        TempData["AdminOrderMessage"] = order.User.IsBlocked
            ? $"{order.FullName} has been blocked from buying products."
            : $"{order.FullName} can buy products again.";

        return RedirectToAction(nameof(AdminDetails), new { id });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel viewModel)
    {
        var cartItems = await LoadCurrentCartAsync();
        viewModel.CartItems = cartItems;

        if (cartItems.Count == 0)
        {
            TempData["CartError"] = "Your cart is empty. Add products before checkout.";
            return RedirectToAction("Index", "Cart");
        }

        if (!PaymentMethods.Contains(viewModel.PaymentMethod))
        {
            ModelState.AddModelError(nameof(CheckoutViewModel.PaymentMethod), "Please choose a supported payment method.");
        }
        ValidatePhoneNumber(viewModel);

        var unavailableItem = cartItems.FirstOrDefault(x =>
            x.Product is null || x.Product.DeleteDate != null || !x.Product.IsActive || x.Product.Stock < x.Quantity);
        if (unavailableItem is not null)
        {
            ModelState.AddModelError(string.Empty, "One or more cart products are no longer available in the requested quantity. Please review your cart.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PaymentMethods = BuildPaymentMethods(viewModel.PaymentMethod);
            ViewBag.Countries = BuildCountries(viewModel.CountryCode);
            return View(viewModel);
        }

        var isDemoPayment = string.Equals(viewModel.PaymentMethod, "Demo Payment", StringComparison.OrdinalIgnoreCase);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            UserId = GetCurrentUserId(),
            FullName = viewModel.FullName.Trim(),
            Email = viewModel.Email.Trim(),
            PhoneNumber = viewModel.PhoneNumber.Trim(),
            City = viewModel.City.Trim(),
            Address = viewModel.Address.Trim(),
            AdditionalComment = string.IsNullOrWhiteSpace(viewModel.AdditionalComment) ? null : viewModel.AdditionalComment.Trim(),
            PaymentMethod = viewModel.PaymentMethod,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var cartItem in cartItems)
        {
            var product = cartItem.Product!;
            var lineTotal = product.Price * cartItem.Quantity;
            order.TotalAmount += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImage = product.DisplayImageUrl,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal
            });
            if (!isDemoPayment)
            {
                product.Stock -= cartItem.Quantity;
            }
        }

        _context.Orders.Add(order);
        if (!isDemoPayment)
        {
            _context.Carts.RemoveRange(cartItems);
        }
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["OrderMessage"] = $"Thank you! Order {order.OrderNumber} has been created.";
        if (isDemoPayment)
        {
            return RedirectToAction("Pay", "Payments", new { orderId = order.Id });
        }
        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await LoadUserOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> History()
    {
        var userId = GetCurrentUserId();
        var orders = await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await LoadUserOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Admin([FromQuery] AdminOrderQueryParameters query)
    {
        var ordersQuery = _context.Orders
           .Include(x => x.Items)
            .Include(x => x.Payments)
            .Include(x => x.User)
             .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            ordersQuery = ordersQuery.Where(x =>
                x.OrderNumber.Contains(query.Search) ||
                x.FullName.Contains(query.Search) ||
                x.Email.Contains(query.Search) ||
                x.City.Contains(query.Search) ||
                x.Items.Any(item => item.ProductName.Contains(query.Search)));
        }

        if (query.Status.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            ordersQuery = ordersQuery.Where(x => x.UserId == query.CustomerId);
        }

        var viewModel = new AdminOrderListViewModel
        {
            Query = query,
            Orders = await ordersQuery.OrderByDescending(x => x.CreatedAt).ToListAsync()
        };

        ViewBag.Statuses = new SelectList(Enum.GetValues<OrderStatus>().Select(x => new { Value = x, Text = x.ToString() }), "Value", "Text", query.Status);
        ViewBag.Customers = await BuildCustomerOptionsAsync(query.CustomerId);
        return View(viewModel);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> AdminDetails(int id)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
             .Include(x => x.Payments)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        ViewBag.Statuses = BuildStatuses(order.Status);
        return View(order);
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (!Enum.IsDefined(status))
        {
            TempData["AdminOrderMessage"] = "Please choose a valid order status.";
            return RedirectToAction(nameof(AdminDetails), new { id });
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["AdminOrderMessage"] = $"Order {order.OrderNumber} status updated to {status}.";

        return RedirectToAction(nameof(AdminDetails), new { id });
    }
    private async Task<bool> IsCurrentUserBlockedAsync()
    {
        var userId = GetCurrentUserId();
        return await _context.Users.AnyAsync(x => x.Id == userId && x.IsBlocked);
    }

    private async Task<List<Cart>> LoadCurrentCartAsync()
    {
        var userId = GetCurrentUserId();
        return await _context.Carts
            .Include(x => x.Product)
               .ThenInclude(x => x!.ProductMoodCategories)
                    .ThenInclude(x => x.MoodCategory)
            .Where(x => x.UserId == userId && x.DeletedDate == null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    private async Task<Order?> LoadUserOrderAsync(int id)
    {
        var userId = GetCurrentUserId();
        return await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        string orderNumber;
        do
        {
            orderNumber = $"MO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _context.Orders.AnyAsync(x => x.OrderNumber == orderNumber));

        return orderNumber;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
    }

    private static SelectList BuildPaymentMethods(string? selected = null)
    {
        return new SelectList(PaymentMethods, selected);
    }
    private static SelectList BuildCountries(string? selected = null)
    {
        return new SelectList(PhoneRules.Select(x => new { Value = x.Key, Text = x.Value.Name }), "Value", "Text", selected ?? "US");
    }

    private void ValidatePhoneNumber(CheckoutViewModel viewModel)
    {
        if (!PhoneRules.TryGetValue(viewModel.CountryCode, out var rule))
        {
            ModelState.AddModelError(nameof(CheckoutViewModel.CountryCode), "Please choose a supported country.");
            return;
        }

        var normalizedPhoneNumber = Regex.Replace(viewModel.PhoneNumber ?? string.Empty, @"[\s().-]", string.Empty);
        if (!Regex.IsMatch(normalizedPhoneNumber, rule.Pattern))
        {
            ModelState.AddModelError(nameof(CheckoutViewModel.PhoneNumber), $"Enter a valid phone number for {rule.Name}.");
        }
    }


    private static SelectList BuildStatuses(OrderStatus selected)
    {
        return new SelectList(Enum.GetValues<OrderStatus>().Select(x => new { Value = x, Text = x.ToString() }), "Value", "Text", selected);
    }
    private async Task<SelectList> BuildCustomerOptionsAsync(string? selected = null)
    {
        var customers = await _context.Orders
            .Include(x => x.User)
            .Select(x => new
            {
                x.UserId,
                CustomerName = x.User != null
                    ? x.User.UserName ?? x.Email
                    : x.Email
            })
            .Distinct()
            .OrderBy(x => x.CustomerName)
            .ToListAsync();

        return new SelectList(customers, "UserId", "CustomerName", selected);
    }
}