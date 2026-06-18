using Moodora.Models;
using Moodora.Repositories;
using Moodora.ViewModels.MoodCategories;

namespace Moodora.Services;

public class MoodCategoryService(IMoodCategoryRepository repository) : IMoodCategoryService
{
    private readonly IMoodCategoryRepository _repository = repository;

    public Task<List<MoodCategory>> GetAllAsync() => _repository.GetAllAsync();
    public async Task<MoodCategoryListViewModel> GetListAsync(MoodCategoryQueryParameters query)
    {
        return new MoodCategoryListViewModel
        {
            Query = query,
            Categories = await _repository.GetFilteredAsync(query)
        };
    }
    public Task<MoodCategory?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task CreateAsync(MoodCategory moodCategory)
    {
        moodCategory.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(moodCategory);
    }

    public async Task<bool> UpdateAsync(MoodCategory moodCategory)
    {
        var exists = await _repository.ExistsAsync(moodCategory.Id);
        if (!exists)
        {
            return false;
        }

        await _repository.UpdateAsync(moodCategory);
        return true;
    }

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}