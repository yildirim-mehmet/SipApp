using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.DTOs.Muzik;
using Resta.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resta.API.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuzikController : ControllerBase
    {
        private readonly RestaContext _context;
        private readonly ILogger<MuzikController> _logger;

        public MuzikController(RestaContext context, ILogger<MuzikController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Muzik/sarkilar
        [HttpGet("sarkilar")]
        [ProducesResponseType(typeof(IEnumerable<SarkiDto>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<SarkiDto>>> GetSarkilar()
        {
            try
            {
                var sarkilar = await _context.SarkiListesi
                    .Where(s => s.aktif)
                    .OrderBy(s => s.ad)
                    .Select(s => new SarkiDto
                    {
                        id = s.id,
                        ad = s.ad,
                        dosyaYolu = s.dosyaYolu,
                        sure = s.sure,
                        aktif = s.aktif,
                        eklenmeTarihi = s.eklenmeTarihi
                    })
                    .ToListAsync();

                return Ok(sarkilar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şarkılar getirme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // GET: api/Muzik/kuyruk
        [HttpGet("kuyruk")]
        [ProducesResponseType(typeof(IEnumerable<CalmaListesiDto>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<CalmaListesiDto>>> GetKuyruk()
        {
            try
            {
                var kuyruk = await _context.CalmaListesi
                    .Include(c => c.Sarki)
                    .Include(c => c.Masa)  // Mevcut Masa entity'si
                    .Where(c => !c.calindi)
                    .OrderByDescending(c => c.siraDegeri)
                    .ThenBy(c => c.id)
                    .Select(c => new CalmaListesiDto
                    {
                        id = c.id,
                        sarkiId = c.sarkiId,
                        sarkiAdi = c.Sarki.ad,
                        dosyaYolu = c.Sarki.dosyaYolu,
                        sure = c.Sarki.sure,
                        masaId = c.masaId,
                        masaAdi = c.Masa.Ad,  // Mevcut Masa.Ad property'si
                        siraDegeri = c.siraDegeri,
                        calindi = c.calindi,
                        odemeMiktari = c.odemeMiktari,
                        eklenmeZamani = c.eklenmeZamani
                       // , aciklama = c.aciklama
                    })
                    .ToListAsync();

                return Ok(kuyruk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kuyruk getirme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // GET: api/Muzik/siradaki
        [HttpGet("siradaki")]
        [ProducesResponseType(typeof(CalmaListesiDto), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<CalmaListesiDto>> GetSiradaki()
        {
            try
            {
                var siradaki = await _context.CalmaListesi
                    .Include(c => c.Sarki)
                    .Include(c => c.Masa)  // Mevcut Masa entity'si
                    .Where(c => !c.calindi)
                    .OrderByDescending(c => c.siraDegeri)
                    .ThenBy(c => c.id)
                    .FirstOrDefaultAsync();

                if (siradaki == null)
                {
                    // Otomatik şarkı başlat
                    await OtomatikSarkiBaslat();

                    siradaki = await _context.CalmaListesi
                        .Include(c => c.Sarki)
                        .Include(c => c.Masa)
                        .Where(c => !c.calindi)
                        .OrderByDescending(c => c.siraDegeri)
                        .ThenBy(c => c.id)
                        .FirstOrDefaultAsync();
                }

                if (siradaki == null)
                    return NotFound(new { error = "Çalınacak şarkı bulunamadı" });

                var dto = new CalmaListesiDto
                {
                    id = siradaki.id,
                    sarkiId = siradaki.sarkiId,
                    sarkiAdi = siradaki.Sarki.ad,
                    dosyaYolu = siradaki.Sarki.dosyaYolu,
                    sure = siradaki.Sarki.sure,
                    masaId = siradaki.masaId,
                    masaAdi = siradaki.Masa.Ad,  // Mevcut Masa.Ad property'si
                    siraDegeri = siradaki.siraDegeri,
                    calindi = siradaki.calindi,
                    odemeMiktari = siradaki.odemeMiktari,
                    eklenmeZamani = siradaki.eklenmeZamani
                   // , aciklama = siradaki.aciklama
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sıradaki şarkı getirme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // POST: api/Muzik/ekle
        [HttpPost("ekle")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SarkiEkle([FromBody] SarkiEkleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Geçersiz veri", detaylar = ModelState });

                // KURAL 1: Aynı masa arka arkaya ekleyemez
                var masaninSonEklemesi = await _context.CalmaListesi
                    .Where(c => c.masaId == request.masaId)
                    .OrderByDescending(c => c.eklenmeZamani)
                    .FirstOrDefaultAsync();

                if (masaninSonEklemesi != null &&
                    (DateTime.UtcNow - masaninSonEklemesi.eklenmeZamani).TotalSeconds < 60)
                {
                    return BadRequest(new
                    {
                        error = "Aynı masadan ardışık istek yapılamaz",
                        beklemeSuresi = 60,
                        kalanSure = 60 - (int)(DateTime.UtcNow - masaninSonEklemesi.eklenmeZamani).TotalSeconds
                    });
                }

                // KURAL 2: Aynı şarkı hemen ardışık eklenemez
                var ayniSarkininSonEklenmesi = await _context.CalmaListesi
                    .Where(c => c.sarkiId == request.sarkiId)
                    .OrderByDescending(c => c.eklenmeZamani)
                    .FirstOrDefaultAsync();

                if (ayniSarkininSonEklenmesi != null &&
                    (DateTime.UtcNow - ayniSarkininSonEklenmesi.eklenmeZamani).TotalSeconds < 60)
                {
                    return BadRequest(new
                    {
                        error = "Bu şarkı kısa süre önce eklendi",
                        beklemeSuresi = 60,
                        kalanSure = 60 - (int)(DateTime.UtcNow - ayniSarkininSonEklenmesi.eklenmeZamani).TotalSeconds
                    });
                }

                // Şarkı kontrolü
                var sarki = await _context.SarkiListesi.FindAsync(request.sarkiId);
                if (sarki == null)
                    return NotFound(new { error = "Şarkı bulunamadı" });

                // Masa kontrolü (Mevcut Masa tablosundan)
                var masa = await _context.Masalar.FindAsync(request.masaId);
                if (masa == null)
                    return NotFound(new { error = "Masa bulunamadı" });

                // CalmaListesi'ne ekle
                var calmaListesi = new CalmaListesi
                {
                    sarkiId = request.sarkiId,
                    masaId = request.masaId,
                    siraDegeri = 1,
                    calindi = false,
                    eklenmeZamani = DateTime.UtcNow,
                    odemeMiktari = 0,
                    //aciklama = DateTime.Now.ToString()
                };

                _context.CalmaListesi.Add(calmaListesi);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Şarkı eklendi: {SarkiAd}, Masa: {MasaAd}, ID: {Id}",
                    sarki.ad, masa.Ad, calmaListesi.id);

                return Ok(new
                {
                    success = true,
                    message = "Şarkı sıraya eklendi",
                    calmaListesiId = calmaListesi.id,
                    sarkiAdi = sarki.ad,
                    siraDegeri = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şarkı ekleme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // POST: api/Muzik/odemeli-ekle
        [HttpPost("odemeli-ekle")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> OdemeliSarkiEkle([FromBody] OdemeliSarkiEkleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Geçersiz veri", detaylar = ModelState });

                if (request.odemeMiktari < 2)
                    return BadRequest(new { error = "Ödeme en az 2 TL olmalıdır" });

                // PayTR doğrulama simülasyonu
                bool odemeOnaylandi = await PayTRDogrula(request.odemeReferans);
                if (!odemeOnaylandi)
                    return BadRequest(new { error = "Ödeme doğrulanamadı" });

                // KURAL: Aynı masa arka arkaya ekleyemez (ödemeli için 30 sn)
                var masaninSonEklemesi = await _context.CalmaListesi
                    .Where(c => c.masaId == request.masaId)
                    .OrderByDescending(c => c.eklenmeZamani)
                    .FirstOrDefaultAsync();

                if (masaninSonEklemesi != null &&
                    (DateTime.UtcNow - masaninSonEklemesi.eklenmeZamani).TotalSeconds < 30)
                {
                    return BadRequest(new { error = "Ödemeli istekler için 30 saniye beklemelisiniz" });
                }

                var sarki = await _context.SarkiListesi.FindAsync(request.sarkiId);
                if (sarki == null)
                    return NotFound(new { error = "Şarkı bulunamadı" });

                // Masa kontrolü (Mevcut Masa tablosundan)
                var masa = await _context.Masalar.FindAsync(request.masaId);
                if (masa == null)
                    return NotFound(new { error = "Masa bulunamadı" });

                // Ödemeli ekle
                var calmaListesi = new CalmaListesi
                {
                    sarkiId = request.sarkiId,
                    masaId = request.masaId,
                    siraDegeri = request.odemeMiktari,
                    calindi = false,
                    eklenmeZamani = DateTime.UtcNow,
                    odemeMiktari = request.odemeMiktari,
                    //aciklama = DateTime.Now.ToString()
                };

                _context.CalmaListesi.Add(calmaListesi);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ödemeli şarkı eklendi: {SarkiAd}, Masa: {MasaAd}, Ödeme: {Odeme}TL",
                    sarki.ad, masa.Ad, request.odemeMiktari);

                return Ok(new
                {
                    success = true,
                    message = $"Ödemeli şarkı sıraya eklendi (Öncelik: {request.odemeMiktari})",
                    calmaListesiId = calmaListesi.id,
                    sarkiAdi = sarki.ad,
                    siraDegeri = request.odemeMiktari
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödemeli şarkı ekleme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // POST: api/Muzik/{id}/calindi
        [HttpPost("{id}/calindi")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SarkiCalindi(int id)
        {
            try
            {
                var calmaListesi = await _context.CalmaListesi
                    .Include(c => c.Sarki)
                    .Include(c => c.Masa)
                    .FirstOrDefaultAsync(c => c.id == id);

                if (calmaListesi == null)
                    return NotFound(new { error = "Kayıt bulunamadı" });

                calmaListesi.calindi = true;

                // Çalınma geçmişine ekle
                var calinmaGecmisi = new CalinmaGecmisi
                {
                    calmaListesiId = calmaListesi.id,
                    masaId = calmaListesi.masaId,
                    calinmaZamani = DateTime.UtcNow
                };
                _context.CalinmaGecmisi.Add(calinmaGecmisi);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Şarkı çalındı olarak işaretlendi: {SarkiAd}, ID: {Id}",
                    calmaListesi.Sarki.ad, id);

                return Ok(new { success = true, message = "Şarkı çalındı olarak işaretlendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalındı işaretleme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // GET: api/Muzik/istatistikler
        [HttpGet("istatistikler")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(MuzikIstatistikDto), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetIstatistikler()
        {
            try
            {
                var bugun = DateTime.Today;

                var istatistikler = new MuzikIstatistikDto
                {
                    toplamSarki = await _context.SarkiListesi.CountAsync(),
                    aktifCalmaListesi = await _context.CalmaListesi.CountAsync(c => !c.calindi),
                    tamamlananCalma = await _context.CalinmaGecmisi.CountAsync(),
                    bugunkuIstekler = await _context.CalinmaGecmisi
                        .CountAsync(c => c.calinmaZamani.Date == bugun),
                    toplamOdeme = await _context.CalmaListesi
                        .Where(c => c.odemeMiktari > 0)
                        .SumAsync(c => c.odemeMiktari),
                    enCokEkleyenMasa = await _context.CalmaListesi
                        .Include(c => c.Masa)
                        .GroupBy(c => c.masaId)
                        .Select(g => new EnCokEkleyenMasaDto
                        {
                            masaId = g.Key,
                            masaAdi = g.First().Masa.Ad,
                            sayi = g.Count()
                        })
                        .OrderByDescending(x => x.sayi)
                        .FirstOrDefaultAsync()
                };

                return Ok(istatistikler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstatistik getirme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // GET: api/Muzik/tum-sira
        [HttpGet("tum-sira")]
        [ProducesResponseType(typeof(IEnumerable<CalmaListesiDto>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<CalmaListesiDto>>> GetTumSira()
        {
            try
            {
                var tumSira = await _context.CalmaListesi
                    .Include(c => c.Sarki)
                    .Include(c => c.Masa)
                    .OrderByDescending(c => c.siraDegeri)
                    .ThenBy(c => c.id)
                    .Select(c => new CalmaListesiDto
                    {
                        id = c.id,
                        sarkiId = c.sarkiId,
                        sarkiAdi = c.Sarki.ad,
                        dosyaYolu = c.Sarki.dosyaYolu,
                        sure = c.Sarki.sure,
                        masaId = c.masaId,
                        masaAdi = c.Masa.Ad,
                        siraDegeri = c.siraDegeri,
                        calindi = c.calindi,
                        odemeMiktari = c.odemeMiktari,
                        //aciklama = c.aciklama,
                        eklenmeZamani = c.eklenmeZamani
                    })
                    .ToListAsync();

                return Ok(tumSira);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm sıra getirme hatası");
                return StatusCode(500, new { error = "Sunucu hatası" });
            }
        }

        // Yardımcı metodlar
        private async Task<bool> PayTRDogrula(string odemeReferans)
        {
            await Task.Delay(100); // Simülasyon
            return !string.IsNullOrEmpty(odemeReferans);
        }

        private async Task OtomatikSarkiBaslat()
        {
            try
            {
                var aktifSarkilar = await _context.SarkiListesi
                    .Where(s => s.aktif)
                    .ToListAsync();

                // Admin masa bul (Mevcut Masa tablosundan)
                var adminMasa = await _context.Masalar
                    .FirstOrDefaultAsync(m => m.Ad.Contains("Admin") || m.Id == 1);

                if (adminMasa == null && await _context.Masalar.AnyAsync())
                    adminMasa = await _context.Masalar.FirstAsync();

                if (adminMasa != null && aktifSarkilar.Any())
                {
                    foreach (var sarki in aktifSarkilar)
                    {
                        var zatenVar = await _context.CalmaListesi
                            .AnyAsync(c => c.sarkiId == sarki.id && !c.calindi);

                        if (!zatenVar)
                        {
                            var calmaListesi = new CalmaListesi
                            {
                                sarkiId = sarki.id,
                                masaId = adminMasa.Id,
                                siraDegeri = 0,
                                calindi = false,
                                eklenmeZamani = DateTime.UtcNow,
                                odemeMiktari = 0
                               // ,aciklama = "Otomatik ekleme"+ DateTime.Now.ToString()
                            };

                            _context.CalmaListesi.Add(calmaListesi);
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Otomatik {Count} şarkı sıraya eklendi", aktifSarkilar.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik şarkı başlatma hatası");
            }
        }
    }
}