using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JO_UNI_Guide.Controllers
{
    public class AccountController : Controller
    {
        // سحبنا محرك تسجيل الدخول تبع Identity
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager , UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 1. فتح شاشة تسجيل الدخول
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string role = "Student")
        {
            if (User.Identity.IsAuthenticated) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            ViewBag.Role = role; // نرسل الدور للـ View عشان نغير النصوص
            return View();
        }

        // 2. تنفيذ عملية الدخول لما الأدمن يكبس "Login"
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // 1. محاولة تسجيل الدخول
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // 2. إذا كان هناك رابط قديم (ReturnUrl) يرجع له
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // 3. السحر هنا: التمييز بين الأدمن والطالب
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        // إذا كان طالب، بنبعته على الـ Student Layout اللي صممناها
                        return RedirectToAction("Dashboard", "Student");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Identity بتدمر الجلسة وبتحذف الـ Cookie
            await _signInManager.SignOutAsync();

            // بنرجعه على الصفحة الرئيسية للزوار
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() 
        {
            // بشيك على اليوزر ليحدد هو ادمن ولا طالب 
            if (User.Identity.IsAuthenticated)
            {
               //في حال كان طالب وديه على StudentController
               //في حال كان ادمن وديه على AdminController
                return RedirectToAction("Dashboard", User.IsInRole("Admin") ? "Admin" : "Student");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register (RegisterViewModel model) 
        {
            if (ModelState.IsValid) 
            {
                //create new user
                var user = new IdentityUser { UserName = model.Name, Email = model.Email };

                //to save in database and try to save the password eyncrpted
                var result = await _userManager.CreateAsync(user , model.Password);
                if (result.Succeeded) 
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Student");
                }
                // إضافة أخطاء Identity (مثل: الباسوورد ضعيفة أو الإيميل مكرر) للـ Validation
                foreach (var error in result.Errors) 
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
                return View(model);
        }
    }
}