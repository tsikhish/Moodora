using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;
using Moodora.ViewModels.Products;

namespace Moodora.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters query)
    {
        var items = _context.Products
                    .Include(x => x.ProductMoodCategories)
                        .ThenInclude(x => x.MoodCategory)
                    .Where(x => x.DeleteDate == null && x.ProductMoodCategories.Any(pc => pc.MoodCategory != null && pc.MoodCategory.DeleteDate == null))
                    .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            items = items.Where(x => x.Name.Contains(query.Search) || (x.Description != null && x.Description.Contains(query.Search)));
        }

        if (query.MoodCategoryId.HasValue)
        {
            items = items.Where(x => x.ProductMoodCategories.Any(pc => pc.MoodCategoryId == query.MoodCategoryId.Value));
        }

        if (query.IsActive.HasValue)
        {
            items = items.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.MinPrice.HasValue) items = items.Where(x => x.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue) items = items.Where(x => x.Price <= query.MaxPrice.Value);
        if (query.InStockOnly) items = items.Where(x => x.Stock > 0);

        items = query.SortBy switch
        {
            "price_asc" => items.OrderBy(x => x.Price),
            "price_desc" => items.OrderByDescending(x => x.Price),
            "created_asc" => items.OrderBy(x => x.CreatedAt),
            _ => items.OrderByDescending(x => x.CreatedAt)
        };

        var total = await items.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        var pageItems = await items.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Product>
        {
            Items = pageItems,
            TotalItems = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products
             .Include(x => x.ProductMoodCategories)
                 .ThenInclude(x => x.MoodCategory)
             .FirstOrDefaultAsync(x => x.Id == id && x.DeleteDate == null && x.ProductMoodCategories.Any(pc => pc.MoodCategory != null && pc.MoodCategory.DeleteDate == null));

        if (product is not null)
        {
            product.SelectedMoodCategoryIds = product.ProductMoodCategories.Select(x => x.MoodCategoryId).ToList();
        }

        return product;
    }
    public async Task AddAsync(Product product)
    {
        var selectedCategoryIds = product.SelectedMoodCategoryIds.Distinct().ToList();
        product.ProductMoodCategories = selectedCategoryIds
            .Select(categoryId => new ProductMoodCategory { MoodCategoryId = categoryId })
            .ToList();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        var existingProduct = await _context.Products
          .Include(x => x.ProductMoodCategories)
          .FirstOrDefaultAsync(x => x.Id == product.Id && x.DeleteDate == null);

        if (existingProduct is null) return;

        _context.Entry(existingProduct).CurrentValues.SetValues(product);
        existingProduct.ProductMoodCategories.Clear();
        foreach (var categoryId in product.SelectedMoodCategoryIds.Distinct())
        {
            existingProduct.ProductMoodCategories.Add(new ProductMoodCategory
            {
                ProductId = product.Id,
                MoodCategoryId = categoryId
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.DeleteDate == null);
        if (product is null) return;

        product.DeleteDate = DateTime.UtcNow;
        product.UpdatedAt = product.DeleteDate;
        await _context.SaveChangesAsync();
    }
}
