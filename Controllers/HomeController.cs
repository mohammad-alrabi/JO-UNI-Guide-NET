using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JO_UNI_Guide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController (ApplicationDbContext context) 
        {
            _context = context;
        }
        //الصفحى الرئيسية للموقع تبعنا (الاحصائيات وافضل الجامعات وغيره)
        public async Task<IActionResult> Index()
        {
            //عشان يعرض البيانات بشكل جيد بالفرونت بس يرسل الاحصائيات 
            ViewBag.UniversitiesCount = await _context.Universities.CountAsync();
            ViewBag.FacultiesCount = await _context.Faculties.CountAsync();
            ViewBag.Courses = await _context.Courses.CountAsync();

            //جلب افضل 3 جامعات مثلا حسب الترتيب او التصنيف لعرضهم بالصفحة الرظيسية 
            var topUniversities = await _context.Universities
                                    .AsNoTracking()
                                    .OrderBy(n => n.Name)
                                    .Take(3)
                                    .ToListAsync();
            return View(topUniversities);
        }
        //صفحة تصفح كل الجامعات (مع فلترة وتقسيم صفحات الطلاب )
        public async Task<IActionResult> Universities(string searchString, string currentFilter, int? pageNumber) 
        {
            if (searchString != null) { pageNumber = 1; }
            else { searchString = currentFilter; }

            ViewData["CurrentFilter"] = searchString;

            var universities = _context.Universities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // الطالب بيبحث عن جامعة باسمها أو موقعها
                universities = universities.Where(u => u.Name.Contains(searchString)
                                                    || u.Location.Contains(searchString));
            }

            universities = universities.OrderBy(u => u.Name);
            int pageSize = 5; // عرض 6 جامعات (كروت) في كل صفحة

            return View(await JO_UNI_Guide.Helpers.PaginatedList<University>.CreateAsync(universities, pageNumber ?? 1, pageSize));
        }
        // 3. صفحة تفاصيل الجامعة (البروفايل الكامل اللي بيشوفه الطالب)
        public async Task<IActionResult> UniversityDetails(int? id)
        {
            if (id == null) return NotFound();

            // هون الداتا ثقيلة شوي، لازم نجيب الجامعة -> كلياتها -> أقسامها -> موادها
            // استخدام AsNoTracking هون ضروري جداً جداً عشان السرعة
            var university = await _context.Universities
                .Include(u => u.Faculties)
                    .ThenInclude(f => f.Departments)
                        .ThenInclude(d => d.Courses)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.University_ID == id);

            if (university == null) return NotFound();

            return View(university);
        }
        // صفحة الخطأ الافتراضية
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
