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

        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber, decimal? minRate, decimal? maxPrice)
        {
            // حفظ الفلاتر الجديدة في الـ View عشان تضل ظاهرة في المربعات النصية
            ViewData["MinRate"] = minRate;
            ViewData["MaxPrice"] = maxPrice;

            if (searchString != null || minRate != null || maxPrice != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var departments = _context.Departments
                .Include(d => d.Faculty)
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
                departments = departments.Where(d => d.AcceptanceRate >= minRate.Value);
            }

            // 3. الفلترة حسب الحد الأقصى لسعر الساعة (اختياري)
            if (maxPrice.HasValue)
            {
                departments = departments.Where(d => d.HourPrice <= maxPrice.Value);
            }

            departments = departments.OrderBy(d => d.DepartmentName);

            int pageSize = 5;
            return View(await JO_UNI_Guide.Helpers.PaginatedList<Department>.CreateAsync(departments, pageNumber ?? 1, pageSize));
        }

        public IActionResult Create()
        {
            // جلب الكليات لعرضها في القائمة المنسدلة
            ViewBag.FacultyList = new SelectList(_context.Faculties.OrderBy(f => f.Name), "Faculty_ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // أضفنا TotalCreditHours وتأكدنا من اسم AcceptanceRate ليتطابق مع الموديل
        public async Task<IActionResult> Create([Bind("DepartmentName,Faculty_ID,AcceptanceRate,HourPrice,TotalCreditHours")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                // رسالة نجاح تظهر للأدمن
                TempData["Success"] = $"Department '{department.DepartmentName}' added successfully.";
                return RedirectToAction(nameof(Index));
            }

            // في حال وجود خطأ في البيانات، نرجع الكليات للقائمة المنسدلة مرة أخرى
            ViewBag.FacultyList = new SelectList(_context.Faculties.OrderBy(f => f.Name), "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments
                .Include(d => d.Faculty)
                    .ThenInclude(f => f.University)
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(m => m.Department_ID == id);

            if (department == null) return NotFound();

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
        // 1. تعديل الحقول داخل الـ Bind لتطابق الموديل الجديد تماماً
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
    }
}