using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;
using Resta.API.Helpers;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasaController : ControllerBase
    {
        private readonly RestaContext _db;
        private readonly IConfiguration _config;

        public MasaController(RestaContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ====================================================
        // GET /api/Masa?bolumId=
        // AMAÇ:
        // - Bölüme göre masaları listeler
        // - Garson / Kasa ekranı
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> GetByBolum([FromQuery] int? bolumId)
        {
            var query = _db.Masalar.AsQueryable();

            if (bolumId.HasValue)
                query = query.Where(m => m.BolumId == bolumId);

            var list = await query
                .Select(m => new
                {
                    m.Id,
                    m.Ad,
                    m.Kod,
                    m.Aktif
                })
                .ToListAsync();

            return Ok(list);
        }

        // ====================================================
        // POST /api/Masa
        // AMAÇ:
        // - Yeni masa oluşturur
        // ====================================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateMasaDto dto)
        {
            if (dto.BolumId <= 0)
                return BadRequest("BolumId zorunludur.");

            var masa = new Masa
            {
                BolumId = dto.BolumId,
                Kod = dto.Kod,
                Ad = dto.Ad,
                Aciklama = dto.Aciklama,
                Aktif = true,
                Tarih = DateTime.Now
            };

            _db.Masalar.Add(masa);
            await _db.SaveChangesAsync();

            return Ok(new { masa.Id });
        }

        // ====================================================
        // PUT /api/Masa/{id}
        // AMAÇ:
        // - Masa düzenleme / aktif-pasif
        // ====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateMasaDto dto)
        {
            var masa = await _db.Masalar.FindAsync(id);
            if (masa == null)
                return NotFound("Masa bulunamadı.");

            masa.Ad = dto.Ad;
            masa.Kod = dto.Kod;
            masa.Aciklama = dto.Aciklama;
            masa.Aktif = dto.Aktif;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ====================================================
        // GET /api/Masa/{id}
        // AMAÇ:
        // - Masa detay bilgisi
        // ====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var masa = await _db.Masalar
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.Ad,
                    m.Kod,
                    m.BolumId,
                    m.Aktif
                })
                .FirstOrDefaultAsync();

            if (masa == null)
                return NotFound();

            return Ok(masa);
        }

        // ====================================================
        // GET /api/Masa/{id}/qr
        // AMAÇ:
        // - Masa için QR üretir
        // - QR → /menu/masa/{masaId}
        // ====================================================
        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQr(int id)
        {
            var masa = await _db.Masalar.FindAsync(id);
            if (masa == null)
                return NotFound();

            string baseUrl =
                _db.Ayarlar.FirstOrDefault()?.QrBaseUrl
                ?? _config["Ayarlar:QrBaseUrl"]
                ?? "https://localhost:7286/menu/masa/";

            string qrUrl = $"{baseUrl}{masa.Id}";

            var pngBytes = QrHelper.GenerateQrPng(qrUrl);
            return File(pngBytes, "image/png");
        }

        // ====================================================
        // GET /api/Masa/{id}/durum
        // AMAÇ:
        // - Masa dolu mu boş mu?
        // ====================================================
        //[HttpGet("{id}/durum")]
        [HttpGet("{id}/durum")]
        public async Task<IActionResult> GetDurum(int id)
        {
            bool doluMu = await _db.Adisyonlar
                .AnyAsync(a => a.MasaId == id && a.Durum == (int)AdisyonDurum.Acik);

            return Ok(new
            {
                durum = doluMu ? "dolu" : "bos"
            });
        }


        // ====================================================
        // GET /api/Masa/{id}/aktif-adisyon
        // AMAÇ:
        // - Masanın açık adisyonu varsa getirir
        // ====================================================
        [HttpGet("{id}/aktif-adisyon")]
        public async Task<IActionResult> GetAktifAdisyon(int id)
        {
            var adisyon = await _db.Adisyonlar
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.MasaId == id &&
                    a.Durum == (int)AdisyonDurum.Acik);

            if (adisyon == null)
                return Ok(null); // 🔴 204 DEĞİL

            return Ok(new
            {
                adisyon.Id
            });
        }


        // ====================================================
        // GET /api/Masa/{id}/son-adisyon
        // AMAÇ:
        // - Masanın en son kapanan adisyonu
        // ====================================================
        [HttpGet("{id}/son-adisyon")]
        public async Task<IActionResult> GetSonAdisyon(int id)
        {
            var adisyon = await _db.Adisyonlar
                .Where(a => a.MasaId == id && a.Durum != (int)AdisyonDurum.Acik)
                .OrderByDescending(a => a.KapanisZamani)
                .Select(a => new
                {
                    a.Id,
                    a.ToplamTutar,
                    a.KapanisZamani
                })
                .FirstOrDefaultAsync();

            return Ok(adisyon);
        }

        // ===================== DTO =========================

        public class CreateMasaDto
        {
            public int BolumId { get; set; }
            public string? Kod { get; set; }
            public string? Ad { get; set; }
            public string? Aciklama { get; set; }
        }

        public class UpdateMasaDto
        {
            public string? Kod { get; set; }
            public string? Ad { get; set; }
            public string? Aciklama { get; set; }
            public bool Aktif { get; set; }
        }
    }
}
