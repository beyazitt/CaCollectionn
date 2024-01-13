using OnionBase.Domain.Entities.Common;
using OnionBase.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class Order:BaseEntity
    {
        [Key]
        public Guid OrderId { get; set; }
        public string UserId { get; set; } // siparişi veren kullanıcının ID'si
        public DateTime OrderDate { get; set; }
        public double TotalPrice { get; set; } // indirimli fiyat dahil
        public string? CampaignCode { get; set; } // eğer kampanya kodu kullanıldıysa
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public bool confirmationRequest { get; set; }
        public bool isConfirmed { get; set; }
        public bool? Shipping { get; set; }
        public string? ShippingCode { get; set; }


    }
}
