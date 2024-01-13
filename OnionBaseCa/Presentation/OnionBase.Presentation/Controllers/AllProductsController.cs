using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using OnionBase.Application.Repositories;
using OnionBase.Application.Services;
using OnionBase.Domain.Entities;
using OnionBase.Domain.Entities.Identity;
using OnionBase.Persistance.Contexts;
using OnionBase.Persistance.Repositories;
using OnionBase.Presentation.DTOs;
using OnionBase.Presentation.ViewModels;
using System.Drawing;
using System.Security.Claims;
using System.Security.Policy;
using System.Xml;
using static OnionBase.Presentation.ViewModels.AddProductViewModel;

namespace OnionBase.Presentation.Controllers
{
    public class AllProductsController : Controller
    {
        private readonly IProductWriteRepository _productWriteRepository;
        private ILogger<AllProductsController> _logger;
        private UserDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProductReadRepository _productReadRepository;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IProductService _productService;
        private readonly IStockUpdateService _stockUpdateService;

        public AllProductsController(ILogger<AllProductsController> logger, UserDbContext dbContext, IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IProductService productService, IStockUpdateService stockUpdateService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _productService = productService;
            _stockUpdateService = stockUpdateService;
        }
        public IActionResult Index()
        {
            var result = _productReadRepository.GetAll()
                .ToList();
            var variants = _dbContext.ProductVariants.ToList();
            AllProductsIndexViewModel allProductsIndexViewModel = new AllProductsIndexViewModel
            {
                Products = result,
                ProductVariants = variants
            };
            return View(allProductsIndexViewModel);
        }
        [HttpGet("ProductDetail/{ProductCode}")]
        public IActionResult ProductDetail(int ProductCode)
        {
            var productWithAttributes = _productReadRepository
                                            .GetAll()
                                            .Where(x => x.ProductCode == ProductCode)
                                            .Include(av => av.ProductVariants)
                                            .ToList();
            var view = _dbContext.VisitedDatas.ToList();
            foreach (var her in view)
            {
                {
                    her.View += 1;
                }
            };
            AllProductsIndexViewModel allProductsIndexViewModel = new AllProductsIndexViewModel
            {
                Products = productWithAttributes,
                ProductVariants = _dbContext.ProductVariants.Where(a => a.ProductId == productWithAttributes.First().ProductId).ToList(),
                fromVariant = false
            };
            _dbContext.SaveChanges();
            if (productWithAttributes == null)
            {
                return NotFound();
            }

            return View(allProductsIndexViewModel);
        }

        [HttpGet("ProductDetail/{ProductId}/{Color}")]
        public IActionResult ProductDetail(Guid ProductId, String Color)
        {
            var product = _dbContext.Products.Where(a => a.ProductId == ProductId)
                .Include(av => av.ProductVariants)
                .ToList();
            var productVariant = _dbContext.ProductVariants.Where(a => a.ProductId == ProductId && a.Color == Color).ToList();
            if(productVariant.Count != 0)
            {
                AllProductsIndexViewModel allProductsIndexViewModel = new AllProductsIndexViewModel
                {
                    Products = product,
                    ProductVariants = productVariant,
                    fromVariant = false,
                    forColor = true,
                    selectedColor = Color
                };
                return View(allProductsIndexViewModel);

            }
            else
            {
                var productVariant2 = _dbContext.ProductVariants.Where(a => a.ProductId == ProductId && a.Size == Color).ToList();
                AllProductsIndexViewModel allProductsIndexViewModel = new AllProductsIndexViewModel
                {
                    Products = product,
                    ProductVariants = productVariant,
                    fromVariant = false,
                    forSize = true,
                    selectedSize = Color
                };
                return View(allProductsIndexViewModel);

            }

            
        }






