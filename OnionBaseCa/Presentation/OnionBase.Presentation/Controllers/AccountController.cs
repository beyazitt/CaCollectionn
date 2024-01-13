using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using OnionBase.Application.Repositories;
using OnionBase.Application.Services;
using OnionBase.Domain.Entities;
using OnionBase.Domain.Entities.Identity;
using OnionBase.Persistance.Contexts;
using OnionBase.Presentation.Commands;
using OnionBase.Presentation.Helpers;
using OnionBase.Presentation.Interfaces;
using OnionBase.Presentation.Models;
using OnionBase.Presentation.ViewModels;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Voice;
using static System.Net.Mime.MediaTypeNames;

namespace OnionBase.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private ILogger<AccountController> _logger;
        private UserDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISmsHelper _smsHelper;
        public AccountController(ILogger<AccountController> logger,
            UserDbContext dbContext,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager,
            IHttpClientFactory httpClientFactory,
            ISmsHelper smsHelper
            )
        //ITokenHandler tokenHandler)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _roleManager = roleManager;
            _httpClientFactory = httpClientFactory;
            _smsHelper = smsHelper;
            //_tokenHandler = tokenHandler;

        }
        [HttpGet]
        public IActionResult Login()
        {
            TempData["SuccessLogin"] = null;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                AppUser user = await _userManager.FindByEmailAsync(model.Email);
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)//Authentication başarılı
                {
                    //var model2 = new ProfileViewModel
                    //{
                    //    Email = user.Email,
                    //    Name = user.Name,
                    //    PhoneNumber = user.PhoneNumber
                    //};
                    if(user.EmailConfirmed != true)
                    {
                        ViewMessageViewModel mesaj = new ViewMessageViewModel()
                        {
                            Message = "Lütfen Mail adresiniz üzerinden hesabınızı aktifleştirin."
                        };
                        return RedirectToAction("Message", "Home", mesaj);
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["SuccessLogin"] = "Başarıyla giriş yapıldı;";
                        return RedirectToAction("Index", "Home");
                    }
                    
                }
                TempData["ErrorLogin"] = "Kullanıcı adı veya şifre hatalı";
                return View("Login");
            }

        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(SMS sms, RegisterViewModel model, SmsViewModel smsVM)
        
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", "İsim alanı boş bırakılamaz!");
                return View(model);
            }

            Random random = new Random();
            int code = random.Next(100000, 1000000);

            IdentityResult result = await _userManager.CreateAsync(new AppUser()
            {
                Email = model.Email,
                Name = model.Name,
                UserName = model.UserName,
                SmsCode = code,
                PhoneNumber = model.PhoneNumber,
            }, model.Password);

            if (result.Succeeded)
            {
                await _roleManager.CreateAsync(new AppRole("user"));
                AppUser user = await _userManager.FindByEmailAsync(model.Email);
                TempData["UserId"] = user.Id;
                var cart = new Cart
                {
                    UserId = user.Id
                };
                _dbContext.Carts.Add(cart);
                _dbContext.SaveChanges();
                try
                {
                    string fromMail = "b.beyazit83@gmail.com";
                    string fromPassword = "zdtfgdblvtqlqbad";
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromMail);
                    message.Subject = "CaCollection'a Hoşgeldiniz.";
                    message.To.Add(new MailAddress(fromMail));
                    string activationLink = Url.Action("ActivateAccount", "Account", new { userId = user.Id, activationCode = user.SmsCode }, Request.Scheme);

                    message.Body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-rbs5jf5trMN6d5Z7/6m9bIH6L5GTgZo7PCkEAgBrEL5pLvH8Cj3pFkkY3VI5S9em\" crossorigin=\"anonymous\">\r\n  <title>Bootstrap Secondary Alert</title>\r\n</head>\r\n<body>\r\n\r\n    <div class=\"alert alert-secondary\" role=\"alert\"  style=\"display: flex; flex-direction: column;\">\r\n        <div class=\"mb-2\">CanaCollection'a Hoşgeldiniz.</div>\r\n        <div class=\"mb-2\">Hesabınıza giriş yapabilmeniz için hesabınızın onaylanması gerekmektedir.</div>\r\n        <div class=\"mb-2\">Onaylamak için alttaki linke tıklayabilirsiniz.</div>\r\n        <div class=\"mb-2\">Güzel günler dileriz.</div>\r\n        <div class=\"mb-2\">\r\n            <!-- Your activation link here -->\r\n            " + activationLink + "\r\n        </div>\r\n      </div>\r\n\r\n      <!-- Bootstrap CSS and JS links -->\r\n      <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC\" crossorigin=\"anonymous\">\r\n      <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM\" crossorigin=\"anonymous\"></script>\r\n    </body>\r\n\r\n</html>";
                    message.IsBodyHtml = true;

                    // ... (remaining code)

                    message.IsBodyHtml = true;
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(fromMail, fromPassword),
                        EnableSsl = true
                    };
                    smtpClient.Send(message);
                    System.Threading.Thread.Sleep(3000); // 3000 milisaniye bekleyin (örneğin)

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Stack Trace: " + ex.StackTrace);
                }
                

                return View("Login");

            }

            // If user registration failed
            return View(model);
        }

        [HttpGet]
        public IActionResult ActivateAccount(string userId, int activationCode)
        {
            // Kullanıcıyı ve aktivasyon kodunu kontrol et
            // Örneğin, bu bilgileri veritabanında bir tabloda saklayarak kontrol edebilirsiniz.
            bool isActivationSuccessful = CheckActivation(userId, activationCode);
            var user = _dbContext.Users.Where(a => a.Id == userId).FirstOrDefault();
            if (isActivationSuccessful)
            {
                // Hesap başarıyla aktifleştirildiyse, istediğiniz işlemleri gerçekleştirin
                // Örneğin, bir teşekkür mesajı göstermek, giriş yapmış gibi yapmak, vb.
                ViewBag.Message = "Hesabınız başarıyla aktifleştirildi!";
                user.EmailConfirmed = true;
                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();

            }
            else
            {
                // Hesap aktifleştirme başarısızsa, istediğiniz işlemleri gerçekleştirin
                ViewBag.Message = "Hesap aktifleştirme başarısız!";
            }
            ViewMessageViewModel mesaj = new ViewMessageViewModel()
            {
                Message = ViewBag.Message,
            };
            return RedirectToAction("Message","Home",mesaj);
        }

        private bool CheckActivation(string userId, int activationCode)
        {
            var user = _dbContext.Users.Where(a => a.Id == userId).FirstOrDefault();
            if(user.SmsCode == activationCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpGet]
        public async Task<IActionResult> LogOut(LoginViewModel model)
        {
            var user = _dbContext.UserDatas.FirstOrDefault(x => x.Email == model.Email);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            var user = _dbContext.Users.Where(a => a.Email == resetPasswordViewModel.Email).FirstOrDefault();
            Random random = new Random();
            int code = random.Next(100000, 1000000);
            if (user != null)
            {
                string activationLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, activationCode = code }, Request.Scheme);
                string fromMail = "b.beyazit83@gmail.com";
                string fromPassword = "zdtfgdblvtqlqbad";
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Şifre Sıfırlama.";
                message.To.Add(new MailAddress(fromMail));
                message.Body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-rbs5jf5trMN6d5Z7/6m9bIH6L5GTgZo7PCkEAgBrEL5pLvH8Cj3pFkkY3VI5S9em\" crossorigin=\"anonymous\">\r\n  <title>Bootstrap Secondary Alert</title>\r\n</head>\r\n<body>\r\n\r\n    <div class=\"alert alert-secondary\" role=\"alert\"  style=\"display: flex; flex-direction: column;\">\r\n        <div class=\"mb-2\">CanaCollection'a Hoşgeldiniz.</div>\r\n    <div class=\"mb-2\"> Şifrenizi sıfırlamak alttaki linke tıklayabilirsiniz.</div>\r\n        <div class=\"mb-2\">Güzel günler dileriz.</div>\r\n        <div class=\"mb-2\">\r\n            <!-- Your activation link here -->\r\n            " + activationLink + "\r\n        </div>\r\n      </div>\r\n\r\n      <!-- Bootstrap CSS and JS links -->\r\n      <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC\" crossorigin=\"anonymous\">\r\n      <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM\" crossorigin=\"anonymous\"></script>\r\n    </body>\r\n\r\n</html>";
                message.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true
                };
                smtpClient.Send(message);
                System.Threading.Thread.Sleep(3000);
                ViewBag.Message = "E-posta adresinize sıfırlama linki gönderildi.";
                ViewMessageViewModel mesaj = new ViewMessageViewModel()
                {
                    Message = ViewBag.Message,
                };
                return RedirectToAction("Message", "Home", mesaj);
            }
            else
            {
                ViewBag.Message = "E-posta adresinize kayıtlı üyelik bulunamadı.";
                ViewMessageViewModel mesaj = new ViewMessageViewModel()
                {
                    Message = ViewBag.Message,
                };
                return RedirectToAction("Message", "Home", mesaj);
            }
            
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, int activationCode)
        {
            var email = _dbContext.Users.Where(a => a.Id == userId).FirstOrDefault();
            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
            {
                Email = email.Email
            };
            return View(resetPasswordViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel resetPasswordViewModel)
        {
            var user = _dbContext.Users.Where(a => a.Email == resetPasswordViewModel.Email).FirstOrDefault();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordViewModel.Password);
            if (!result.Succeeded)
            {
                ViewBag.Message = "Şifre yenileme başarısız.";
                ViewMessageViewModel mesaj = new ViewMessageViewModel()
                {
                    Message = ViewBag.Message,
                };
                return RedirectToAction("Message", "Home", mesaj);
            }


            return View("Login");
        }



        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var name = User.Identity.Name;
                var userDetail = await _userManager.FindByNameAsync(name);
                ProfileViewModel model = new ProfileViewModel
                {
                    Name = userDetail.Name,
                    Email = userDetail.Email,
                    PhoneNumber = userDetail.PhoneNumber,
                    ProfileImage = userDetail.ProfileImage
                };
                return View(model);
            }
            else
            {
                return View("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProfileUpdate()
        {
            var name = User.Identity.Name;
            var userDetail = await _userManager.FindByNameAsync(name);
            ProfileViewModel model = new ProfileViewModel
            {
                Name = userDetail.Name,
                Email = userDetail.Email,
                PhoneNumber = userDetail.PhoneNumber
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ProfileUpdate(ProfileViewModel model, IFormFile ProfileImage)
        {
            //using (var memoryStream = new MemoryStream())
            //{
            //    ProfileImage.CopyTo(memoryStream);
            //    string base64String = Convert.ToBase64String(memoryStream.ToArray());
            //    string fileName = Guid.NewGuid().ToString() + ".txt";
            //    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Image", fileName);
            //    System.IO.File.WriteAllText(filePath, base64String);
            //    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            //    currentUser.ProfileImage = base64String;
            //    var user = await _userManager.GetUserAsync(User);
            //    if (model.Name != null)
            //    {
            //        user.Name = model.Name;
            //    }
            //    if (model.Email != null)
            //    {
            //        user.Email = model.Email;
            //    }
            //    if (model.PhoneNumber != null)
            //    {
            //        user.PhoneNumber = model.PhoneNumber;
            //    }
            //    if (model.ProfileImage != null)
            //    {
            //        user.ProfileImage = base64String;
            //    }
            //    var result = await _userManager.UpdateAsync(user);
            //}

            //return RedirectToAction("Profile");
            if (ProfileImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    ProfileImage.CopyTo(memoryStream);
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());
                    string fileName = Guid.NewGuid().ToString() + ".txt";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Image", fileName);
                    System.IO.File.WriteAllText(filePath, base64String);
                    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                    currentUser.ProfileImage = base64String;
                }
            }

            var user = await _userManager.GetUserAsync(User);
            if (model.Name != null)
            {
                user.Name = model.Name;
            }
            if (model.Email != null)
            {
                user.Email = model.Email;
            }
            if (model.PhoneNumber != null)
            {
                user.PhoneNumber = model.PhoneNumber;
            }
            if (model.ProfileImage != null)
            {
                user.ProfileImage = model.ProfileImage;
            }
            var result = await _userManager.UpdateAsync(user);

            return RedirectToAction("Profile");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel createRoleViewModel)
        {
            var role = createRoleViewModel.roleName;
            AppRole Role = new AppRole(role);
            if (role != null)
            {
                var roleExist = await _roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    //create the roles and seed them to the database: Question 1
                    await _roleManager.CreateAsync(Role);
                    return RedirectToAction("CreateRole");
                }
                else
                {
                    return RedirectToAction("CreateRole");
                }
            }
            else
            {
                return RedirectToAction("CreateRole");
            }
        }

        [HttpGet]
        public IActionResult RoleManagment()
        {
            var users = _dbContext.UserDatas.ToList();
            var roles = _roleManager.Roles.ToList();
            var viewModel = new RoleManagmentViewModel
            {
                Users = users,
                Roles = roles
            };
            var viewModel2 = new CombinedModelsForRoleManagement
            {
                Model2 = viewModel
            };
            return View(viewModel2);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userName = viewModel.SelectedUser;
                var role = await _roleManager.FindByNameAsync(viewModel.SelectedRole);
                var user = _dbContext.Users.Where(x => x.Name == userName).FirstOrDefault();
                if (user != null && role != null)
                {
                    var result = await _userManager.AddToRoleAsync(user, role.Name);

                    if (result.Succeeded)
                    {
                        TempData["SuccessAssignedRole"] = "Role assigned successfully.";
                        TempData["MessageAssignedRole"] = $"The role '{role.Name}' has been assigned to the user '{viewModel.SelectedRole}'.";
                    }
                    else
                    {
                        TempData["ErrorRoleAssign"] = "Failed to assign role.";
                    }
                }
                else
                {
                    TempData["ErrorRoleFound"] = "User or role not found.";
                }
            }
            else
            {
                TempData["Error"] = "Invalid model state.";
            }

            return RedirectToAction("Admin", "AllProducts");
        }


        [HttpGet]
        public IActionResult SmsVerification()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SmsVerification(RegisterViewModel model,SmsViewModel smsVM)
        {
            var userr = _dbContext.UserDatas.Where(x => x.Email == model.Email).FirstOrDefault();
            string userId = TempData["UserId"].ToString();
            AppUser user = await _dbContext.Users.FindAsync(userId);
            try
            {
                if (ModelState.IsValid || user.SmsCode == smsVM.code)
                {
                    
                    user.SmsVerify = true;
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["SmsError"] = "Sms kodunuz yanlış, Lütfen tekrar deneyin.";
                if (ModelState.IsValid || user.SmsCode == smsVM.code)
                {
                    user.SmsVerify = true;
                    return View("Login");
                }
            }
            
            return View("Register");
        }


        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var name = User.Identity.Name;
            var userDetail = await _userManager.FindByNameAsync(name);
            var currentUsersOrders = _dbContext.Orders.Include(a => a.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ToList();
            return View(currentUsersOrders);
        }
    }
}
