using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Models;
using Resta.MVC.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Resta.MVC.Controllers
{
    public class MuzikController : Controller
    {
        private readonly ApiClient _api;
        private readonly ILogger<MuzikController> _logger;

        public MuzikController(ApiClient api, ILogger<MuzikController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: /Muzik/{masaHash}
        // QR kod: https://localhost:7231/Muzik/eqr52d23qsd661f5erry78op
        [HttpGet("Muzik/{masaHash}")]
        public async Task<IActionResult> Index(string masaHash)
        {
            
            try
            {
                // 1. Masa bilgilerini mevcut Menu API'sinden al
                var masa = await _api.GetAsync<MasaDto>($"Menu/masa/{masaHash}");
                if (masa == null || !masa.Aktif)
                {
                    //return View("Error", new ErrorViewModel
                    //{
                    //    Message = "Masa bulunamadı veya pasif durumda."
                    //});
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }

                // 2. Şarkı listesini Müzik API'sinden al
                var sarkilar = await _api.GetAsync<SarkiDto[]>("Muzik/sarkilar");

                // 3. Mevcut sırayı Müzik API'sinden al
                var mevcutSira = await _api.GetAsync<CalmaListesiDto[]>("Muzik/kuyruk");

                // DTO -> ViewModel mapping
                var sarkiViewModels = (sarkilar ?? Array.Empty<SarkiDto>())
                    .Select(s => new SarkiViewModel
                    {
                        Id = s.id,
                        Ad = s.ad,
                        DosyaYolu = s.dosyaYolu,
                        Sure = s.sure,
                        Aktif = s.aktif,
                        EklenmeTarihi = s.eklenmeTarihi
                    })
                    .ToList();

                var siraViewModels = (mevcutSira ?? Array.Empty<CalmaListesiDto>())
                    .Select(c => new CalmaListesiViewModel
                    {
                        Id = c.id,
                        SarkiId = c.sarkiId,
                        SarkiAdi = c.sarkiAdi,
                        DosyaYolu = c.dosyaYolu,
                        Sure = c.sure,
                        MasaId = c.masaId,
                        MasaAdi = c.masaAdi,
                        SiraDegeri = c.siraDegeri,
                        Calindi = c.calindi,
                        OdemeMiktari = c.odemeMiktari,
                        EklemeZamani = c.eklenmeZamani
                    })
                    .ToList();

                ViewBag.Masa = new MasaDto
                {
                    id = masa.id,
                    Ad = masa.Ad,
                    HashId = masa.HashId,
                    Aktif = masa.Aktif
                };

                ViewBag.MevcutSira = siraViewModels;
                ViewBag.Title = $"Müzik Siparişi - {masa.Ad}";

                return View(sarkiViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Muzik/Index hatası - MasaHash: {MasaHash}", masaHash);
                //return View("Error", new ErrorViewModel
                //{
                //    Message = "Sistem hatası oluştu. Lütfen tekrar deneyin."
                //});
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Muzik/Panel (Yönetim Paneli)
        [HttpGet("Muzik/Panel")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Panel()
        {
            try
            {
                // İstatistikleri API'den al
                var istatistikler = await _api.GetAsync<MuzikIstatistikDto>("Muzik/istatistikler");

                // Şarkı listesini API'den al
                var sarkilar = await _api.GetAsync<SarkiDto[]>("Muzik/sarkilar");

                // Tüm sırayı API'den al
                var tumSira = await _api.GetAsync<CalmaListesiDto[]>("Muzik/tum-sira");

                // DTO -> ViewModel mapping
                var istatistikViewModel = istatistikler != null ? new MuzikIstatistikViewModel
                {
                    ToplamSarki = istatistikler.toplamSarki,
                    AktifCalmaListesi = istatistikler.aktifCalmaListesi,
                    TamamlananCalma = istatistikler.tamamlananCalma,
                    BugunkuIstekler = istatistikler.bugunkuIstekler,
                    ToplamOdeme = istatistikler.toplamOdeme,
                    EnCokEkleyenMasa = istatistikler.enCokEkleyenMasa != null ?
                        new EnCokEkleyenMasaViewModel
                        {
                            MasaId = istatistikler.enCokEkleyenMasa.masaId,
                            MasaAdi = istatistikler.enCokEkleyenMasa.masaAdi,
                            Sayi = istatistikler.enCokEkleyenMasa.sayi
                        } : null
                } : null;

                var sarkiViewModels = (sarkilar ?? Array.Empty<SarkiDto>())
                    .Select(s => new SarkiViewModel
                    {
                        Id = s.id,
                        Ad = s.ad,
                        DosyaYolu = s.dosyaYolu,
                        Sure = s.sure,
                        Aktif = s.aktif,
                        EklenmeTarihi = s.eklenmeTarihi
                    })
                    .ToList();

                var siraViewModels = (tumSira ?? Array.Empty<CalmaListesiDto>())
                    .Select(c => new CalmaListesiViewModel
                    {
                        Id = c.id,
                        SarkiId = c.sarkiId,
                        SarkiAdi = c.sarkiAdi,
                        DosyaYolu = c.dosyaYolu,
                        Sure = c.sure,
                        MasaId = c.masaId,
                        MasaAdi = c.masaAdi,
                        SiraDegeri = c.siraDegeri,
                        Calindi = c.calindi,
                        OdemeMiktari = c.odemeMiktari,
                        EklemeZamani = c.eklenmeZamani
                    })
                    .ToList();

                ViewBag.Istatistikler = istatistikViewModel;
                ViewBag.Sarkilar = sarkiViewModels;
                ViewBag.Sira = siraViewModels;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Muzik/Panel hatası");
                //return View("Error", new ErrorViewModel
                //{
                //    Message = "Yönetim paneline erişilirken hata oluştu."
                //});
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Muzik/Ekle (AJAX için)
        [HttpPost("Muzik/Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle([FromBody] SarkiEkleViewModel request)
        {
            try
            {
                if (request == null || request.SarkiId <= 0 || request.MasaId <= 0)
                    return BadRequest(new { success = false, message = "Geçersiz veri" });

                // API'ye istek gönder
                var response = await _api.PostAsync<object>("Muzik/ekle", new
                {
                    sarkiId = request.SarkiId,
                    masaId = request.MasaId
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şarkı ekleme hatası");
                return StatusCode(500, new { success = false, message = "Sunucu hatası" });
            }
        }

        // POST: /Muzik/OdemeliEkle (AJAX için)
        [HttpPost("Muzik/OdemeliEkle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdemeliEkle([FromBody] OdemeliSarkiEkleViewModel request)
        {
            try
            {
                if (request.OdemeMiktari < 2)
                    return BadRequest(new { success = false, message = "Ödeme en az 2 TL olmalıdır" });

                // API'ye istek gönder
                var response = await _api.PostAsync<object>("Muzik/odemeli-ekle", new
                {
                    sarkiId = request.SarkiId,
                    masaId = request.MasaId,
                    odemeMiktari = request.OdemeMiktari,
                    odemeReferans = request.OdemeReferans
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödemeli şarkı ekleme hatası");
                return StatusCode(500, new { success = false, message = "Sunucu hatası" });
            }
        }
    }

    // MVC için API DTO'ları (API'dan gelen verileri karşılamak için)
    public class SarkiDto
    {
        public int id { get; set; }
        public string ad { get; set; } = null!;
        public string dosyaYolu { get; set; } = null!;
        public int sure { get; set; }
        public bool aktif { get; set; }
        public DateTime eklenmeTarihi { get; set; }
    }

    public class CalmaListesiDto
    {
        public int id { get; set; }
        public int sarkiId { get; set; }
        public string sarkiAdi { get; set; } = null!;
        public string dosyaYolu { get; set; } = null!;
        public int sure { get; set; }
        public int masaId { get; set; }
        public string masaAdi { get; set; } = null!;
        public int siraDegeri { get; set; }
        public bool calindi { get; set; }
        public decimal odemeMiktari { get; set; }
        public DateTime eklenmeZamani { get; set; }
    }

    public class MasaDto
    {
        public int id { get; set; }
        public string Ad { get; set; } = null!;
        public string HashId { get; set; } = null!;
        public bool Aktif { get; set; }
    }

    public class MuzikIstatistikDto
    {
        public int toplamSarki { get; set; }
        public int aktifCalmaListesi { get; set; }
        public int tamamlananCalma { get; set; }
        public int bugunkuIstekler { get; set; }
        public decimal toplamOdeme { get; set; }
        public EnCokEkleyenMasaDto? enCokEkleyenMasa { get; set; }
    }

    public class EnCokEkleyenMasaDto
    {
        public int masaId { get; set; }
        public string masaAdi { get; set; } = null!;
        public int sayi { get; set; }
    }
}