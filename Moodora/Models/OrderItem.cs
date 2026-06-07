using System.ComponentModel.DataAnnotations;

namespace Moodora.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ProductImage { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    public Order? Order { get; set; }

    public Product? Product { get; set; }
}