        [HttpGet]
        [Route("AllProducts/ProductDetailFromQr/{productId}")]
        public async Task<IActionResult> ProductDetailFromQr(Guid productId)
        {
            var data = await _productService.QrCodeToProductAsync(productId);
            return File(data,"image/png");
        }



        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var user = _dbContext.UserDatas.Where(x => x.IsAdmin == true).ToList();
            if (user != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Profile", "Account");
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AddProductStep1()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProductStep1(AddProductViewModel addProductViewModel)
        {
            Random random = new Random();
            int code = random.Next(10000, 999999);

            if (ModelState.IsValid)
            {
                // Özelleştirilen metni oluştur
                string styledDescription = $"<font color=\"{addProductViewModel.ProductDescriptionColor}\"><strong style=\"font-family: {addProductViewModel.ProductDescriptionFont}; font-size: {addProductViewModel.ProductDescriptionFontSize};\">{addProductViewModel.ProductDescription}</strong></font>";

                using (var memoryStream = new MemoryStream())
                {
                    

                    var product = new Product
                    {
                        ProductName = addProductViewModel.ProductName,
                        ProductDescription = styledDescription,
                        Price = addProductViewModel.Price,
                        ProductCode = code,
                        ProductVariants = {}
                    };
                    addProductViewModel.Image.CopyTo(memoryStream);
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());
                    string fileName = Guid.NewGuid().ToString() + ".txt";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Image", fileName);
                    System.IO.File.WriteAllText(filePath, base64String);
                    product.Image = base64String;


                    await _productWriteRepository.AddAsync(product);
                    await _productWriteRepository.SaveAsync();
                    return RedirectToAction("AddProductStep2", new { ProductId = product.ProductId });
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = new List<string>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errors.Add($"{key} : {state.Errors.First().ErrorMessage}");
                    }
                }

                // Hataları yazdırabilir veya loglayabilirsiniz.
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AddProductStep2(Guid ProductId)
        {
            var product = _productReadRepository.GetAll().Where(x => x.Id == ProductId).FirstOrDefault();
            List<Sizes> liste = _dbContext.Sizes.ToList();
            AddProductViewModel addProductViewModel = new AddProductViewModel()
            {
                ProductId = ProductId,
                Sizes = liste
            };
            if (product == null)
            {
                // Hata mesajı gösterip uygun bir sayfaya yönlendirebilirsiniz
                TempData["AddProductStep2 Error"] = "Bir önceki sayfadaki ürün bulunamadı.";
                return View(addProductViewModel);
            }
            
            return View(addProductViewModel);
        }



        [HttpPost]
        public async Task<IActionResult> AddVariant(AddProductViewModel model, IFormCollection form)
        {
            try
            {
                var productId = model.ProductId; // Modelden productId alınır
                var colorCount = form.Keys.Count(key => key.StartsWith("Colors"));
                var product = _dbContext.Products.Where(a => a.ProductId == productId).FirstOrDefault();
                for (int i = 1; i <= colorCount; i++)
                {
                    if(i == 1)
                    {
                        var colors = form["Colors"];
                        var selectedSizes = form["SelectedSizes" + "[]"];
                        foreach (var size in selectedSizes)
                        {
                            var productVariant = new ProductVariant
                            {
                                ProductId = (Guid)productId,
                                Color = colors,
                                Size = size,
                                Stock = 0 // Varsayılan stok değeri
                            };
                            _dbContext.ProductVariants.Add(productVariant);
                            product.ProductVariants.Add(productVariant);
                        }
                    }
                    else
                    {
                        var colors = form["Colors" + i];
                        var selectedSizes = form["SelectedSizes" + i + "[]"];
                        foreach (var size in selectedSizes)
                        {
                            var productVariant = new ProductVariant
                            {
                                ProductId = (Guid)productId,
                                Color = colors,
                                Size = size,
                                Stock = 0 // Varsayılan stok değeri
                            };
                            _dbContext.ProductVariants.Add(productVariant);
                            product.ProductVariants.Add(productVariant);
                        }
                    }
                    

                    // Bu renk için seçilen bedenleri işle
                    
                }
                    // Varyasyonu veritabanına ekle

                // Değişiklikleri kaydet
                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Ürün başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                // Hata durumunda yanıt
                return Json(new { success = false, message = "Ürün kaydedilirken bir hata oluştu: " + ex.Message });
            }
        }





        [HttpGet]
        public IActionResult AddProductStep3(Guid ProductId)
        {
            var list = _dbContext.ProductVariants.Where(a => a.ProductId == ProductId).ToList();
            AddProductStep3ViewModel addProductStep3ViewModel = new AddProductStep3ViewModel()
            {
                ProductId = ProductId,
                productVariants = list
            };
            return View(addProductStep3ViewModel);
        }



        [HttpPost]
        public IActionResult AddImageToVariant(IFormFile image, Guid variantId)
        {

            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    image.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());
                    string fileName = variantId.ToString() + ".txt";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Image", fileName);
                    System.IO.File.WriteAllText(filePath, base64String);
                    ProductVariant variant = _dbContext.ProductVariants.Where(a => a.ProductVariantId == variantId).FirstOrDefault();
                    variant.Image = base64String;
                }
                _dbContext.SaveChanges();
                return Json(new { success = true, message = "Resim başarıyla yüklendi" });
            }
            else
            {
                return Json(new { success = false, message = "Ürün kaydedilirken bir hata oluştu: "});
            }
            // Burada resim işleme ve kaydetme işlemlerini yapın
            
        }





        [HttpPost]
        public async Task<JsonResult> UpdateStock([FromBody] StockUpdateModel model)
        {
            try
            {
                var variant = _dbContext.ProductVariants.Find(model.VariantId);
                if (variant == null)
                {
                    return Json(new { success = false, message = "Varyasyon bulunamadı" });
                }
                _stockUpdateService.AddStock(variant.ProductId,model.VariantId, model.Stock);
                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Stok başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }


        [HttpPost]
        public IActionResult AddProductStep3(AddProductStep3ViewModel addProductStep3ViewModel, Guid ProductId)
        {
            Product product = _dbContext.Products.Where(a => a.ProductId == ProductId).FirstOrDefault();
            foreach (var her in _dbContext.ProductVariants.Where(a => a.ProductId == ProductId).ToList())
            {
                product.TotalStock += her.Stock;
            }
            TempData["Success"] = "Başarıyla Eklendi";
            return RedirectToAction("Index"); // Index sayfasına yönlendirme örneği
        }

        






        [HttpGet]
        public async Task<IActionResult> Delete(string ProductName)
        {
            var product = await _productReadRepository.GetSingleAsync(x => x.ProductName == ProductName);

            if (product != null)
            {
                _productWriteRepository.Remove(product);
                await _productWriteRepository.SaveAsync();

                // Silme işlemi başarılıysa yönlendirme yapabilirsiniz
                TempData["Success"] = "Başarılı";
                TempData["Message"] = "Ürün başarıyla silindi";
            }
            else
            {
                // Ürün bulunamazsa, hata mesajıyla birlikte yönlendirme yapabilirsiniz
                TempData["Error"] = "Ürün bulunamadı";
            }

            return RedirectToAction("Index", "AllProducts");
        }


        [HttpGet]
        public IActionResult OrderOrQuestion(int ProductCode, double totalAmount )
        {
            ProductCodeDTO dto = new ProductCodeDTO
            {
                ProductCode = ProductCode
            };
            CombinedModelsForOrder model = new CombinedModelsForOrder()
            {
                Model2 = dto,
                price = totalAmount
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> OrderOrQuestion(CombinedModelsForOrder OrderVM)
        {
            var name = User.Identity.Name;
            var userDetail = await _userManager.FindByNameAsync(name);

            // Siparişi oluşturun
            var order = CreateOrderFromViewModel(OrderVM, userDetail);

            // Kampanya kodu kontrolü
            

            // Kullanıcının sepetini alın
            var cart = GetCartForUser(userDetail.Id);

            // CartItems'i sipariş ayrıntılarına ekleyin
            foreach (var cartItem in cart.CartItems)
            {
                var orderDetail = CreateOrderDetailFromCartItem(cartItem);
                orderDetail.Address = OrderVM.Model1.Address;
                orderDetail.City = OrderVM.Model1.City;
                orderDetail.PhoneNumber = OrderVM.Model1.Phone;
                order.OrderDetails.Add(orderDetail);
            }
            order.Shipping = false;
            var campaign = _dbContext.Campaigns.FirstOrDefault(c => c.CampaignCode == OrderVM.Model1.CampaignCode);
            if (campaign != null)
            {
                if (campaign.ValidUntil < DateTime.UtcNow)
                {
                    order.CampaignCode = OrderVM.Model1.CampaignCode;
                    order.TotalPrice *= (1 - (campaign.DiscountPercentage / 100));
                }
            }


            // Siparişi kaydedin
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            // Kullanıcının sepetini temizleyin
            ClearUserCart(userDetail.Id);

            TempData["Success"] = "Siparişiniz Alındı. Siparişinizin onaylanması için lütfen ödeme sayfasına geçiniz.";

            return RedirectToAction("Payment", "AllProducts", new { OrderId = order.OrderId, totalAmount = order.TotalPrice });
        }

        // Kullanıcının sepetini almak için yardımcı bir yöntem
        private Cart GetCartForUser(string userId)
        {
            var vart = _dbContext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefault(c => c.UserId == userId);
            return vart;
        }

        // Kullanıcının sepetini temizlemek için yardımcı bir yöntem
        private void ClearUserCart(string userId)
        {
            var cart = GetCartForUser(userId);
            if (cart != null)
            {
                _dbContext.CartItems.RemoveRange(cart.CartItems);
                _dbContext.Carts.Remove(cart);
                _dbContext.SaveChanges();
            }
        }

        // CartItem'dan OrderDetail oluşturmak için yardımcı bir yöntem
        private OrderDetail CreateOrderDetailFromCartItem(CartItem cartItem)
        {
            var orderDetail = new OrderDetail
            {
                ProductVariantId = cartItem.ProductVariantId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.ProductVariant.Product.Price * cartItem.Quantity  , // Ürünün fiyatını alabilirsiniz, fiyat nesnesine bağlı olarak
                DiscountedPrice = cartItem.ProductVariant.Product.Price,                                         // Diğer sipariş ayrıntılarını ayarlayabilirsiniz
                ProductVariant = _dbContext.ProductVariants.Where(a => a.ProductVariantId == cartItem.ProductVariantId).FirstOrDefault()
            };
            return orderDetail;
        }
        private Order CreateOrderFromViewModel(CombinedModelsForOrder OrderVM, AppUser userDetail)
        {
            var order = new Order()
            {
                OrderDate = DateTime.UtcNow,
                UserId = userDetail.Id,
                CampaignCode = OrderVM.Model1.CampaignCode,
                TotalPrice = OrderVM.price, // Bu satırda eksik kalmış
                OrderDetails = new List<OrderDetail>() // OrderDetails koleksiyonunu başlatın
            };

            return order;
        }



        [HttpGet]
        public IActionResult Payment(Guid OrderId, Product product, double totalAmount)
        {
            //ViewBag.OrderId = OrderId;
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderId == OrderId);
            OrderIdDTO orderIdDTO = new OrderIdDTO() 
            {
                orderId = OrderId,
                totalAmount = totalAmount
                
            };

            return View(orderIdDTO);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentApprove(Guid OrderId)
        {
            var currentOrder = _dbContext.Orders.Where(x => x.OrderId == OrderId).FirstOrDefault();
            var urun = _dbContext.OrderDetails.Where(x => x.OrderId == OrderId).ToList();
            foreach (var her in urun)
            {
                var product = _dbContext.ProductVariants.Where(a => a.ProductVariantId == her.ProductVariantId).FirstOrDefault();
                _stockUpdateService.RemoveStock(product.ProductId,product.ProductVariantId, her.Quantity);
                await _productWriteRepository.SaveAsync();

            };
            
            currentOrder.confirmationRequest = true;
            _dbContext.SaveChanges();
            TempData["Mesaj"] = "Onay isteğiniz gönderildi";
            return RedirectToAction("Profile", "Account");
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult PaymentConfirmation()     
        {
            var list = _dbContext.Orders.Include(av => av.OrderDetails).ToList();
            var model = new PaymentConfirmationDTO
            {
                orders = list
            };
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PaymentConfirmationPost([FromBody] PaymentConfirmationDTO pcDTO)
        {
            try
            {
                var currentOrder = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant) 
                    .Where(o => o.OrderId == pcDTO.OrderId)
                    .FirstOrDefaultAsync();

                if (currentOrder == null)
                {
                    // Sipariş bulunamazsa hata dönebilirsiniz
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                }

                currentOrder.isConfirmed = true;
                currentOrder.confirmationRequest = false;

                foreach (var orderDetail in currentOrder.OrderDetails)
                {
                    // Her bir sipariş detayını işle
                    var product = orderDetail.ProductVariant;

                    if (product == null)
                    {
                        // Ürün bulunamazsa hata dönebilirsiniz
                        return Json(new { success = false, message = "Ürün bulunamadı." });
                    }


                    // Eğer ürünün bedenleri varsa ve seçili bir beden varsa
                    if (product != null)
                    {
                        // Seçilen bedenin stok miktarını güncelle
                        var selectedSizeStock = product.Stock;
                        if (selectedSizeStock > 0)
                        {
                            await _stockUpdateService.RemoveStock(product.ProductId, product.ProductVariantId, orderDetail.Quantity);

                            // Güncellenmiş stok miktarını kaydet
                        }
                        else
                        {
                            // Stok yetersizse hata dönebilirsiniz
                            return Json(new { success = false, message = "Seçilen bedenin stok miktarı yetersiz." });
                        }
                    }
                    else
                    {
                        // Eğer ürün bedenleri yoksa veya seçili bir beden yoksa, toplam stoktan düş
                    }

                    // Değişiklikleri kaydet
                    _dbContext.SaveChanges();
                }

                TempData["Message"] = "Onaylandı.";
                return Json(new { success = true, message = "Onaylandı." });
            }
            catch (Exception ex)
            {
                // Hata durumu
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddShippingCode(Guid OrderId, PaymentConfirmationDTO pcDTO)
        {
            var her = _dbContext.Orders.Where(a => a.OrderId == OrderId).FirstOrDefault();
            her.Shipping = true;
            her.ShippingCode = pcDTO.shippingCode;
            _dbContext.SaveChanges();
            return RedirectToAction("PaymentConfirmation");
        }


        [HttpGet]
        public IActionResult AddCampaign()
        {
            return View();
        }

        private void AddToCart(String userId, Guid productId, string size, string color, int quantity)
        {
            var cart = _dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { 
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _dbContext.Carts.Add(cart);
                _dbContext.SaveChanges();
            }
            var productVariantId = _dbContext.ProductVariants.Where(a=> a.ProductId == productId && a.Size == size && a.Color == color).FirstOrDefault();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantId == productVariantId.ProductVariantId);
            if(cartItem != null)
            {
                if (cartItem.Beden == size)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        ProductVariantId = productVariantId.ProductVariantId,
                        Quantity = quantity,
                        CartId = cart.Id,
                        Beden = size,
                        Color = color,
                        ProductVariant = productVariantId,
                        ProductId = productId,
                        Product = _productReadRepository.GetWhere(a => a.ProductId == productId).ToList()
                    };
                    _dbContext.CartItems.Add(cartItem);
                }
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductVariantId = productVariantId.ProductVariantId,
                    Quantity = quantity,
                    CartId = cart.Id,
                    Beden = size,
                    Color = color,
                    ProductVariant = productVariantId,
                    ProductId = productId,
                    Product = _productReadRepository.GetWhere(a => a.ProductId == productId).ToList()
                };
                _dbContext.CartItems.Add(cartItem);
            };
            
                                                                                                                                                                            

            _dbContext.SaveChanges();
        }
        private void DeleteFromCart(String userId, Guid productVariantId, int quantity)
        {
            var cart = _dbContext.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == userId);
            var deleted = _dbContext.CartItems.Where(x => x.ProductVariantId == productVariantId).FirstOrDefault();
            _dbContext.CartItems.Remove(deleted);
            _dbContext.SaveChanges();
        }

        [HttpPost]
        public IActionResult AddToCart(string id, string size, string color)
        {
            Guid productId = Guid.Parse(id);

            AddToCart(User.FindFirstValue(ClaimTypes.NameIdentifier), productId, size,color, 1);

            return Json(new { status = "success" });
        }
        [HttpPost]
        public IActionResult DeleteFromCart(string id)
        {
            Guid productVariantId = Guid.Parse(id);

            DeleteFromCart(User.FindFirstValue(ClaimTypes.NameIdentifier), productVariantId, 1);

            return Json(new { status = "success" });
        }

        [HttpGet]
        public IActionResult ShoppingCart(String id)
        {
            var cartitems = _dbContext.CartItems
                .Where(x => x.Cart.UserId == id)
                .Include(av => av.ProductVariant)
                .ThenInclude(av => av.Product)
                .Include(ax => ax.Product)
                .ToList();
            var cart = _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == id);
            if(cart != null)
            {
                var cartId = _dbContext.Carts.FirstOrDefault(x => x.UserId == id).Id;

                ShoppingCartViewModel shoppingCartViewModel = new ShoppingCartViewModel()
                {
                    CartItems = cartitems,
                    cartId = cartId
                };
                return View(shoppingCartViewModel);
            };

            return View();
        }

        [HttpPost]
        public IActionResult UpdateCartItems(Guid productVariantId,Guid cartId, string color, string size)
        {
            try
            {
                // Burada, gelen color ve size değerlerini kullanarak sepeti güncelleyin
                // Örneğin, CartItem'ı bulun ve güncelleyin
                var cartItem = _dbContext.CartItems.FirstOrDefault(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);
                if (cartItem != null)
                {
                    cartItem.Color = color;
                    cartItem.Beden = size;
                    _dbContext.SaveChanges();
                }

                // Başarılı bir yanıt döndürün
                return Json(new { success = true, message = "Sepet başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajı ile yanıt döndürün
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult UpdateCartQuantity(Guid cartId, string action)
        {
            // productId'ye göre veritabanından ilgili ürünü bulun
            var cartItem = _dbContext.CartItems.Where(a => a.CartId == cartId).FirstOrDefault();

            // Miktarı güncelle
            if (action == "increase")
            {
                cartItem.Quantity++;
            }
            else if (action == "decrease" && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }

            // Veritabanını güncelle
            _dbContext.SaveChanges();

            return Json(new { success = true });
        }



        [HttpGet]
        public IActionResult Orders()
        {
            var list = _dbContext.Orders.Where(x => x.isConfirmed == true).Include(o => o.OrderDetails).ToList();
            return View(list);
        }


        [HttpGet]
        public IActionResult OrderDetail(Guid OrderId)
        {
            var list1 = _dbContext.Orders.Where(a => a.OrderId == OrderId).Include(b => b.OrderDetails).ToList();
            List<Guid> productId = new List<Guid>();
            List<OrderDetail> orderDetail = new List<OrderDetail>();
            List<Product> product = new List<Product>();
            foreach (var her in list1)
            {
                foreach (var a in her.OrderDetails)
                {
                    orderDetail.Add(a);
                    productId.Add(a.ProductVariantId);
                };
            };
            foreach (var her in productId)
            {
                product.Add(_dbContext.Products.Where(a => a.ProductId == her).FirstOrDefault());
            };
            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel()
            {
                Product = product,
                OrderDetail = orderDetail
            };
            return View(orderDetailViewModel);
        }



        //[HttpGet]
        //public IActionResult ShippingStage(Guid OrderId)
        //{
        //    // İlgili OrderId'ye sahip ürünleri getir
        //    var products = _dbContext.OrderDetails
        //        .Where(orderDetail => orderDetail.Order.isConfirmed && orderDetail.OrderId == OrderId) // İlgili siparişin onaylı olduğundan ve OrderId'ye sahip olduğundan emin olun
        //        .Select(orderDetail => orderDetail.Product)
        //        .ToList();

        //    return View(products);
        //}

        [HttpGet]
        public IActionResult ArrangeProducts()
        {
            var product = _dbContext.Products
                .Include(a => a.ProductVariants)
                .ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttributeFromProductAsync(Guid ProductVariantId, Guid ProductId)
        {
            // Silme işlemini gerçekleştirin
            bool result = await _stockUpdateService.DeleteAllStock(ProductId, ProductVariantId);

            if (result)
            {
                // Silme işlemi başarılıysa, success durumunu döndür
                return Json(new { status = "success" });
            }
            else
            {
                // Silme işlemi başarısızsa, error durumunu döndür
                return Json(new { status = "error" });
            }
        }

        [HttpGet]
        public IActionResult Chart(Guid productId)
        {
            var chartData = _dbContext.VisitedDatas
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Date)
                .ToList();
            int x = 0;
            var viewModel = new ChartViewModel
            {
                ProductId = productId,
            };
            foreach (var her in chartData)
            {
                if (x == chartData.Count)
                {
                    x = 5;
                };
                if (x == 5)
                {
                    break;
                }
                else
                {
                    viewModel.Date.Add(her.Date);
                    viewModel.Views.Add(her.View);
                    x++;
                }
            };

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult ChartOfIndex()
        {
            var chartData = _dbContext.VisitedDatas
                .Where(v => v.Name.StartsWith("Index"))
                .OrderBy(v => v.Date)
                .ToList();

            int x = 0;
            var viewModel = new ChartViewModel();
            foreach (var her in chartData)
            {
                if (x == chartData.Count)
                {
                    x = 5;
                };
                if (x == 5)
                {
                    break;
                }
                else
                {
                    viewModel.Date.Add(her.Date);
                    viewModel.Views.Add(her.View);
                    x++;
                }
            };

            return View("Chart",viewModel);
        }


    }
}
