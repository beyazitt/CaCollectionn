using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.DTOs
{
    public class PaymentConfirmationDTO
    {
        public bool isConfirmed { get; set; }
        public Guid OrderId { get; set; }
        public List<Order> orders { get; set; }
        public string? shippingCode { get; set; }
    }
}
