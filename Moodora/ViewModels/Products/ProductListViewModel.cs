using Moodora.Models;

namespace Moodora.ViewModels.Products;

public class ProductListViewModel
{
    public PagedResult<Product> Result { get; set; } = new();
    public ProductQueryParameters Query { get; set; } = new();
    public List<MoodCategory> MoodCategories { get; set; } = [];
}