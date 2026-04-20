using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class CourseController : Controller
    {
       private readonly ApplicationDbContext _context;
        public CourseController (ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task<bool>CourseExists(int id) 
        {
            return await _context.Courses.AnyAsync(e =>e.Course_ID == id);
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
            // course => Department => Faculty => university
            var courses = _context.Courses
                .Include(e => e.Department)
                    .ThenInclude(e => e.Faculty)
                        .ThenInclude(e => e.University)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c =>
                    c.Course_Name.Contains(searchString) ||
                    c.Department.DepartmentName.Contains(searchString) ||
                    c.Department.Faculty.Name.Contains(searchString) ||          
                    c.Department.Faculty.University.Name.Contains(searchString)  
                );
            }
            courses = courses.OrderBy(c => c.Course_Name);
            int pageSize = 5;
            return View(await JO_UNI_Guide.Helpers.PaginatedList<Course>.CreateAsync(
                        courses, pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> Create() 
        {
            var department = await _context.Departments
                .AsNoTracking()
                .OrderBy(n =>n.DepartmentName)
                .ToListAsync();
            ViewData["Department_ID"] = new SelectList(department, "Department_ID", "DepartmentName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Course_ID,Course_Name,Details,Department_ID")]Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The course has been successfully added.";
                return RedirectToAction(nameof(Index));
            }
            var department = await _context.Departments
                .AsNoTracking()
                .OrderBy (n =>n.DepartmentName)
                .ToListAsync();
            ViewData["Department_ID"] = new SelectList(department, "Department_ID", "DepartmentName" ,course.Department_ID);
            return View(course);
        }
        public async Task<IActionResult> Details(int? id) 
        {
            if (id == null)
                return NotFound();
            var course = await _context.Courses
                .Include(d=>d.Department)
                .ThenInclude(f=>f.Faculty)
                .ThenInclude(u=>u.University)
                .FirstOrDefaultAsync(i => i.Course_ID == id);

            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        public async Task<IActionResult> Edit(int? id) 
        {
            if(id == null)
                return NotFound();
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();
            var department = await _context.Departments.AsNoTracking().OrderBy (n =>n.DepartmentName).ToListAsync();
            ViewData["Department_ID"] = new SelectList(department, "Department_ID", "DepartmentName", course.Department_ID);
            return View(course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("Course_ID,Course_Name,Details,Department_ID")]Course course)
        {
            if(id !=course.Course_ID)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "The course has been successfully modified.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if(! await CourseExists(course.Course_ID))
                    {

                        return NotFound();
                    }

                    else
                    {
                        throw; 
                    }  
                }
                    return RedirectToAction(nameof(Index));
            }
                var department = await _context.Departments.AsNoTracking().OrderBy(n=>n.DepartmentName).ToListAsync();
                ViewData["Department_ID"] = new SelectList(department, "Department_ID", "DepartmentName", course.Department_ID);
                return View(course);
        }
        public async Task<IActionResult>Delete(int? id) 
        {
            if (id == null)
                return NotFound();
            var course = await _context.Courses.Include(d =>d.Department).ThenInclude(f => f.Faculty).FirstOrDefaultAsync(i => i.Course_ID == id);
            if(course == null)
                { return NotFound(); }
            return View(course);
        }
        [HttpPost , ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) 
        {
            var course = await _context.Courses.FindAsync(id);
            if( course == null ) { return NotFound(); }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "The course was deleted.";
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Recommendations(string type, string sort)
        {
            var courses = _context.Courses
                .Include(c => c.Department)
                .ThenInclude(d => d.Faculty)
                .ThenInclude(f => f.University)
                .AsQueryable();

            // ===== Filter =====
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "gov")
                {
                    courses = courses.Where(c => c.Department.Faculty.University.Type == UniversityType.Government);
                }
                else if (type == "private")
                {
                    courses = courses.Where(c => c.Department.Faculty.University.Type == UniversityType.Private);
                }
            }

            // ===== Sorting =====
            switch (sort)
            {
                case "name":
                    courses = courses.OrderBy(c => c.Course_Name);
                    break;

                case "gpa":
                    courses = courses.OrderByDescending(c => c.Course_ID); // placeholder
                    break;

                default:
                    courses = courses.OrderBy(c => c.Course_Name);
                    break;
            }

            return View(await courses.ToListAsync());
        }

    }
}
