using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;
using Moodora.ViewModels.Payments;


namespace Moodora.Controllers;

[Authorize]
public class PaymentsController(ApplicationDbContext context) : Controller
{
    private const string SuccessfulTestCard = "4242424242424242";
    private const string DeclinedTestCard = "4000000000000002";
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Pay(int orderId)
    {
        var order = await LoadUserOrderAsync(orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Payments.Any(x => x.Status == PaymentStatus.Successful))
        {
            TempData["PaymentMessage"] = "This order already has a successful payment.";
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }

        return View(BuildFormViewModel(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(PaymentFormViewModel viewModel)
    {
        var order = await LoadUserOrderAsync(viewModel.OrderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Payments.Any(x => x.Status == PaymentStatus.Successful))
        {
            TempData["PaymentMessage"] = "This order already has a successful payment.";
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }

        viewModel.OrderNumber = order.OrderNumber;
        viewModel.Amount = order.TotalAmount;
        viewModel.Items = order.Items.ToList();

        var cardDigits = OnlyDigits(viewModel.CardNumber);
        if (cardDigits.Length is < 12 or > 19)
        {
            ModelState.AddModelError(nameof(PaymentFormViewModel.CardNumber), "Enter a valid test card number.");
        }
        else if (cardDigits != SuccessfulTestCard && cardDigits != DeclinedTestCard)
        {
            ModelState.AddModelError(nameof(PaymentFormViewModel.CardNumber), "Use one of the test cards shown on this page.");
        }

        if (!IsValidExpiryDate(viewModel.ExpiryDate))
        {
            ModelState.AddModelError(nameof(PaymentFormViewModel.ExpiryDate), "Use a future expiry date in MM/YY format.");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var status = ResolvePaymentStatus(cardDigits);
        if (status == PaymentStatus.Successful)
        {
            var unavailableProductIds = await FindUnavailableProductIdsAsync(order);
            if (unavailableProductIds.Count > 0)
            {
                ModelState.AddModelError(string.Empty, "One or more products are no longer available in the ordered quantity. Please review your cart.");
                return View(viewModel);
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var payment = new Payment
        {
            OrderId = order.Id,
            UserId = GetCurrentUserId(),
            Amount = order.TotalAmount,
            Status = status,
            TransactionId = await GenerateTransactionIdAsync(),
            CardLastFourDigits = cardDigits[^4..],
            CardBrand = ResolveCardBrand(cardDigits),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        if (status == PaymentStatus.Successful)
        {
            await CompleteOrderInventoryAsync(order);
        }

        order.Status = status == PaymentStatus.Successful ? OrderStatus.Confirmed : OrderStatus.Pending;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return RedirectToAction(nameof(Result), new { id = payment.Id });
    }

    public async Task<IActionResult> Result(int id)
    {
        var userId = GetCurrentUserId();
        var payment = await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(x => x!.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (payment?.Order is null)
        {
            return NotFound();
        }

        return View(new PaymentResultViewModel
        {
            OrderId = payment.OrderId,
            OrderNumber = payment.Order.OrderNumber,
            Amount = payment.Amount,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            CardLastFourDigits = payment.CardLastFourDigits,
            Items = payment.Order.Items.ToList()
        });
    }

    private async Task<Order?> LoadUserOrderAsync(int orderId)
    {
        var userId = GetCurrentUserId();
        return await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);
    }

    private async Task<IReadOnlyList<int>> FindUnavailableProductIdsAsync(Order order)
    {
        var productIds = order.Items.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        return order.Items
            .Where(x => !products.TryGetValue(x.ProductId, out var product)
                || product.DeleteDate != null
                || !product.IsActive
                || product.Stock < x.Quantity)
            .Select(x => x.ProductId)
            .ToList();
    }

    private async Task CompleteOrderInventoryAsync(Order order)
    {
        var productIds = order.Items.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                product.Stock -= item.Quantity;
            }
        }

        var userId = GetCurrentUserId();
        var cartItems = await _context.Carts
            .Where(x => x.UserId == userId && productIds.Contains(x.ProductId) && x.DeletedDate == null)
            .ToListAsync();
        _context.Carts.RemoveRange(cartItems);
    }

    private static PaymentFormViewModel BuildFormViewModel(Order order)
    {
        return new PaymentFormViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Amount = order.TotalAmount,
            Items = order.Items.ToList()
        };
    }

    private static string OnlyDigits(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static PaymentStatus ResolvePaymentStatus(string cardDigits)
    {
        return cardDigits == SuccessfulTestCard ? PaymentStatus.Successful : PaymentStatus.Failed;
    }

    private static string ResolveCardBrand(string cardDigits)
    {
        return cardDigits.StartsWith('4') ? "Visa Test" : "Test Card";
    }

    private static bool IsValidExpiryDate(string expiryDate)
    {
        var parts = expiryDate.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
        {
            return false;
        }

        if (month is < 1 or > 12)
        {
            return false;
        }

        var fullYear = 2000 + year;
        var lastDayOfMonth = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));
        return lastDayOfMonth >= DateTime.UtcNow.Date;
    }

    private async Task<string> GenerateTransactionIdAsync()
    {
        string transactionId;
        do
        {
            transactionId = $"MDP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }
        while (await _context.Payments.AnyAsync(x => x.TransactionId == transactionId));

        return transactionId;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
    }
}