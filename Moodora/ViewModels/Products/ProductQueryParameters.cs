namespace Moodora.ViewModels.Products
{
    public class ProductQueryParameters
    {
        public string? Search { get; set; }
        public int? MoodCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }
        public string SortBy { get; set; } = "created_desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
