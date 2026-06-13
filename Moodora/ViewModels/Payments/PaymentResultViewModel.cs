using Moodora.Models;

namespace Moodora.ViewModels.Payments;

public class PaymentResultViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public string? CardLastFourDigits { get; set; }

    public IReadOnlyList<OrderItem> Items { get; set; } = Array.Empty<OrderItem>();
}