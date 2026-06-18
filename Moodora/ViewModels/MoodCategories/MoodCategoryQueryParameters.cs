namespace Moodora.ViewModels.MoodCategories
{
    public class MoodCategoryQueryParameters
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "name_asc";
    }
}
