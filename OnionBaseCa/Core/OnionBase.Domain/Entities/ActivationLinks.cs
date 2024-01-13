using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class ActivationLinks
    {
        public Guid Id { get; set; }    
        public long Link {  get; set; }
        public bool ısActive { get; set; }
    }
}
