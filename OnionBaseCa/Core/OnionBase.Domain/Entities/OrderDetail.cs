using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class OrderDetail
    {
        [Key]
        public Guid OrderDetailId { get; set; }

        [ForeignKey("OrderId")]
        public Guid OrderId { get; set; }

        [ForeignKey("ProductFkId\"")]
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; } // indirimsiz birim fiyat
        public double DiscountedPrice { get; set; } // indirimli fiyat
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }
        public virtual Order Order { get; set; }

    }
}
