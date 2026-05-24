using Moodora.Models;

namespace Moodora.Services;

public interface IMoodCategoryService
{
    Task<List<MoodCategory>> GetAllAsync();
    Task<MoodCategory?> GetByIdAsync(int id);
    Task CreateAsync(MoodCategory moodCategory);
    Task<bool> UpdateAsync(MoodCategory moodCategory);
    Task DeleteAsync(int id);
}