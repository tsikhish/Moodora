using Moodora.Models;
using Moodora.Repositories;
using Moodora.ViewModels.Products;

namespace Moodora.Services;

public class ProductService(IProductRepository productRepository, IMoodCategoryRepository moodCategoryRepository) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMoodCategoryRepository _moodCategoryRepository = moodCategoryRepository;

    public async Task<ProductListViewModel> GetCatalogAsync(ProductQueryParameters query)
    {
        var result = await _productRepository.GetPagedAsync(query);
        var categories = await _moodCategoryRepository.GetAllAsync();

        return new ProductListViewModel
        {
            Query = query,
            Result = result,
            MoodCategories = categories
        };
    }

    public Task<Product?> GetByIdAsync(int id) => _productRepository.GetByIdAsync(id);

    public async Task CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.SelectedMoodCategoryIds = product.SelectedMoodCategoryIds.Distinct().ToList();
        if (product.Stock < 0) product.Stock = 0;
        await _productRepository.AddAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        product.SelectedMoodCategoryIds = product.SelectedMoodCategoryIds.Distinct().ToList();
        if (product.Stock < 0) product.Stock = 0;
        await _productRepository.UpdateAsync(product);
    }

    public Task DeleteAsync(int id) => _productRepository.DeleteAsync(id);

    public async Task<List<MoodCategory>> GetMoodCategoriesAsync() => await _moodCategoryRepository.GetAllAsync();
}