using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moodora.Models;

public class MoodCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }


    [StringLength(1000)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeleteDate { get; set; }

    [NotMapped]
    public string? DisplayImageUrl => NormalizeImageUrl(ImageUrl);

    public ICollection<Product> Products { get; set; } = new List<Product>();

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var trimmed = imageUrl.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            return trimmed;
        }

        return $"https://{trimmed}";
    }

    public ICollection<ProductMoodCategory> ProductMoodCategories { get; set; } = new List<ProductMoodCategory>();
}