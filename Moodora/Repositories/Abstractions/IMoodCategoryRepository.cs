using Moodora.Models;

namespace Moodora.Repositories;

public interface IMoodCategoryRepository
{
    Task<List<MoodCategory>> GetAllAsync();
    Task<MoodCategory?> GetByIdAsync(int id);
    Task AddAsync(MoodCategory moodCategory);
    Task UpdateAsync(MoodCategory moodCategory);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}