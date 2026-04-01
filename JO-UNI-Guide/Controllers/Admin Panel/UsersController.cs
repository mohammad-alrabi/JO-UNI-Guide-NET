using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "SuperAdmin")] 
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // حقن الخدمات الجاهزة من Identity
        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. عرض كل المستخدمين بالداتا بيز
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // 2. فتح شاشة إضافة مستخدم
        public IActionResult Create()
        {
            // بنبعت قائمة بالرتب الموجودة (Admin, Student) عشان تطلع Dropdown
            var allowedRoles = new List<string> { "Admin", "Student" };
            ViewData["Role"] = new SelectList(allowedRoles);
            return View();
        }

        // 3. تنفيذ الإضافة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                // بنجهز حساب جديد
                var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };

                // بنحفظه بالداتا بيز مع الباسوورد (النظام لحاله بيشفر الباسوورد!)
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // إذا نجحت الإضافة، بنعطيه الرتبة اللي اخترناها من الشاشة
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["Success"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // لو في خطأ (مثلاً الباسوورد ضعيف)، بنرجعه للشاشة مع رسالة الخطأ
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            var allowedRoles = new List<string> { "Admin", "Student" };
            ViewData["Roles"] = new SelectList(allowedRoles);
            return View(model);
        }
        public async Task<IActionResult>Edit (string id) 
        {
            if(id  == null) {  return NotFound(); }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }
            //جيب الرتبة تبعته
            var currentRoles = await _userManager.GetRolesAsync(user);

            var allowedRoles = new List<string> { "Admin", "Student" };
            ViewData["Role"] = new SelectList(allowedRoles, currentRoles.FirstOrDefault());
            ViewBag.CurrentRole = currentRoles.FirstOrDefault();
            ViewBag.UserEmail = user.Email;
            ViewBag.UserId = user.Id;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Edit(string id , string role) 
        {
           var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }
            //لمنع تعديل على الsuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
            if (isSuperAdmin) 
            {
                TempData["Error"] = "Cannot modify SuperAdmin role.";
                return RedirectToAction(nameof(Index));
            }
            //شيل الرتبة القديمة وحط وحدة جديدة 
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, currentRoles);

            TempData["Success"] = "User role updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult>Delete(string id)
        {
            if(id  == null) { return NotFound(); }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }

            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
            if (isSuperAdmin) 
            {
                TempData["Error"] = "Cannot delete SuperAdmin.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        [HttpPost , ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id) 
        {
            // منع المستخدم من حذف نفسه 
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId) 
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }
            // منع حذف السوبر أدمن
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
            if (isSuperAdmin)
            {
                TempData["Error"] = "Cannot delete SuperAdmin.";
                return RedirectToAction(nameof(Index));
            }
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));


        }
    }
}