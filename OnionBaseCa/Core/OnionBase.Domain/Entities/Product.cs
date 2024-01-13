using OnionBase.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class Product:BaseEntity
    {
        [Key]
        public Guid ProductId { get; set; }  
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int TotalStock { get; set; }
        public int ProductCode { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public List<ProductVariant> ProductVariants { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Campaigns> Campaigns { get; set; }
        public ICollection<Categories> Categories { get; set; }
    }
}
