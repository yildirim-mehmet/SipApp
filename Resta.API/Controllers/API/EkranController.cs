using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class EkranController : ControllerBase
    {
        private readonly RestaContext _db;

        public EkranController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // POST /api/Ekran
        // AMAÇ:
        // - Yeni ekran tanımı (mutfak, bar, kasa)
        // ====================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEkranDto dto)
        {
            var ekran = new Ekran
            {
                Ad = dto.Ad,
                Tip = dto.Tip, // mutfak / bar / kasa
                IpAdresi = dto.IpAdresi,
                BaglantiKodu = dto.BaglantiKodu,
                Aktif = true,
                Tarih = DateTime.Now
            };

            _db.Ekranlar.Add(ekran);
            await _db.SaveChangesAsync();

            return Ok(ekran);
        }

        // ====================================================
        // GET /api/Ekran/{id}/Siparisler
        // AMAÇ:
        // - Bu ekrana düşmesi gereken AKTİF siparişleri getirir
        //
        // ZİNCİR:
        // AdisyonKalem → Urun → UrunKategori → Kategori → EkranKategori
        // ====================================================
        [HttpGet("{id}/Siparisler")]
        public async Task<IActionResult> GetEkranSiparisler(int id)
        {
            var siparisler = await _db.AdisyonKalemler
                .Include(k => k.Adisyon)
                .Include(k => k.Urun)
                .Where(k =>
                    k.SiparisDurumu != 2 && // iptal olmayan
                    k.Urun.UrunKategoriler
                        .Any(uk =>
                            uk.Kategori.EkranKategoriler
                                .Any(ek => ek.EkranId == id)))
                .OrderBy(k => k.SiparisVerilmeZamani)
                .Select(k => new
                {
                    k.Id,
                    k.AdisyonId,
                    k.Adisyon.MasaId,
                    UrunAd = k.Urun.Ad,
                    k.Adet,
                    k.SiparisDurumu,
                    k.SiparisVerilmeZamani
                })
                .ToListAsync();

            return Ok(siparisler);
        }

        // ====================================================
        // DTO
        // ====================================================
        public class CreateEkranDto
        {
            public string Ad { get; set; } = null!;
            public string Tip { get; set; } = null!;
            public string? IpAdresi { get; set; }
            public string? BaglantiKodu { get; set; }
        }
    }
}
