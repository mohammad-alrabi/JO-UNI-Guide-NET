using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController (ApplicationDbContext context , UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }
        //هاي الصفحة بنعرض فيها الاحصائيات
        [Route("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel
            {
                UniversitiesCount = await _context.Universities.CountAsync(),
                PublicUniversitiesCount = await _context.Universities
                    .CountAsync(u => u.Type == UniversityType.Governmental),
                PrivateUniversitiesCount = await _context.Universities
            .       CountAsync(u => u.Type == UniversityType.Private),
                FacultiesCount = await _context.Faculties.CountAsync(),
                DepartmentsCount = await _context.Departments.CountAsync(),
                CoursesCount = await _context.Courses.CountAsync(),

                //عدد الادمنز
                AdminsCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count,

                LatestUniversities = await _context.Universities
                                    .OrderByDescending(u => u.University_ID)
                                    .Take(5)
                                    .ToListAsync(),
                LatestCourses = await _context.Courses
                                  .OrderByDescending(c => c.Course_ID)
                                  .Take(5)
                                  .ToListAsync(),

                LatestMessages = await _context.ContactMessages
                                .OrderByDescending(m => m.SentDate)
                                .Take(5)
                                .ToListAsync(),

            };

            ViewBag.MessagesCount = await _context.ContactMessages.CountAsync();
            return View("~/Views/Dashboard/Dashboard.cshtml", model);
        }
    }
}
