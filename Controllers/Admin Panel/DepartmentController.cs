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

        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
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

            if (!string.IsNullOrEmpty(searchString))
            {
                // البحث باسم القسم أو اسم الكلية التابع لها
                departments = departments.Where(d => d.DepartmentName.Contains(searchString) ||
                                                     d.Faculty.Name.Contains(searchString));
            }

            departments = departments.OrderBy(d => d.DepartmentName);

            int pageSize = 5; // عدد الأقسام في الصفحة الواحدة

            return View(await JO_UNI_Guide.Helpers.PaginatedList<Department>.CreateAsync(departments, pageNumber ?? 1, pageSize));
        }

        public IActionResult Create()
        {
            ViewBag.FacultyList = new SelectList(_context.Faculties, "Faculty_ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department added successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FacultyList = new SelectList(_context.Faculties, "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            ViewBag.FacultyList = new SelectList(_context.Faculties, "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Department_ID) return NotFound();

            ModelState.Remove("Faculty");
            ModelState.Remove("Courses");

            if (ModelState.IsValid)
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FacultyList = new SelectList(_context.Faculties, "Faculty_ID", "Name", department.Faculty_ID);
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
            if (department == null) return NotFound();

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}