using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Authorize(Roles = "Student")]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public FavoriteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int depId) // رقم التخصص الي رح يكبس عليه الطالب
        {
            //جيب الطالب 
            var userId = _userManager.GetUserId(User);

            if (!User.Identity.IsAuthenticated)  // عشان يتأكد اذا الطالب مسجل دخول او لا 
                return Unauthorized();
            // عشان يشوف اذا التخصص مضاف مسبقا ام لا 
            //موجود => object
            //مش موجود => null
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.DepartmentId == depId);

            bool isAdded;  // عشان اذا تمت الاضافة او الحذف

            if (existingFavorite != null) // في حال كان موجود التخصص بالمفضلة
            {
                _context.Favorites.Remove(existingFavorite); //احذفه من المفضلة 
                isAdded = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite // يعني التخصص مش موجود بالمفضلة لهيك ضيفه 
                {
                    UserId = userId,
                    DepartmentId = depId,
                    SavedDate = DateTime.Now
                });
                isAdded = true;
            }

            await _context.SaveChangesAsync();

            var count = await _context.Favorites   //لحساب عدد التخصصات الي بالمفضلة للطالب
                .CountAsync(f => f.UserId == userId);

            return Json(new  //برجعه للفرونت اند 
            {
                success = true,
                isAdded,
                count
            });
        }
        [HttpGet] 
        public async Task<IActionResult> MyFavorites() 
        { 
            var userId = _userManager.GetUserId(User); 
            var favorites = await _context.Favorites
                .Include(f => f.Department)
                    .ThenInclude(d => d.Faculty)
                        .ThenInclude(fac => fac.University)
                            .Where(f => f.UserId == userId)
                                .OrderByDescending(f => f.SavedDate)
                                    .ToListAsync();
            return View(favorites);
        }
        [HttpGet]
        public async Task<IActionResult> Compare(int[] departmentIds)
        {
            // التحقق من تسجيل الدخول
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            // التحقق من عدد الاختيارات
            if (departmentIds == null || departmentIds.Length < 2)
            {
                TempData["Error"] = "You must select at least 2 departments to compare.";
                return RedirectToAction("MyFavorites");
            }

            //  جلب التخصصات المختارة
            var departmentsToCompare = await _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Where(d => departmentIds.Contains(d.Department_ID))
                .AsNoTracking()
                .ToListAsync();

            //  تأكد فعلاً عندك 2 تخصص أو أكثر بعد الجلب
            if (departmentsToCompare.Count < 2)
            {
                TempData["Error"] = "Selected departments not found.";
                return RedirectToAction("MyFavorites");
            }

            // ترتيب حسب نفس اختيار المستخدم مو فقط GPA
            var sortedDepartments = departmentsToCompare
                .OrderBy(d => Array.IndexOf(departmentIds, d.Department_ID))
                .ToList();

            // تجهيز ViewModel
            var model = new ComparisonViewModel
            {
                SelectedDepartments = sortedDepartments
            };

            
            return View(model);
        }
    }
}
