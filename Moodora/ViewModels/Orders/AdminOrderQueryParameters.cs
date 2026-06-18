using Moodora.Models;

namespace Moodora.ViewModels.Orders
{
    public class AdminOrderQueryParameters
    {
        public string? Search { get; set; }
        public OrderStatus? Status { get; set; }
    }
}
