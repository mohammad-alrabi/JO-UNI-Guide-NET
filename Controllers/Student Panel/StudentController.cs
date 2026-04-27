using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<StudentController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Onboarding()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (user.IsOnboarded)
                return RedirectToAction("Dashboard", "Student");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onboarding(OnboardingViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            user.GPA = model.GPA;
            user.TawjihiTrack = model.TawjihiTrack;
            user.PreferredUniType = model.PreferredUniType;
            user.IsOnboarded = true;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return RedirectToAction("Dashboard", "Student");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!user.IsOnboarded)
                return RedirectToAction("Onboarding", "Student");

            // FIX: نفس منطق ProposedMajors — يأخذ Track بعين الاعتبار عشان الأرقام تتطابق
            var matchingMajorsCount = await _context.Departments
                .CountAsync(d => d.AcceptanceRate <= user.GPA
                    && (d.RequiredTrack == null || d.RequiredTrack == user.TawjihiTrack));

            // FIX: Include موجود + نفس شرط الـ Track
            var topMajors = await _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Where(d => d.AcceptanceRate <= user.GPA
                    && (d.RequiredTrack == null || d.RequiredTrack == user.TawjihiTrack))
                .OrderByDescending(d => d.AcceptanceRate)
                .Take(5)
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                Name = user.Name,
                GPA = user.GPA,
                MatchingMajorsCount = matchingMajorsCount,
                TopMajors = topMajors
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ProposedMajors()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // FIX: حماية ضد null قبل الـ cast
            if (user.GPA == null)
                return RedirectToAction("Onboarding", "Student");

            var query = _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Where(d => d.AcceptanceRate <= user.GPA.Value
                    && (d.RequiredTrack == null || d.RequiredTrack == user.TawjihiTrack))
                .AsQueryable();

            if (user.PreferredUniType == "Public")
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Governmental);
            else if (user.PreferredUniType == "Private")
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Private);

            var result = await query.OrderByDescending(d => d.AcceptanceRate).ToListAsync();
            var userId = _userManager.GetUserId(User);
            var userFavorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.DepartmentId)
                .ToListAsync();

            ViewBag.UserFavorites = userFavorites;
            foreach (var item in result)
            {
                _logger.LogInformation($"Department: {item.DepartmentName}, MinGPA: {item.AcceptanceRate}");
            }

            return View(result);
        }

        // for universities : 
        // Task for Bhaa 
        [HttpGet]
        public async Task<IActionResult> Universities()
        {
            var allUniversities = await _context.Universities
                .AsNoTracking()
                .ToListAsync();

            var model = new UniversitiesListViewModel
            {
                PublicUniversities = allUniversities.Where(u => u.Type == UniversityType.Governmental).ToList(),
                PrivateUniversities = allUniversities.Where(u => u.Type == UniversityType.Private).ToList(),
            };
            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> UniversityDetails(int id)
        {
            var university = await _context.Universities
                .Include(u => u.Faculties)
                    .ThenInclude(f => f.Departments)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.University_ID == id);

            if (university == null) return NotFound();

            return View(university);
        }
        [Authorize]
        public async Task<IActionResult> MajorDetails(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Include(d => d.Courses)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Department_ID == id);

            if (department == null) return NotFound();
            return View(department);
        }
        public async Task<IActionResult> SmartSearch(SmartSearchViewModel searchViewModel)
        {
            var query = _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .AsQueryable();

            //  الفلترة حسب النص (اسم التخصص أو الجامعة أو الكلية)
            if (!string.IsNullOrEmpty(searchViewModel.Keyword))
            {
                query = query.Where(d => d.DepartmentName.Contains(searchViewModel.Keyword) ||
                                     d.Faculty.University.Name.Contains(searchViewModel.Keyword));
            }
            //  الفلترة حسب سعر الساعة
            if (searchViewModel.MaxHourPrice.HasValue)
            {
                query = query.Where(d => d.HourPrice <= searchViewModel.MaxHourPrice.Value);
            }
            //  الفلترة حسب المعدل (ليش هو سمارت؟ لأنه بطلع بس اللي بقبله الطالب)
            if (searchViewModel.StudentGPA.HasValue)
            {
                query = query.Where(d => d.AcceptanceRate <= searchViewModel.StudentGPA.Value);
            }
            //  الفلترة حسب نوع الجامعة
            if (searchViewModel.UniType.HasValue)
            {
                query = query.Where(d => d.Faculty.University.Type == searchViewModel.UniType.Value);
            }
            searchViewModel.Results = await query
                .OrderBy(d => d.AcceptanceRate)
                .ToListAsync();
            return View(searchViewModel);
        }
        [HttpGet]
        public async Task<JsonResult> GetSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<object>());

            var suggestions = await _context.Departments
                .Where(d => d.DepartmentName.Contains(term))
                .Select(d => new { id = d.Department_ID, name = d.DepartmentName })
                .Take(5) // نكتفي بـ 5 اقتراحات
                .ToListAsync();

            return Json(suggestions);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Governorate = user.Governorate,
                GPA = user.GPA,
                TawjihiTrack = user.TawjihiTrack,
                PreferredUniType = user.PreferredUniType,
                FavoritesCount = await _context.Favorites
                    .CountAsync(f => f.UserId == user.Id)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.Name = model.Name;
            user.Governorate = model.Governorate;
            user.GPA = model.GPA;
            user.TawjihiTrack = model.TawjihiTrack;
            user.PreferredUniType = model.PreferredUniType;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}