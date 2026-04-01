using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static JO_UNI_Guide.Models.LoginModel;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AccountController : Controller
    {
        // سحبنا محرك تسجيل الدخول تبع Identity
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // 1. فتح شاشة تسجيل الدخول
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 2. تنفيذ عملية الدخول لما الأدمن يكبس "Login"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // هون Identity بتشتغل سحرها وبتشيك على الإيميل والباسوورد
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // إذا الداتا صح، بندخله على لوحة التحكم
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // التوجيه الافتراضي للداشبورد
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                else
                {
                    // إذا الباسوورد غلط
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // إذا الفورم فيه مشكلة بنرجعه لنفس الشاشة
            return View(model);
        }

        // 3. تسجيل الخروج
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Identity بتدمر الجلسة وبتحذف الـ Cookie
            await _signInManager.SignOutAsync();

            // بنرجعه على الصفحة الرئيسية للزوار
            return RedirectToAction("Index", "Home");
        }
    }
}