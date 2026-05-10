using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JO_UNI_Guide.Service;
using JO_UNI_Guide.interfaces;


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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // التحقق من صحة الدرجة حسب نوع الشهادة
            var gradeValid = model.CertificateType switch
            {
                CertificateType.Tawjihi => model.OriginalGrade >= 0 && model.OriginalGrade <= 100,
                CertificateType.Saudi => model.OriginalGrade >= 0 && model.OriginalGrade <= 100,
                CertificateType.AmericanHighSchool => model.OriginalGrade >= 0 && model.OriginalGrade <= 4,
                CertificateType.IGCSE => model.OriginalGrade >= 0 && model.OriginalGrade <= 8,
                CertificateType.IB => model.OriginalGrade >= 1 && model.OriginalGrade <= 45,
                _ => model.OriginalGrade >= 0 && model.OriginalGrade <= 100
            };

            if (!gradeValid)
                ModelState.AddModelError("OriginalGrade",
                    "Please enter a valid grade for your certificate type.");

            if (!ModelState.IsValid)
                return View(model);

            user.CertificateType = model.CertificateType;
            user.OriginalGrade = model.OriginalGrade;
            var converter = GradeConverterFactory.GetConverter(model.CertificateType);
            user.EquivalentGrade = Math.Min(100, converter.Convert(model.OriginalGrade ?? 0));
            user.PreferredUniType = model.PreferredUniType;
            user.IsOnboarded = true;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!user.IsOnboarded)
                return RedirectToAction("Onboarding", "Student");

            // 🔴 حماية من null (مهم جداً)
            var equivalentGrade = user.EquivalentGrade ?? 0;

            // =========================
            // Matching Count
            // =========================
            var matchingMajorsCount = await _context.Departments
              .CountAsync(d => d.MinEquivalentGrade <= equivalentGrade);


            // =========================
            // Top Majors
            // =========================
            var topMajors = await _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                         .Where(d => d.MinEquivalentGrade <= equivalentGrade)
                .OrderByDescending(d => d.MinEquivalentGrade)
                .Take(5)
                .ToListAsync();

            // =========================
            // ViewModel
            // =========================
            var model = new StudentDashboardViewModel
            {
                Name = user.Name,
                GPA = equivalentGrade, // 🔥 أهم إصلاح (بدون null / بدون 1000%)
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
            if (user.EquivalentGrade == null)
                return RedirectToAction("Onboarding", "Student");

            var query = _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Where(d => d.MinEquivalentGrade <= user.EquivalentGrade.Value)
                .AsQueryable();

            if (user.PreferredUniType == "Public")
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Governmental);
            else if (user.PreferredUniType == "Private")
                query = query.Where(d => d.Faculty.University.Type == UniversityType.Private);

            var result = await query.OrderByDescending(d => d.MinEquivalentGrade).ToListAsync();
            var userId = _userManager.GetUserId(User);
            var userFavorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.DepartmentId)
                .ToListAsync();

            ViewBag.UserFavorites = userFavorites;

            foreach (var item in result)
            {
                _logger.LogInformation($"Department: {item.DepartmentName}, MinGPA: {item.MinEquivalentGrade}");
            }

            return View(result);
        }

        // for universities : 
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
            var user = await _userManager.GetUserAsync(User);

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
            if (user?.EquivalentGrade != null)
            {
                query = query.Where(d => d.MinEquivalentGrade <= user.EquivalentGrade);
            }

            //  الفلترة حسب نوع الجامعة
            if (searchViewModel.UniType.HasValue)
            {
                query = query.Where(d => d.Faculty.University.Type == searchViewModel.UniType.Value);
            }

            searchViewModel.Results = await query
                .OrderBy(d => d.MinEquivalentGrade)
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
                EquivalentGrade = user.EquivalentGrade,
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
            user.OriginalGrade = model.OriginalGrade;
            user.CertificateType = model.CertificateType;
            var converter = GradeConverterFactory
                .GetConverter(model.CertificateType);
            user.EquivalentGrade = Math.Min(100, converter.Convert(model.OriginalGrade ?? 0));
            user.CertificateType = model.CertificateType;
            user.OriginalGrade = model.OriginalGrade;
            user.PreferredUniType = model.PreferredUniType;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}