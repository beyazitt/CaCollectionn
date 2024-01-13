using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnionBase.Persistance.Contexts;
using OnionBase.Presentation.Models;
using OnionBase.Presentation.ViewModels;
using System.Diagnostics;

namespace OnionBase.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, UserDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            foreach (var her in _dbContext.VisitedDatas.ToList())
            {
                if(her.locked == false)
                {
                    if (her.Name.StartsWith("Index"))
                    {
                        her.View += 1;
                    };  
                };
            };
            _dbContext.SaveChanges();
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contract()
        {
            return View();
        }

        public IActionResult DeliveryAndReturn()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Message(ViewMessageViewModel viewMessageViewModel)
        {
            return View(viewMessageViewModel);
        }

    }
}