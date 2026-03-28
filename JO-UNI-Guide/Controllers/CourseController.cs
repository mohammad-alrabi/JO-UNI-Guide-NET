using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace JO_UNI_Guide.Controllers
{
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
        public async Task<IActionResult> Index() 
        {
            // course => Department => Faculty => university
            var courses = _context.Courses
                .Include(e => e.Department)
                    .ThenInclude(e => e.Faculty)
                        .ThenInclude(e => e.University)
                .AsNoTracking();
            return View(await courses.ToListAsync());
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
            var department = await _context.Departments.AsNoTracking().OrderBy (n =>n.DepartmentName).ToArrayAsync();
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
                var departemnt = await _context.Departments.AsNoTracking().OrderBy(n=>n.DepartmentName).ToListAsync();
                ViewData["Department_ID"] = new SelectList(departemnt, "Department_ID", "DepartmentName", course.Department_ID);
                return View(course);
        }
        public async Task<IActionResult>Delete(int? id) 
        {
            if (id == null)
                return NotFound();
            var course = await _context.Courses.Include(d =>d.Department).FirstOrDefaultAsync(i => i.Course_ID == id);
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
    }
}
