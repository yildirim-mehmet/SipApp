using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Models;

namespace Resta.MVC.Controllers
{
    public class Home_Controller : Controller
    {
        private readonly ILogger<Home_Controller> _logger;

        public Home_Controller(ILogger<Home_Controller> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
