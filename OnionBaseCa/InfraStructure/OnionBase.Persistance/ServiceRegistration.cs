using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnionBase.Application.Repositories;
using OnionBase.Application.Services;
using OnionBase.Domain.Entities.Identity;
using OnionBase.Persistance.Contexts;
using OnionBase.Persistance.Repositories;
using OnionBase.Persistance.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Persistance
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<UserDbContext>(options =>
            //    options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")));
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnionUser-Product;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=False;"));
            //Console.WriteLine("//////////////////////////");
            //Console.WriteLine(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
            services.AddScoped<IOrderReadRepository, OrderReadRepository>();
            services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
            services.AddScoped<IProductReadRepository, ProductReadRepository>();
            services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
            services.AddScoped<ViewService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStockUpdateService, StockUpdateService>();
        }
    }
}
