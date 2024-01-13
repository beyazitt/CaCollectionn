using OnionBase.Domain.Entities;
using OnionBase.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Persistance.Services
{
    public class ViewService
    {
        private readonly UserDbContext _context;

        public ViewService(UserDbContext context)
        {
            _context = context;
        }

        public void GenerateDailyViews()
        {
            // Saat 00:00 olduğunda çalışacak bu metot
            DateTime now = DateTime.Now;
            var x = _context.VisitedDatas.ToList();
            if(x.Count == 0)
            {
                DateTime todayDate = now.Date;
                CreateDailyViews(todayDate);
            }
            else 
            {
                if (now.Hour == 0 && now.Minute == 0)
                {
                    // Bugünün tarihini al
                    DateTime todayDate = now.Date;
                    foreach (var her in _context.VisitedDatas.ToList())
                    {
                        if (her.locked == false)
                        {
                            her.locked = true;
                        };
                    };
                    _context.SaveChanges();

                    // Daha önce bugünün verileri oluşturulmuş mu kontrol et
                    if (!_context.VisitedDatas.Any(v => v.Date == todayDate))
                    {
                        // Bugünkü verileri oluştur
                        CreateDailyViews(todayDate);
                    }
                }
            }
            
        }

        private void CreateDailyViews(DateTime todayDate)
        {
            var list = _context.Products.ToList();
            foreach(var item in list)
            {
                var productid = item.ProductId.ToString();
                string productName = $"{productid}-{todayDate:dd.MM.yyyy}";
                var productData = new VisitedDatas
                {
                    Name = productName,
                    Date = todayDate,
                    View = 0,
                    locked = false,
                    ProductId = item.ProductId
                };
                _context.VisitedDatas.Add(productData);
            };
            // Örnek isimler
            string indexName = $"Index-{todayDate:dd.MM.yyyy}";
            
            
            // Yeni verileri oluştur
            var indexData = new VisitedDatas
            {
                Name = indexName,
                Date = todayDate,
                View = 0, // Başlangıç değeri 0 olarak ayarlandı, isteğe bağlı olarak başka bir değer de atanabilir.
                locked = false
            };

            

            // Veritabanına ekleyip kaydet
            _context.VisitedDatas.Add(indexData);
            
            _context.SaveChanges();
        }
    }
}
