using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.ViewModels
{
    public class OrderDetailViewModel
    {
        public List<Product> Product { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
    }
}
