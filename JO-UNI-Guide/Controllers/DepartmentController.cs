using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
     private readonly ApplicationDbContext _context;
        public DepartmentController (ApplicationDbContext context) 
        {
            _context = context;
        }
        //عشان يتأكد اذا الid المدخلة هي لنفس القسم
        private async Task<bool> DepartmentExists(int id) 
        {
            return await _context.Departments.AnyAsync(e => e.Department_ID == id);
        }
        // لعرض كل الاقسام 
        public async Task<IActionResult> Index() 
        {
            //include => بتجيب الكلية (faculty>
            //ThenInclude => بتكمل وبتجيب الجامعة التابعة الها الكلية
            var departments = _context.Departments
                .Include(e => e.Faculty)
                .ThenInclude(f => f.University)
                .AsNoTracking();
            return View(await departments.ToListAsync());
        }
        //لاضافة قسم جديد 
        public async Task<IActionResult> Create() 
        {
            //بتجيب اسماء الكليات الموجودة 
            var faculties = await _context.Faculties
                .AsNoTracking()
                .OrderBy(f => f.Name)
                .ToListAsync();
            ViewData["Faculty_ID"] = new SelectList(faculties, "Faculty_ID", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Department_ID,DepartmentName,Details,Faculty_ID")] Department department) 
        {
            if (ModelState.IsValid) 
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The department has been successfully added.";
                return RedirectToAction(nameof(Index));
            }
            //في حال وجود خطأ بعملية الانشاء خليه يرجعلي قائمة الكليات
            var faculties = await _context.Faculties
                .AsNoTracking()
                .OrderBy(f => f.Name)
                .ToListAsync();
            ViewData["Faculty_ID"] = new SelectList(faculties, "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }
        //لعرض تفاصيل القسم 
        public async Task<IActionResult>Details(int? id) 
        {
            if (id == null)
                return NotFound();
            var department = await _context.Departments
                .Include(d =>d.Faculty)
                .ThenInclude(d => d.University)
                .FirstOrDefaultAsync(m =>m.Department_ID == id);
            if(department == null)
                return NotFound();
            return View(department);
        }
        //لتعديل قسم معين 
        public async Task<IActionResult>Edit(int? id)
        {
            if(id == null)
                return NotFound();
            var department = await _context.Departments.FindAsync(id);
            if(department == null)
                return NotFound();
            var faculties = await _context.Faculties
                .AsNoTracking()
                .OrderBy(f =>f.Name)
                .ToListAsync();
            ViewData["Faculty_ID"] = new SelectList(faculties, "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Department_ID,DepartmentName,Details,Faculty_ID")] Department department)
        {
            if (id != department.Department_ID)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "The department has been successfully modified.";
                }
                catch (DbUpdateConcurrencyException) 
                {
                    if (! await DepartmentExists(department.Department_ID))
                        return NotFound();
                    else 
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var faculties = await _context.Faculties.AsNoTracking() .OrderBy(f =>f.Name) .ToListAsync();
            ViewData["Faculty_ID"] = new SelectList(faculties, "Faculty_ID", "Name", department.Faculty_ID);
            return View(department);
        }
        //لحذف قسم 
        public async Task<IActionResult>Delete(int? id) 
        {
            if (id == null)
                return NotFound();
            var department = await _context.Departments
                .Include(d =>d.Faculty)
                .FirstOrDefaultAsync(f =>f.Department_ID == id);
            if (department == null)
                return NotFound();
            return View(department);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteConfirmed(int id) 
        {
            var department = await _context.Departments.FindAsync(id);
            if(department != null) 
            {
                _context.Departments.Remove(department); 
                    await _context.SaveChangesAsync();
                TempData["Success"] = "The department was deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
