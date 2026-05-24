using Moodora.Models;
using Moodora.ViewModels.Products;

namespace Moodora.Services;

public interface IProductService
{
    Task<ProductListViewModel> GetCatalogAsync(ProductQueryParameters query);
    Task<Product?> GetByIdAsync(int id);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<List<MoodCategory>> GetMoodCategoriesAsync();
}