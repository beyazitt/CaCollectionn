using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities
{
    public class CodeRequests
    {
        [Key]
        public Guid Id { get; set; }
        public string CodeFor { get; set; }
        public int Code { get; set; }
    }
}
