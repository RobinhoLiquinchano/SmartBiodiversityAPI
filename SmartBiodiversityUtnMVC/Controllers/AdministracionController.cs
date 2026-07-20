using Microsoft.AspNetCore.Mvc;

namespace SmartBiodiversityUtnMVC.Controllers
{
    public class AdministracionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}