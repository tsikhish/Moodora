namespace Moodora.Models;

public class ProductMoodCategory
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int MoodCategoryId { get; set; }
    public MoodCategory? MoodCategory { get; set; }
}