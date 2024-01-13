using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; } // Hangi ürüne ait olduğu
        public int Quantity { get; set; }  // Sepette kaç adet olduğu
        public virtual ProductVariant ProductVariant { get; set; } // Ürün detayları

        public List<Product> Product { get; set; }
        public Guid ProductId { get; set; }
        public Guid CartId { get; set; }      // Hangi sepete ait olduğu
        public Cart Cart { get; set; }
        public string Beden { get; set; }
        public string Color { get; set; }
    }
}
