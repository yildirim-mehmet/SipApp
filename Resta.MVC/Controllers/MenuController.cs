using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Models;
using Resta.MVC.Services;

namespace Resta.MVC.Controllers;

/// <summary>
/// Müşteri menüsü (QR sonrası)
/// - Sayfa render'ı server-side (Razor)
/// - JS sadece sepet (localStorage) ve SiparisVer aksiyonunu tetikler
/// - Böylece browser'da API portuna direkt fetch yapıp CORS'e takılmayız
/// </summary>
public class MenuController : Controller
{
    private readonly ApiClient _api;

    public MenuController(ApiClient api)
    {
        _api = api;
    }

    // /Menu?masaId=2
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Index(int masaId, CancellationToken ct)
    {
        // 1) Menü: API array dönüyor -> List<MenuKategoriVm> çekiyoruz
        var kategoriler = await _api.GetAsync<List<MenuKategoriVm>>($"Menu/masa/{masaId}", ct)
                        ?? new List<MenuKategoriVm>();

        // 2) Masa durum (bos/dolu)
        var durum = await _api.GetAsync<MasaDurumVm>($"Masa/{masaId}/durum", ct)
                   ?? new MasaDurumVm { Durum = "bos" };

        // 3) ViewModel'i burada biz sarıyoruz
        var vm = new MenuResponseVm
        {
            MasaId = masaId,
            BolumId = 0,              // API dönmüyor, şimdilik 0 bırak
            Kategoriler = kategoriler
        };

        ViewBag.MasaDurum = (durum.Durum ?? "bos").ToLowerInvariant();
        return View(vm);
    }

    //aşağıda ilk versiyon farklı api okuma
    //public async Task<IActionResult> Index(int masaId, CancellationToken ct)
    //{
    //    // 1) Menü çek
    //    var menu = await _api.GetAsync<MenuResponseVm>($"Menu/masa/{masaId}", ct)
    //               ?? new MenuResponseVm { MasaId = masaId, BolumId = 0, Kategoriler = new() };

    //    // 2) Masa durum çek (bos/dolu)
    //    var durum = await _api.GetAsync<MasaDurumVm>($"Masa/{masaId}/durum", ct)
    //               ?? new MasaDurumVm { Durum = "bos" };

    //    ViewBag.MasaDurum = (durum.Durum ?? "bos").ToLowerInvariant();
    //    return View(menu);
    //}

    // JS burayı çağırır: Sepettekileri siparişe çevirir.
    // MVC -> API proxy (same-origin)
    [HttpPost]
    public async Task<IActionResult> SiparisVer([FromBody] SiparisVerRequestVm req, CancellationToken ct)
    {
        if (req.MasaId <= 0)
            return BadRequest(new { message = "masaId hatalı" });

        if (req.Urunler == null || req.Urunler.Count == 0)
            return BadRequest(new { message = "urun yok" });

        // API /api/Siparis/ver (BaseAddress zaten .../api/)
        var res = await _api.PostAsync<SiparisVerResponseVm>("Siparis/ver", new
        {
            masaId = req.MasaId,
            urunler = req.Urunler.Select(x => new { urunId = x.UrunId, adet = x.Adet }).ToArray()
        }, ct);

        if (res == null)
            return StatusCode(500, new { message = "Sipariş API başarısız." });

        return Ok(res);
    }

    // JS confirm senaryosu için: aktif adisyon var mı?
    // API /api/Masa/{masaId}/aktif-adisyon -> null veya adisyon
    [HttpGet]
    public async Task<IActionResult> AktifAdisyon(int masaId, CancellationToken ct)
    {
        var adisyon = await _api.GetAsync<AdisyonMiniDto>($"Masa/{masaId}/aktif-adisyon", ct);
        return Ok(adisyon); // null gelebilir
    }
}
