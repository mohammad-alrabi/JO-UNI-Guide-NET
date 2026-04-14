using JO_UNI_Guide.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Route("University")] //for public user
    public class UniExplorerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UniExplorerController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("Index")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var universities = await _context.Universities
                .OrderBy(n => n.Rank_QS)
                .ToListAsync();
            return View("~/Views/StUniversity/Index.cshtml", universities);
        }
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var university = await _context.Universities
                .Include(u =>u.Faculties)
                .FirstOrDefaultAsync(u =>u.University_ID == id);
            if(university == null) { return NotFound(); }
            return View("~/Views/StUniversity/Details.cshtml", university);

        }

        
    }
}
