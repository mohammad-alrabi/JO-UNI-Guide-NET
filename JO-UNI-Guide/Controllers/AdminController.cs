using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
      private readonly ApplicationDbContext _context;
        public AdminController (ApplicationDbContext context) 
        {
            _context = context;
        }
        //هاي الصفحة بنعرض فيها الاحصائيات
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel
            {
                UniversitiesCount = await _context.Universities.CountAsync(),
                FacultiesCount = await _context.Faculties.CountAsync(),
                DepartmentsCount = await _context.Departments.CountAsync(),
                CoursesCount = await _context.Courses.CountAsync(),
            };
            return View(model);
        }
    }
}
