using Microsoft.EntityFrameworkCore;
using Moodora.Data;
using Moodora.Models;

namespace Moodora.Repositories;

public class MoodCategoryRepository(ApplicationDbContext context) : IMoodCategoryRepository
{
    private readonly ApplicationDbContext _context = context;

    public Task<List<MoodCategory>> GetAllAsync()
    {
        return _context.MoodCategories
            .Include(x => x.ProductMoodCategories.Where(productCategory => productCategory.Product != null && productCategory.Product.DeleteDate == null))
                .ThenInclude(x => x.Product)
            .Where(x => x.DeleteDate == null)
         
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<MoodCategory?> GetByIdAsync(int id)
    {
        return _context.MoodCategories
    .Include(x => x.ProductMoodCategories.Where(productCategory => productCategory.Product != null && productCategory.Product.DeleteDate == null))
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeleteDate == null);
    }

    public async Task AddAsync(MoodCategory moodCategory)
    {
        _context.MoodCategories.Add(moodCategory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MoodCategory moodCategory)
    {
        _context.MoodCategories.Update(moodCategory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.MoodCategories.FirstOrDefaultAsync(x => x.Id == id && x.DeleteDate == null);
        if (entity is null)
        {
            return;
        }
        entity.DeleteDate = DateTime.UtcNow;
        _context.MoodCategories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.MoodCategories.AnyAsync(x => x.Id == id && x.DeleteDate == null);
    }
}