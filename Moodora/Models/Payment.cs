using System.ComponentModel.DataAnnotations;

namespace Moodora.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Required]
    [StringLength(40)]
    public string TransactionId { get; set; } = string.Empty;

    [StringLength(4)]
    public string? CardLastFourDigits { get; set; }

    [StringLength(80)]
    public string CardBrand { get; set; } = "Test Card";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public Order? Order { get; set; }

    public ApplicationUser? User { get; set; }
}