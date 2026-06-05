using System.ComponentModel.DataAnnotations;

namespace Moodora.Models;

public class Cart
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public Product? Product { get; set; }
}