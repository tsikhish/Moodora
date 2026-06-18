using Moodora.Models;
using Moodora.ViewModels.MoodCategories;

namespace Moodora.Repositories;

public interface IMoodCategoryRepository
{
    Task<List<MoodCategory>> GetAllAsync();
    Task<List<MoodCategory>> GetFilteredAsync(MoodCategoryQueryParameters query);
    Task<MoodCategory?> GetByIdAsync(int id);
    Task AddAsync(MoodCategory moodCategory);
    Task UpdateAsync(MoodCategory moodCategory);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}