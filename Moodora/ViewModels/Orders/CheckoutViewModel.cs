using System.ComponentModel.DataAnnotations;
using Moodora.Models;

namespace Moodora.ViewModels.Orders;

public class CheckoutViewModel
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(40)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Additional comment")]
    public string? AdditionalComment { get; set; }

    [Required]
    [StringLength(80)]
    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    public IReadOnlyList<Cart> CartItems { get; set; } = Array.Empty<Cart>();

    public decimal TotalAmount => CartItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);
}