using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Authorize (Roles ="Student")]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        
        public StudentController(UserManager<ApplicationUser> userManager , ApplicationDbContext context) 
        {
            _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Onboarding()
        {
            var user = await _userManager.GetUserAsync(User);

            // حماية: إذا المستخدم مش مسجل دخول
            if (user == null)
                return RedirectToAction("Login", "Account");

            // إذا خلص Onboarding ما يرجع لهون
            if (user.IsOnboarded)
                return RedirectToAction("Dashboard", "Student");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onboarding(OnboardingViewModel model)
        {
            //  تحقق من صحة البيانات
            if (!ModelState.IsValid)
                return View(model);

            // جلب المستخدم
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // تحديث بيانات المستخدم
            user.GPA = model.GPA;
            user.TawjihiTrack = model.TawjihiTrack;
            user.PreferredUniType = model.PreferredUniType;
            user.IsOnboarded = true;

            // حفظ في الداتابيس
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Dashboard", "Student");
            }

            // إذا فشل الحفظ، نضيف الأخطاء
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            //  حماية: إذا المستخدم ما عمل Onboarding
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!user.IsOnboarded)
                return RedirectToAction("Onboarding", "Student"); // وجهه لـ كنترولر الطالب
            // حساب عدد التخصصات المناسبة
            var matchingMajorsCount = await _context.Departments
                .CountAsync(d => d.MinGPA <= user.GPA);

            // أفضل تخصصات مثلاً أول 5
            var topMajors = await _context.Departments
                .Where(d => d.MinGPA <= user.GPA)
                .OrderByDescending(d => d.MinGPA) // الأقرب لمعدل الطالب
                .Take(5)
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                Name = user.Name,
                GPA = (double?)user.GPA,
                MatchingMajorsCount = matchingMajorsCount,
                TopMajors = topMajors
            };

            return View(model);
        }
        //ترجع التخصصات المناسبة للطالب بناءً على معدله وتفضيلاته
        [HttpGet]
        public async Task<IActionResult> ProposedMajors()
        {
            //  جلب بيانات الطالب الحالي
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return RedirectToAction("Login", "Account");

            //فلترة التخصصات بناءً على معدل الطالب وفرعه
            // نستخدم الـ Include لجلب بيانات الجامعة والكلية مع التخصص في طلب واحد
            var query = _context.Departments
                .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                .Where(d => d.MinGPA <= (double)user.GPA) 
                .AsQueryable();

            // فلترة إضافية بناءً على "تفضيل الجامعة" (حكومي/خاص) الذي حدده في الـ Onboarding
            if (user.PreferredUniType == "Public")
            {
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Governmental);
            }
            else if (user.PreferredUniType == "Private")
            {
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Private);
            }

            // ترتيب النتائج حسب المعدل الأنسب (من الأعلى للأقل)
            var result = await query.OrderByDescending(d => d.MinGPA).ToListAsync();

            return View(result);
        }
    }
}
