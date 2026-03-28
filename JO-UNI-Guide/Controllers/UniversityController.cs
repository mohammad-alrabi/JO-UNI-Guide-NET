using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Http;
using System.IO;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles ="Admin")] //عشان ما حدا من المستخدمين او الطلاب يقدر يفوت على صفحة الادمن 
    public class UniversityController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        // Dependency injection => بربط الداتابيز بالكونترولر
        public UniversityController (ApplicationDbContext context , IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // هاي الميثود وضيفتها تبحث بالداتابيز عشان تتأكد اذا الجامعة موجودة او لا 
        private async Task<bool> UniversityExists(int id)
        {
            return await _context.Universities.AnyAsync(e => e.University_ID == id);
        }
        // هون رح نعرض كل الجامعات 
        public async Task<IActionResult> Index() 
        {
            //هون بدنا نجيب كل الجامعات من الداتا بيز 
            var universities = await _context.Universities
                .AsNoTracking()
                .OrderBy(u=>u.Name)
                .ToListAsync();
            
            return View(universities);
        }
        public IActionResult Create()
        {
            return View();
        }
        // to Create new Uni 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("University_ID,Name,Location,Logo,Description,Website")] University university, IFormFile? logoFile) //OverPosting Protection
        {
            // في حال كانت الداتا الي دخلها الادمن صح
            if (ModelState.IsValid) 
            {
                //لمعالجة ورفع الصورة اللوجو
                if(logoFile != null && logoFile.Length > 0) 
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string imagesFolder = Path.Combine(wwwRootPath, "imges");

                    //اذا الملف مش موجود اعمله
                    if (!Directory.Exists(imagesFolder)) 
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }
                    //انشاء اسم يونيك عشان ما يصير كونفليكت
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                    string fullPath = Path.Combine(imagesFolder, fileName);
                    //لحفظ الصورة فعليا جوا السيرفر
                    using (var fileStrem = new FlieStrem(fullPath , FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStrem);
                    }
                    university.Logo = "/images/" + fileName;
                }
                _context.Universities.Add(university);
                await _context.SaveChangesAsync();

                TempData["Success"] = "The university has been successfully added.";

                return RedirectToAction(nameof(Index));
            }
            // في حال ما تحقق الشرط وكانت البيانات الي دخلها الادمن خطأ او ناقصة 
            return View(university);

        }
        // لعرض التفاصيل details جامعة وحدة 
        public async Task<IActionResult> Details (int? id) 
        {
            if(id == null) 
                return NotFound();
            //البحث عن الجامعة من خلال ال(id)
            var university = await _context.Universities.FirstOrDefaultAsync(m =>m.University_ID == id);

            if (university == null) // في حال دورنا على الجامعة ما لقيناها 
                return NotFound();
            return View(university); 
        }
        //لفتح شاشة التعديل على حسب الid 
        public async Task<IActionResult> Edit (int? id) 
        {
            if (id == null)
                return NotFound();

            var university = await _context.Universities.FindAsync(id);
            if (university ==null)
                return NotFound();
            return View(university);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //بعد استلام الداتا من التعديل وحفظها بالداتابيز
        public async Task <IActionResult>Edit(int id , [Bind("University_ID,Name,Logo,Location,Rank_QS")] University university) 
        {
            if (id != university.University_ID)
                return NotFound();
            if (ModelState.IsValid) 
            {
                try
                {
                    _context.Update(university);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "The university has been successfully modified";
                }
                catch (DbUpdateConcurrencyException) // if conflict happen
                {
                    if (!await UniversityExists(university.University_ID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index)); // الصفحة الي بتحتوي على الجامعات
            }
            return View(university); // لو في اخطأ ارجع لنفس الفورم 
        }
        //شاشة لتعرض تأكيد الحذف 
        public async Task<IActionResult>Delete(int? id) 
        {
            if (id == null) return NotFound();
            var university = await _context.Universities.FirstOrDefaultAsync(m =>m.University_ID == id);

            if(university == null) return NotFound();
            return View(university);
        }
        //شاشة لتنفيذ عملية الحذف بعد التأكيد
        [HttpPost , ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var university = await _context.Universities.FindAsync(id);
            if (university != null)
            {

                _context.Universities.Remove(university);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The university was deleted";
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
