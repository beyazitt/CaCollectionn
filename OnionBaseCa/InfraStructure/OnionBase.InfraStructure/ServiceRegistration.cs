using Microsoft.Extensions.DependencyInjection;
using OnionBase.Application.Services;
using OnionBase.InfraStructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.InfraStructure
{
    public static class ServiceRegistration
    {
        public static void AddInfraStructureServices(this IServiceCollection services) 
        {
            services.AddScoped<IMailService,MailService>();
            services.AddScoped<IQRCodeService, QRCodeService>();



        }
    }
}
