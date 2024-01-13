using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class ProductVariant
    {
        public Guid ProductVariantId { get; set; }
        public Guid ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }
        public string? Image { get; set; }

        // İlişkili ana ürün
        public virtual Product Product { get; set; }
    }

}
