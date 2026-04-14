using Microsoft.AspNetCore.Mvc;

namespace JO_UNI_Guide.Controllers
{
    public class StudentDashboardController : Controller
    {
        public IActionResult Index()
        {
          
        {
            int gpa = HttpContext.Session.GetInt32("GPA") ?? 0;

            ViewBag.GPA = gpa;

            // majors fake (مؤقت)
            var majors = new List<string>();

            if (gpa >= 85)
            {
                majors = new List<string> { "Medicine", "Engineering", "Computer Science", "Pharmacy", "AI" };
            }
            else if (gpa >= 70)
            {
                majors = new List<string> { "Business", "Accounting", "IT", "Design", "Marketing" };
            }
                ViewBag.Majors = majors;
                ViewBag.MajorsCount = majors.Count;

            return View();
        }
         
        }
    }
}
