using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Models;
using Resta.MVC.Services;

namespace Resta.MVC.Controllers
{
    public class MasaController : Controller
    {
        private readonly ApiClient _api;

        public MasaController(ApiClient api)
        {
            _api = api;
        }

        // QR okutulduğunda buraya düşer
        // /Masa/2
        [HttpGet("Masa/{id:int}")]
        public async Task<IActionResult> Index(int id)
        {
            // API'den masa bilgisi çekilir
            var masa = await _api.GetAsync<MasaDto>($"Masa/{id}");

            if (masa == null || !masa.Aktif)
                return NotFound("Masa bulunamadı veya pasif.");

            // Menü ekranına yönlendiriyoruz
            return RedirectToAction("Index", "Menu", new { masaId = masa.Id });
        }
    }
}
