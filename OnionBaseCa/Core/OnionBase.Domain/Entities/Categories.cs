using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class Categories
    {
        [Key]
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

    }
}
