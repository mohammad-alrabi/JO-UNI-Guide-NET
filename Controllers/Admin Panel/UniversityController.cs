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
    [Authorize(Roles = "SuperAdmin, Admin")] //عشان ما حدا من المستخدمين او الطلاب يقدر يفوت على صفحة الادمن 
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
        // ورح نضيف searchString للبحث عن جامعة مثلا 
        public async Task<IActionResult> Index(string searchString , string currentFilter , int? pageNumber) 
        {
            //اذا الادمن عمل عملية بحث جديدة رجعه للصفحة الاولى
            if (searchString != null) 
            {
                pageNumber = 1;
            }
            else 
            {
                searchString = currentFilter;
            }
            //رتب النتائج ونفذ الكويري عشان تنبعث للشاشة 
            ViewData["CurrentFilter"] = searchString;
            // عشان نقدر نعدل عليه قبل ما يروح على الداتابيز بنعرف الكويري ك IQueryable
            var universities = _context.Universities
                 .AsNoTracking()
                 .AsQueryable();
            // عشان يتأكد هل الادمن كتب اشي بمربع البحث ؟
            if (!string.IsNullOrEmpty(searchString)) 
            {
                //اذا كتب , بفلتر النتائج يعني بنبحث بالاسم او الموقع مثلا
                universities = universities.Where(u => u.Name.Contains(searchString) || u.Location.Contains(searchString));
            }
            //ترتيب الداتا ابجديا في كل صفحة مثلا في 5 جامعات
            universities = universities.OrderBy(n => n.Name);
            int pageSize = 3;
            return View (await JO_UNI_Guide.Helpers.PaginatedList<University>.CreateAsync(
                universities,
                pageNumber ?? 1 ,
                pageSize));
        }
        public IActionResult Create()
        {
            return View();
        }
        // to Create new Uni 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("University_ID,Name,Location,Logo,Rank_QS")] University university, IFormFile? logoFile) //OverPosting Protection
        {
            ModelState.Remove("Logo");

            // في حال كانت الداتا الي دخلها الادمن صح
            if (ModelState.IsValid) 
            {
                try
                {
                    //عشان يتحقق من نوع الملف 
                    if (logoFile != null && !logoFile.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "Only image files are allowed.");
                        return View(university);
                    }
                    //للتحقق من حجم الملف
                    if (logoFile != null && logoFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "File size must be less than 2MB");
                        return View(university);
                    }
                    //الان بنقدر نرفع الصورة
                    if(logoFile != null && logoFile.Length > 0)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string imagesFolder = Path.Combine(wwwRootPath, "images");

                        //اذا الملف مش موجود اعمله
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }
                        //انشاء اسم يونيك عشان ما يصير كونفليكت
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                        string fullPath = Path.Combine(imagesFolder, fileName);
                        //لحفظ الصورة فعليا جوا السيرفر
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }
                        university.Logo = "/images/" + fileName;
                    }
                    else
                    {
                        //اذا ما في صورة خليه يعمل fallback
                        university.Logo = "/images/default.png";
                    }
                    //حفظ البيانات
                    _context.Universities.Add(university);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "The university has been successfully added.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception) 
                {
                    ModelState.AddModelError("", "This Unvirsity already exists");
                }
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
            var university = await _context.Universities
                .Include(u => u.Faculties)
                .FirstOrDefaultAsync(m =>m.University_ID == id);

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
        public async Task <IActionResult>Edit(int id , [Bind("University_ID,Name,Location,Logo,Rank_QS")] University university, IFormFile? logoFile) 
        {
            if (id != university.University_ID)
                return NotFound();
            if (ModelState.IsValid) 
            {
                try
                {
                    //جيب الداتا القديمة من الداتابيز
                    var existingUniversity = await _context.Universities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.University_ID == id);

                    if(existingUniversity == null) 
                        return NotFound();
                    // validation للصورة 
                    if(logoFile != null && !logoFile.ContentType.StartsWith("image/")) 
                    {
                        ModelState.AddModelError("", "Only image Files are allowed . ");
                        return View(university);
                    }
                    if (logoFile != null && logoFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "File Size must be less than 2MB");
                        return View(university);
                    }
                    // 1. إذا الأدمن اختار صورة جديدة
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string imagesFolder = Path.Combine(wwwRootPath, "images");

                        //تأكد انه الفولدر موجود 
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        // حذف الصورة القديمة if not default
                        if (!string.IsNullOrEmpty(existingUniversity.Logo) && existingUniversity.Logo != "/images/default.png")
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, existingUniversity.Logo.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // حفظ الصورة الجديدة
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                        string fullPath = Path.Combine(imagesFolder, fileName);

                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        // تحديث المسار في الداتا بيز
                        university.Logo = "/images/" + fileName;
                    }
                    else
                    {
                        //اذا ما في صورة خلي الصورة القديمة 
                        university.Logo = existingUniversity.Logo;
                    }
                    // هون بعمل تحديث للداتا الي صار عليها تعديل 
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
            if (university == null)
                return NotFound();
            //لحذف الصورة من السيرفير مش بس الداتابيز
            if (!string.IsNullOrEmpty(university.Logo)&& university.Logo != "/images/default.png") 
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath , university.Logo.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) 
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            //حذف الصورة من الداتابيز
                _context.Universities.Remove(university);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The university was deleted";
            return RedirectToAction(nameof(Index));
        }
    }
}
