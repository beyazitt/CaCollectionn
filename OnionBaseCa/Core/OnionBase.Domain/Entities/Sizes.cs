using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class Sizes
    {
        [Key]
        public Guid SizeId { get; set; }
        public string Value { get; set; }
    }
}
