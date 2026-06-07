using System.ComponentModel.DataAnnotations;

namespace Moodora.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(40)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AdditionalComment { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    [StringLength(80)]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
