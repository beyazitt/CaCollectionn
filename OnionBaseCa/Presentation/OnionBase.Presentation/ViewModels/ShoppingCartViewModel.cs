using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<CartItem>? CartItems { get; set; }
        public Guid cartId { get; set; }
  
    }
}
