using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdisyonController : ControllerBase
    {
        private readonly RestaContext _db;

        public AdisyonController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // GET /api/Adisyon/masa/{masaId}/aktif
        // AMAÇ:
        // - Masanın aktif (açık) adisyonu var mı?
        // - Varsa getirir, yoksa null döner
        //
        // NEREDEN KULLANILIR:
        // - Garson/Kasa ekranında masanın "dolu mu boş mu" bilgisinde
        // - Müşteri menüye girdiğinde "devam eden adisyon" var mı kontrolünde
        //
        // NOT:
        // - Burada adisyon OLUŞTURMA YOK (side-effect yok)
        // ====================================================
        [HttpGet("masa/{masaId}/aktif")]
        public async Task<IActionResult> GetAktifAdisyon(int masaId)
        {
            // masa var mı?
            bool masaVarMi = await _db.Masalar.AsNoTracking().AnyAsync(m => m.Id == masaId && m.Aktif);
            if (!masaVarMi) return NotFound("Masa bulunamadı.");

            var adisyon = await _db.Adisyonlar
                .AsNoTracking()
                .Where(a => a.MasaId == masaId && a.Durum == (int)AdisyonDurum.Acik)
                .Select(a => new
                {
                    a.Id,
                    a.MasaId,
                    a.Durum,
                    a.KisiSayisi,
                    a.ToplamTutar,
                    a.OdemeSekli,
                    a.AcilisZamani,
                    a.KapanisZamani
                })
                .FirstOrDefaultAsync();

            // aktif adisyon yoksa null dönmek isteniyor (kural)
            return Ok(adisyon);
        }

        // ====================================================
        // GET /api/Adisyon/masa/{masaId}/son
        // AMAÇ:
        // - Masanın son kapanan (ödenmiş/iptal) adisyonunu getirir
        //
        // NEREDEN KULLANILIR:
        // - Yönetim paneli / geçmiş adisyon görüntüleme
        // - Masa detay ekranı
        // ====================================================
        [HttpGet("masa/{masaId}/son")]
        public async Task<IActionResult> GetSonAdisyon(int masaId)
        {
            bool masaVarMi = await _db.Masalar.AsNoTracking().AnyAsync(m => m.Id == masaId);
            if (!masaVarMi) return NotFound("Masa bulunamadı.");

            var adisyon = await _db.Adisyonlar
                .AsNoTracking()
                .Where(a => a.MasaId == masaId && a.Durum != (int)AdisyonDurum.Acik)
                .OrderByDescending(a => a.KapanisZamani ?? a.AcilisZamani)
                .Select(a => new
                {
                    a.Id,
                    a.MasaId,
                    a.Durum,
                    a.ToplamTutar,
                    a.OdemeSekli,
                    a.AcilisZamani,
                    a.KapanisZamani
                })
                .FirstOrDefaultAsync();

            return Ok(adisyon);
        }

        // ====================================================
        // GET /api/Adisyon/masa/{masaId}/durum
        // AMAÇ:
        // - Masanın dolu/boş durumunu döner
        //
        // KURAL:
        // - Aktif adisyon varsa -> dolu
        // - Yoksa -> boş
        //
        // NOT:
        // - Garson/Kasa ekranında masaları boyamak için ideal
        // ====================================================
        [HttpGet("masa/{masaId}/durum")]
        public async Task<IActionResult> GetMasaDurum(int masaId)
        {
            bool masaVarMi = await _db.Masalar.AsNoTracking().AnyAsync(m => m.Id == masaId && m.Aktif);
            if (!masaVarMi) return NotFound("Masa bulunamadı.");

            bool dolu = await _db.Adisyonlar.AsNoTracking()
                .AnyAsync(a => a.MasaId == masaId && a.Durum == (int)AdisyonDurum.Acik);

            return Ok(new { masaId, dolu });
        }

        // ====================================================
        // POST /api/Adisyon/masa/{masaId}
        // AMAÇ:
        // - Garson / Kasa tarafından masaya MANUEL adisyon açar
        //
        // KURAL:
        // - Masada zaten açık adisyon varsa hata döner
        //
        // NOT:
        // - Müşteri tarafında adisyonu "Sipariş Ver" açacak (SiparisController)
        // - Burada sadece manuel açma var
        // ====================================================
        [HttpPost("masa/{masaId}")]
        public async Task<IActionResult> OpenAdisyonManuel(int masaId, [FromBody] OpenAdisyonDto? dto)
        {
            var masa = await _db.Masalar.FirstOrDefaultAsync(m => m.Id == masaId && m.Aktif);
            if (masa == null) return NotFound("Masa bulunamadı.");

            bool varMi = await _db.Adisyonlar.AnyAsync(a => a.MasaId == masaId && a.Durum == (int)AdisyonDurum.Acik);
            if (varMi) return BadRequest("Bu masada zaten açık bir adisyon var.");

            var adisyon = new Adisyon
            {
                MasaId = masaId,
                Durum = (int)AdisyonDurum.Acik,
                KisiSayisi = dto?.KisiSayisi,
                ToplamTutar = 0,
                OdemeSekli = null,
                AcilisZamani = DateTime.Now,
                KapanisZamani = null,
                Ekleyen = dto?.Ekleyen
            };

            _db.Adisyonlar.Add(adisyon);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                adisyon.Id,
                adisyon.MasaId,
                adisyon.Durum,
                adisyon.KisiSayisi,
                adisyon.ToplamTutar,
                adisyon.AcilisZamani
            });
        }

        // ====================================================
        // PUT /api/Adisyon/{id}/kapat
        // AMAÇ:
        // - Ödeme sonrası adisyon kapatılır (kasa)
        //
        // KURAL:
        // - Sadece AÇIK adisyon kapatılabilir
        // - ToplamTutar DB'deki kalemlerden yeniden hesaplanır
        // - (iptal edilen kalemler toplamdan düşer)
        //
        // NOT:
        // - Ödeme tablosunu daha sonra bağlayacağız
        // ====================================================
        [HttpPut("{id}/kapat")]
        public async Task<IActionResult> CloseAdisyon(int id, [FromBody] CloseAdisyonDto? dto)
        {
            var adisyon = await _db.Adisyonlar
                .Include(a => a.Kalemler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adisyon == null) return NotFound("Adisyon bulunamadı.");
            if (adisyon.Durum != (int)AdisyonDurum.Acik) return BadRequest("Sadece açık adisyon kapatılabilir.");

            // Toplamı yeniden hesapla (iptal=2 hariç)
            adisyon.ToplamTutar = RecalcTotal(adisyon);

            adisyon.OdemeSekli = dto?.OdemeSekli; // "masada" / "online" vb (ileride)
            adisyon.Durum = (int)AdisyonDurum.Odenmis;
            adisyon.KapanisZamani = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                adisyon.Id,
                adisyon.MasaId,
                adisyon.Durum,
                adisyon.ToplamTutar,
                adisyon.OdemeSekli,
                adisyon.AcilisZamani,
                adisyon.KapanisZamani
            });
        }

        // ====================================================
        // Helper: Toplam hesap
        // ====================================================

        // SiparisDurumu: null/onay bekliyor, 0 hazırlanıyor, 1 masada, 2 iptal
        // Toplama iptal (2) dahil edilmez
        private static decimal RecalcTotal(Adisyon adisyon)
        {
            return adisyon.Kalemler
                .Where(k => k.SiparisDurumu != 2)
                .Sum(k => k.AraToplam);
            //.Sum(k => k.AraToplam ?? 0m);
        }


        // ====================================================
        // DTO'lar
        // ====================================================
        public class OpenAdisyonDto
        {
            public int? KisiSayisi { get; set; }
            public int? Ekleyen { get; set; } // garson/kasa kullanıcı id (ileride auth ile gelecek)
        }

        public class CloseAdisyonDto
        {
            public string? OdemeSekli { get; set; } // "masada" / "online" / "nakit" / "pos" vb.
        }
    }
}
