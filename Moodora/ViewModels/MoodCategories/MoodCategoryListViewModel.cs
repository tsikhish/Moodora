using Moodora.Models;

namespace Moodora.ViewModels.MoodCategories
{
    public class MoodCategoryListViewModel
    {
        public MoodCategoryQueryParameters Query { get; set; } = new();
        public List<MoodCategory> Categories { get; set; } = [];
    }
}
