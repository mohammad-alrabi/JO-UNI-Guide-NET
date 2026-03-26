using Microsoft.AspNetCore.Mvc;

namespace JO_UNI_Guide.Controllers
{
    public class UniversityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
