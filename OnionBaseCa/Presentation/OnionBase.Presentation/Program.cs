using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OnionBase.Domain.Entities.Identity;
using OnionBase.Persistance;
using OnionBase.Persistance.Contexts;
using OnionBase.InfraStructure;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OnionBase.Presentation.Helpers;
using OnionBase.Presentation.Interfaces;
using Vonage;
using OnionBase.Persistance.Services;
using OnionBase.Domain.Entities;
using System;

namespace OnionBase.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddInfraStructureServices();
            builder.Services.AddScoped<ISmsHelper, SmsHelper>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Cookie.Name = "AuthCookie";
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Deny";
            });
            builder.Services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {

                var services = scope.ServiceProvider;

                System.Threading.Thread.Sleep(5000);
                var context = services.GetRequiredService<UserDbContext>();
                await InitializeCodeFors(context);
                await InitializeSize(context);
                context.Database.Migrate();
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var viewService = services.GetRequiredService<ViewService>();

                // Her uygulama baþladýðýnda bir kere çalýþtýr
                viewService.GenerateDailyViews();

                // Ardýndan her gün ayný saatte çalýþtýr
                var timer = new System.Threading.Timer(
                    callback: _ => viewService.GenerateDailyViews(),
                    state: null,
                    dueTime: TimeSpan.FromHours(24), // Uygulama baþladýktan 24 saat sonra
                    period: TimeSpan.FromHours(24)); // Her 24 saatte bir tekrarla
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                

                await CreateAdminRoleAndAssignUser(roleManager, userManager);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
        private static async Task InitializeCodeFors(UserDbContext context)
        {
            if (!context.CodeFors.Any())
            {
                context.CodeFors.AddRange(new List<CodeFors>
                {
                    new CodeFors { Id = Guid.NewGuid(), CodeFor = "Code1", Description = "Phone Number Verify" },
                    new CodeFors { Id = Guid.NewGuid(), CodeFor = "Code2", Description = "Password Reset" }
                });

                await context.SaveChangesAsync();
            }
        }
        private static async Task InitializeSize(UserDbContext context)
        {
            if (!context.Sizes.Any())
            {
                context.Sizes.AddRange(new List<Sizes>
                {
                    new Sizes{ SizeId = Guid.NewGuid(),Value = "70"},
                    new Sizes{ SizeId = Guid.NewGuid(),Value = "75"},
                    new Sizes{ SizeId = Guid.NewGuid(),Value = "80"},
                    new Sizes{ SizeId = Guid.NewGuid(),Value = "85"},
                    new Sizes{ SizeId = Guid.NewGuid(),Value = "90"},

                });
                await context.SaveChangesAsync();
            }
        }


        private static async Task CreateAdminRoleAndAssignUser(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            string role = "Admin";
            string adminUserEmail = "admin@admin.com";

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole(role));
            }

            var user = await userManager.FindByEmailAsync(adminUserEmail);

            if (user != null)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                IdentityResult result = await userManager.CreateAsync(new AppUser()
                {
                    Email = "admin@admin.com",
                    Name = "Admin",
                    UserName = "Admin",
                    PhoneNumber = "05050439008",
                }, "AskAdamiIcardi,");
                var user2 = await userManager.FindByEmailAsync(adminUserEmail);
                await userManager.AddToRoleAsync(user2, role);

                // Kullanýcý bulunamadý, burada yeni bir kullanýcý oluþturabilirsiniz.
                // Bu örnekte, böyle bir kullanýcýnýn zaten mevcut olduðunu varsayýyoruz.
            }
        }
    }
}
