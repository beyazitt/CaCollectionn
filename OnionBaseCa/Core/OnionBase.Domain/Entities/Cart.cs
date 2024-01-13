using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; }
        public String UserId { get; set; } // Eğer kimlik doğrulama kullanıyorsanız bu, kullanıcının Id'si olabilir.
        public List<CartItem> CartItems { get; set; }
    }
}
