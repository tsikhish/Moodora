using Moodora.Models;
using Moodora.ViewModels.MoodCategories;

namespace Moodora.Services;

public interface IMoodCategoryService
{
    Task<List<MoodCategory>> GetAllAsync();
    Task<MoodCategoryListViewModel> GetListAsync(MoodCategoryQueryParameters query);
    Task<MoodCategory?> GetByIdAsync(int id);
    Task CreateAsync(MoodCategory moodCategory);
    Task<bool> UpdateAsync(MoodCategory moodCategory);
    Task DeleteAsync(int id);
}