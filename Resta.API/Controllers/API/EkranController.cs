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
        // GET /api/Ekran
        // AMAÇ:
        // - Tüm ekranları listeler (şimdilik auth yok)
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> TumEkranlar()
        {
            var ekranlar = await _db.Ekranlar
                .Select(e => new
                {
                    e.Id,
                    e.Ad
                })
                .ToListAsync();

            return Ok(ekranlar);
        }

        // ====================================================
        // GET /api/Ekran/{ekranId}/siparisler
        // AMAÇ:
        // - Bu ekrana bağlı kategorilerin
        // - Aktif adisyonlardaki sipariş kalemlerini getirir
        // ====================================================
        // ====================================================
        // GET /api/Ekran/{ekranId}/siparisler
        // AMAÇ:
        // - Ekrana yetkili kategorilerdeki
        // - Aktif adisyonlara ait siparişleri getirir
        // ====================================================
        // ====================================================
        // GET /api/Ekran/{id}/Siparisler
        // AMAÇ:
        // - Bu ekrana düşmesi gereken AKTİF siparişleri getirir
        //
        // ZİNCİR:
        // AdisyonKalem → Urun → UrunKategori → Kategori → EkranKategori
        // ====================================================
        [HttpGet("{ekranId:int}/siparisler")]
        public async Task<IActionResult> EkranSiparisleri(int ekranId)
        {
            var siparisler = await _db.AdisyonKalemler
                .Include(k => k.Adisyon)
                    .ThenInclude(a => a.Masa)
                .Include(k => k.Urun)
                    .ThenInclude(u => u.UrunKategoriler)
                        .ThenInclude(uk => uk.Kategori)
                            .ThenInclude(k => k.EkranKategoriler)
                .Where(k =>
                    k.SiparisDurumu != 2 && // iptal olmayan
                    k.Adisyon.Durum == (int)AdisyonDurum.Acik &&
                    k.Urun.UrunKategoriler
                        .Any(uk =>
                            uk.Kategori.EkranKategoriler
                                .Any(ek => ek.EkranId == ekranId)))
                .OrderBy(k => k.SiparisVerilmeZamani)
                .Select(k => new
                {
                    kalemId = k.Id,
                    masaId = k.Adisyon.MasaId,
                    masaAdi = k.Adisyon.Masa.Ad,
                    urunAdi = k.Urun.Ad,
                    adet = k.Adet,
                    siparisDurumu = k.SiparisDurumu,
                    zaman = k.SiparisVerilmeZamani
                })
                .ToListAsync();

            return Ok(siparisler);
        }



        // ====================================================
        // PUT /api/Ekran/siparis/{kalemId}/durum
        // AMAÇ:
        // - Ekran tarafından sipariş durumu güncellenir
        // ====================================================
        [HttpPut("siparis/{kalemId}/durum")]
        public async Task<IActionResult> EkranSiparisDurumGuncelle(
            int kalemId,
            [FromBody] int yeniDurum)
        {
            var kalem = await _db.AdisyonKalemler
                .FirstOrDefaultAsync(k => k.Id == kalemId);

            if (kalem == null)
                return NotFound();

            kalem.SiparisDurumu = yeniDurum;
            await _db.SaveChangesAsync();

            return Ok();
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
