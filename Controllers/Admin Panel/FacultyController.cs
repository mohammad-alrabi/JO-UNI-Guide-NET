using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyController (ApplicationDbContext context) 
        {
            _context = context;
        }
        // التأكد من وجود الكلية من خلال ال ID
        private async Task<bool> FacultyExists(int id)
        {
            return await _context.Faculties.AnyAsync(e => e.Faculty_ID == id);
        }
        // عرض كل الكليات مع اسم الجامعة التابعه لها 
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
            ViewData["CurrentFilghter"] = searchString;
            //عشان نجيب بيانات الجامعة المربوطة بالكلية رح نستخدم Include
            // Include هي المسؤلة عن العلاقات مع الجدول الاساسي 
            // معها رح يجيبلنا الكلية والجامعة التابعة لها 
            var faculties = _context.Faculties
                .Include(f => f.University)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                //البحث باسم الكلية او اسم الجامعة التابعه لها
                faculties = faculties.Where(f=>f.Name.Contains(searchString) ||
                                             f.University.Name.Contains(searchString));
            }
            faculties = faculties.OrderBy(n => n.Name);
            int pageSize = 5;
            
            return View(await JO_UNI_Guide.Helpers.PaginatedList<Faculty>.CreateAsync(
                faculties , pageNumber ?? 1 , pageSize));
        }

        public async Task<IActionResult>Create() 
        {
            var universities = await _context.Universities   //جلب اسماء الجامعة للقائمة 
                .AsNoTracking()
                .OrderBy(u=>u.Name)
                .ToListAsync();
            ViewData["University_ID"] = new SelectList(universities , "University_ID" ,"Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Faculty_ID,Name,Details,Faculty_Dean,Location,University_ID")] Faculty faculty) 
        {
            if (ModelState.IsValid) 
            {
                _context.Add(faculty);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The faculty has been successfully added.";
                return RedirectToAction(nameof(Index));
            }
            //في حال وجود خطأ بالادخال 
            var universities = await _context.Universities
                .AsNoTracking()
                .OrderBy(u=>u.Name)
                .ToListAsync();
            ViewData["University_ID"] = new SelectList(universities, "University_ID", "Name", faculty.University_ID);
            return View(faculty);
        }
        //لعرض تفاصيل الكلية
        public async Task<IActionResult>Details(int? id) 
        {
            if (id == null)
                return NotFound();
            var faculty = await _context.Faculties
                .Include(f=>f.University)
                .FirstOrDefaultAsync(m => m.Faculty_ID == id);

            if (faculty == null)
                return NotFound();
            return View(faculty);
        }
        public async Task<IActionResult>Edit(int? id) 
        {
            if(id == null)
                return NotFound();

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
                return NotFound();

            //تجهيز قائمة الجامعات ونحدد الجامعة الحالية كخيار افتراضي
            var universities = await _context.Universities
                .AsNoTracking()
                .OrderBy(u=>u.Name)
                .ToListAsync();
            ViewData["University_ID"] = new SelectList(universities, "University_ID", "Name", faculty.University_ID);
            return View(faculty);
        }
        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<IActionResult>Edit(int id, [Bind("Faculty_ID,Name,Details,Faculty_Dean,Location,University_ID")] Faculty faculty)
        {
            if (id != faculty.Faculty_ID)
                return NotFound();
            if (ModelState.IsValid)
            {
                try 
                {
                    _context.Update(faculty);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "The faculty has been successfully modified.";
                }
                catch (DbUpdateConcurrencyException) 
                {
                    if (! await FacultyExists(faculty.Faculty_ID))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            //لو في خطأ بترجع تبعث القائمة 
            var universities = await _context.Universities
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .ToListAsync();
            ViewData["University_ID"] = new SelectList(universities , "University_ID","Name" , faculty.University_ID);
            return View(faculty);
        }
        public async Task<IActionResult> Delete (int? id) 
        {
            if(id == null)
                return NotFound();
            var faculty = await _context.Faculties
                .Include(f => f.University)
                .FirstOrDefaultAsync(m => m.Faculty_ID == id);

            if(faculty == null)
                return NotFound();
            return View(faculty);
        }
        [HttpPost , ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) 
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if(faculty != null) 
            {
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The faculty was deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
        
    }
}
