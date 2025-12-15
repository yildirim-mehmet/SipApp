using Microsoft.AspNetCore.Mvc;

namespace Resta.MVC.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index(int masaId)
        {
            // Sadece masaId View'a gönderilir
            ViewBag.MasaId = masaId;
            return View();
        }
    }
}
