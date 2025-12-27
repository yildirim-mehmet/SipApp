using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Services;

namespace Resta.MVC.Controllers;

public class EkranController : Controller
{
    private readonly ApiClient _api;
    public EkranController(ApiClient api) { _api = api; }

    // /Ekran -> ekran seçimi
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var ekranlar = await _api.GetAsync<List<dynamic>>("Ekran", ct) ?? new();
        return View(ekranlar);
    }

    // /Ekran/Pano?ekranId=1
    [HttpGet]
    public IActionResult Pano(int ekranId)
    {
        ViewBag.EkranId = ekranId;
        return View();
    }

    // ✅ UI sol/sağ panellerin datası (MVC -> API proxy)
    [HttpGet]
    public async Task<IActionResult> Siparisler(int ekranId, CancellationToken ct)
    {
        var data = await _api.GetAsync<object>($"Ekran/{ekranId}/siparisler", ct);
        return Ok(data);
    }

    // ✅ tek masa adisyon detayı (API tarafında bunu birazdan ekleyeceğiz)
    [HttpGet]
    public async Task<IActionResult> MasaAdisyon(int masaId, CancellationToken ct)
    {
        var data = await _api.GetAsync<object>($"Adisyon/aktif/masa/{masaId}", ct);
        return Ok(data);
    }

    // ✅ durum güncelle
    [HttpPut]
    //[HttpPut]
    public async Task<IActionResult> KalemDurum(int kalemId, [FromBody] int yeniDurum, CancellationToken ct)
    {
        var res = await _api.PutAsync<object>($"Ekran/siparis/{kalemId}/durum", yeniDurum, ct);
        return Ok(res);
    }


    // ✅ adisyon kapat/iptal (API tarafında ekleyeceğiz) burada sorun çıkabilir
    [HttpPost]
    public async Task<IActionResult> AdisyonKapat(int adisyonId, CancellationToken ct)
    {
        var res = await _api.PostAsync<object>($"Adisyon/{adisyonId}/kapat", new { }, ct);
        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> AdisyonIptal(int adisyonId, CancellationToken ct)
    {
        var res = await _api.PostAsync<object>($"Adisyon/{adisyonId}/iptal", new { }, ct);
        return Ok(res);
    }

    // ✅ A5 print
    [HttpGet]
    public async Task<IActionResult> Yazdir(int adisyonId, CancellationToken ct)
    {
        var data = await _api.GetAsync<dynamic>($"Adisyon/{adisyonId}/detay", ct);
        return View("Yazdir", data);
    }



    // SON DURUM EKLEMELERİ
    // /Ekran?ekranId=1
    public async Task<IActionResult> Index(int ekranId)
    {
        ViewBag.EkranId = ekranId;

        var siparisler = await _api.GetAsync<List<dynamic>>(
            $"Ekran/{ekranId}/siparisler");

        var masalar = await _api.GetAsync<List<dynamic>>("Masa");

        return View(new
        {
            Siparisler = siparisler,
            Masalar = masalar
        });
    }

    // Adisyon detay (panel için)
    public async Task<IActionResult> AdisyonPanel(int masaId)
    {
        var adisyon = await _api.GetAsync<dynamic>(
            $"Adisyon/aktif/masa/{masaId}");

        return PartialView("_AdisyonPanel", adisyon);
    }

    public async Task<IActionResult> Yazdir(int adisyonId)
    {
        var data = await _api.GetAsync<dynamic>(
            $"Adisyon/detay/{adisyonId}");

        return View(data);
    }


}
