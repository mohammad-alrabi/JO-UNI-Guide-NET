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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager , UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 1. فتح شاشة تسجيل الدخول
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string role = "Student")
        {
            // إذا كان المستخدم مسجل دخوله أصلاً، ليش نخليه يشوف صفحة الـ Login؟
            if (User.Identity.IsAuthenticated)
            {
                // نتحقق من دوره ونوجهه لمكانه الصح فوراً
                if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Dashboard", "Student");
                }
            }

            // إذا لم يكن مسجلاً، نرسل الدور للـ View لتجهيز النصوص والأزرار
            ViewBag.Role = role;
            return View();
        }

        // 2. تنفيذ عملية الدخول لما الأدمن يكبس "Login"
        // 1. بوابة دخول الطالب
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginSt(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    // منع الأدمن من دخول بوابة الطلاب (كودك الأصلي ممتاز)
                    if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "This login is for students only.");
                        ViewBag.Role = "Student";
                        return View("Login", model);
                    }

                    if (!user.IsOnboarded)
                    {
                        return RedirectToAction("Onboarding", "Student");
                    }

                    return RedirectToAction("Dashboard", "Student");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            ViewBag.Role = "Student";
            return View("Login", model);
        }

        // 2. بوابة دخول الإدارة (Admin & SuperAdmin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    // التحقق: يجب أن يكون أدمن أو سوبر أدمن
                    if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    // إذا كان طالب يحاول الدخول من هنا
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Access denied. You do not have administrative privileges.");
                    ViewBag.Role = "Admin";
                    return View("Login", model);
                }
                ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
            }
            ViewBag.Role = "Admin";
            return View("Login", model);
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // إنشاء المستخدم مع الحقول الجديدة (الاسم والمحافظة)
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    Governorate = model.Governorate, 
                    IsOnboarded = false // القيمة الافتراضية
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // 🚀 بعد التسجيل مباشرة نرسله للـ Onboarding ليكمل بياناته
                    return RedirectToAction("Onboarding", "Student");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
    }
}