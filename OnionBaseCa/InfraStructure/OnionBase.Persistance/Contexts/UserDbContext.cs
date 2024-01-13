using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnionBase.Domain.Entities;
using OnionBase.Domain.Entities.Common;
using OnionBase.Domain.Entities.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnionBase.Persistance.Contexts
{
    public class UserDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> UserDatas { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Campaigns> Campaigns { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<VisitedDatas> VisitedDatas { get; set; }
        public DbSet<CodeFors> CodeFors { get; set; }
        public DbSet<CodeRequests> CodeRequests { get; set; }
        public DbSet<ActivationLinks> ActivationLinks { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Sizes> Sizes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(av => av.ProductVariant)
                .WithMany()
                .HasForeignKey(av => av.ProductVariantId);

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var datas = ChangeTracker
                .Entries<BaseEntity>();
            foreach (var data in datas)
            {
                _ = data.State switch
                {
                    EntityState.Modified => data.Entity.UpdatedTime = DateTime.UtcNow.AddHours(3),
                    EntityState.Deleted => data.Entity.UpdatedTime = DateTime.UtcNow.AddHours(3),
                    EntityState.Added => data.Entity.CreatedTime = DateTime.UtcNow.AddHours(3),
                    EntityState.Unchanged => data.Entity.UpdatedTime = DateTime.UtcNow.AddHours(3)
                };
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
