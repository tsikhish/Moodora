using System.ComponentModel.DataAnnotations;
using Moodora.Models;

namespace Moodora.ViewModels.Payments;

public class PaymentFormViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public IReadOnlyList<OrderItem> Items { get; set; } = Array.Empty<OrderItem>();

    [Required]
    [Display(Name = "Name on card")]
    [StringLength(120)]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Card number")]
    [StringLength(23, MinimumLength = 12)]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Expiry date")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Use MM/YY format.")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Security code")]
    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Enter 3 or 4 digits.")]
    public string SecurityCode { get; set; } = string.Empty;
}
