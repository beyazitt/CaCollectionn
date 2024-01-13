using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class VisitedDatas
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int View { get; set; }
        public bool locked { get; set; }

        [ForeignKey("ProductId")]
        public Guid? ProductId { get; set; }
        public Product Product { get; set; }
    }
}
