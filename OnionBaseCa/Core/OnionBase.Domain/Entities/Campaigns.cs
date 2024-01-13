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
    public class Campaigns : BaseEntity
    {
        [Key]
        public string CampaignCode { get; set; }
        public double DiscountPercentage { get; set; } // yüzde olarak indirim oranı
        public DateTime ValidUntil { get; set; }

    }
}
