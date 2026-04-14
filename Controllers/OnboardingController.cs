using Microsoft.AspNetCore.Mvc;

namespace JO_UNI_Guide.Controllers
{
    public class OnboardingController : Controller
    {
        public IActionResult Index()
        {
            // GET

            // منع إعادة الإدخال
            if (HttpContext.Session.GetString("Done") == "true")
            {
                
                return RedirectToAction("Index", "StudentDashboard");
            }

            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Index(int GPA, string Track, string UniversityType)
        {
            // 🔴 validation أول شي
            if (GPA < 0 || GPA > 100)
            {
                ModelState.AddModelError("", "GPA must be between 0 and 100");
                return View();
            }

            // 🟢 بعدين التخزين
            HttpContext.Session.SetString("Done", "true");
            HttpContext.Session.SetInt32("GPA", GPA);

            TempData["Success"] = "Onboarding completed successfully 🎉";

            return RedirectToAction("Index", "StudentDashboard");
        }
    }
}
        
    
