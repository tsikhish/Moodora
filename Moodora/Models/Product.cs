using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moodora.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeleteDate { get; set; }
    [NotMapped]
    [Display(Name = "Mood Categories")]
    public List<int> SelectedMoodCategoryIds { get; set; } = new();

    public ICollection<ProductMoodCategory> ProductMoodCategories { get; set; } = new List<ProductMoodCategory>();
}