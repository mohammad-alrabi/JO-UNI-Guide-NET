using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber, decimal? minRate, decimal? maxPrice, UniversityType? uniType)
        {
            // حفظ الفلاتر الجديدة في الـ View عشان تضل ظاهرة في المربعات النصية والقائمة المنسدلة
            ViewData["MinRate"] = minRate;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SelectedUniType"] = uniType; // حفظ النوع المختار للـ View

            // إذا تغير أي فلتر (بحث، معدل، سعر، أو نوع جامعة) نرجع للصفحة الأولى
            if (searchString != null || minRate != null || maxPrice != null || uniType != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            // جلب البيانات مع العلاقات الضرورية (الكلية والجامعة لفلترة النوع)
            var departments = _context.Departments
                .Include(d => d.Faculty)
                   .ThenInclude(f => f.University) // ضروري جداً لإضافة الجامعة عشان نفلتر بالـ Type
                .AsNoTracking()
                .AsQueryable();

            // 1. البحث النصي (الاسم أو الكلية)
            if (!string.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d => d.DepartmentName.Contains(searchString) ||
                                                     d.Faculty.Name.Contains(searchString));
            }

            // 2. الفلترة حسب الحد الأدنى للمعدل (اختياري)
            if (minRate.HasValue)
            {
                // تحويل minRate من decimal إلى double ليطابق نوع الحقل في قاعدة البيانات
                departments = departments.Where(d => d.AcceptanceRate >= (decimal)minRate.Value);
            }

            // 3. الفلترة حسب الحد الأقصى لسعر الساعة (اختياري)
            if (maxPrice.HasValue)
            {
                departments = departments.Where(d => d.HourPrice <= maxPrice.Value);
            }

            // 4. الفلترة حسب نوع الجامعة (حكومية/خاصة) - مضاف حديثاً
            if (uniType.HasValue)
            {
                departments = departments.Where(d => d.Faculty.University.Type == uniType.Value);
            }

            departments = departments.OrderBy(d => d.DepartmentName);

            int pageSize = 5;
            return View(await JO_UNI_Guide.Helpers.PaginatedList<Department>.CreateAsync(departments, pageNumber ?? 1, pageSize));
        }

        public IActionResult Create()
        {
            // جلب الكليات مع الجامعات الخاصة بها لتمييزها
            var faculties = _context.Faculties.Include(f => f.University).OrderBy(f => f.Name).ToList();

            // دمج اسم الكلية مع اسم الجامعة في نص واحد يظهر للأدمن
            ViewBag.FacultyList = new SelectList(faculties.Select(f => new {
                Faculty_ID = f.Faculty_ID,
                DisplayName = $"{f.Name} - ({f.University.Name})"
            }), "Faculty_ID", "DisplayName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentName,Faculty_ID,AcceptanceRate,HourPrice,TotalCreditHours")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Department '{department.DepartmentName}' added successfully.";
                return RedirectToAction(nameof(Index));
            }

            // إعادة جلب القائمة في حال فشل الـ Validation
            var faculties = _context.Faculties.Include(f => f.University).OrderBy(f => f.Name).ToList();
            ViewBag.FacultyList = new SelectList(faculties.Select(f => new {
                Faculty_ID = f.Faculty_ID,
                DisplayName = $"{f.Name} - ({f.University.Name})"
            }), "Faculty_ID", "DisplayName", department.Faculty_ID);

            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            // ترتيب الكليات أبجدياً يسهل العمل على الأدمن
            ViewBag.FacultyList = new SelectList(_context.Faculties.OrderBy(f => f.Name), "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Department_ID,DepartmentName,Faculty_ID,AcceptanceRate,HourPrice,TotalCreditHours")] Department department)
        {
            if (id != department.Department_ID) return NotFound();

            // هدول السطرين ممتازين، بخلوا الـ ModelState يتجاهل العلاقات عشان يضل Valid
            ModelState.Remove("Faculty");
            ModelState.Remove("Courses");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Changes to '{department.DepartmentName}' saved successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Departments.Any(e => e.Department_ID == department.Department_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // إعادة تحميل القائمة المنسدلة في حال فشل الـ Validation
            ViewBag.FacultyList = new SelectList(_context.Faculties.OrderBy(f => f.Name), "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments
                .Include(d => d.Faculty)
                .FirstOrDefaultAsync(d => d.Department_ID == id);

            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                string deptName = department.DepartmentName; // حفظ الاسم قبل الحذف للرسالة
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"The department '{deptName}' and all its associated data have been deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            // 1. التحقق إذا كان الـ ID المرسل فارغاً
            if (id == null)
            {
                return NotFound();
            }

            // 2. جلب القسم مع كافة العلاقات المرتبطة به
            var department = await _context.Departments
                .Include(d => d.Faculty)              // جلب الكلية التابع لها القسم
                    .ThenInclude(f => f.University)   // جلب الجامعة التابعة لها الكلية (سلسلة الربط)
                .Include(d => d.Courses)              // جلب قائمة المواد الدراسية التابعة لهذا القسم
                .AsNoTracking()                       // تحسين الأداء لأننا فقط نعرض بيانات ولا نعدلها
                .FirstOrDefaultAsync(m => m.Department_ID == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }
    }
}