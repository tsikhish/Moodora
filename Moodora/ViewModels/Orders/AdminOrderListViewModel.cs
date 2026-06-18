using Moodora.Models;

namespace Moodora.ViewModels.Orders
{
    public class AdminOrderListViewModel
    {
        public AdminOrderQueryParameters Query { get; set; } = new();
        public IReadOnlyList<Order> Orders { get; set; } = [];
    }
}
