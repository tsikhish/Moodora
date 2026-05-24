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
            .Include(x => x.Products)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<MoodCategory?> GetByIdAsync(int id)
    {
        return _context.MoodCategories
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);
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
        var entity = await _context.MoodCategories.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.MoodCategories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.MoodCategories.AnyAsync(x => x.Id == id);
    }
}