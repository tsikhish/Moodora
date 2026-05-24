using Moodora.Models;
using Moodora.ViewModels.Products;

namespace Moodora.Repositories;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters query);
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}