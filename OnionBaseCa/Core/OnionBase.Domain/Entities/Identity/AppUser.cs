using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public int? SmsCode { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool? SmsVerify { get; set; }
        public string? ProfileImage { get; set; }
        public ICollection<Order> orders { get; set; }
        public AppUser() : base()
        {
        }
        public int userRate { get; set; }

        //public AppUser(string roleName) : base(roleName)
        //{
        //}
    }
}
