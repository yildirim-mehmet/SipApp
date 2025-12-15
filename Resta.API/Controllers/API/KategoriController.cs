using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class KategoriController : ControllerBase
    {
        private readonly RestaContext _db;
        public ICollection<BolumKategori> BolumKategoriler { get; set; } = new List<BolumKategori>();


        public KategoriController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // GET /api/Kategori
        // AMAÇ:
        // - Tüm kategorileri listeler
        // - Yönetim ekranı
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Kategoriler
                .OrderBy(k => k.SiraNo)
                .Select(k => new
                {
                    k.Id,
                    k.Ad,
                    k.UstId,
                    k.SiraNo,
                    k.Aktif
                })
                .ToListAsync();

            return Ok(list);
        }

        // ====================================================
        // POST /api/Kategori
        // AMAÇ:
        // - Yeni kategori ekler
        // ====================================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateKategoriDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ad))
                return BadRequest("Kategori adı zorunludur.");

            // Root kategori için DB'de NULL kullanılmalı. UI/Swagger bazen 0 gönderir.
            var ustId = dto.UstId;
            if (ustId == 0) ustId = null;

            var kategori = new Kategori
            {
                Ad = dto.Ad,
                UstId = ustId,
                Renk = dto.Renk,
                SiraNo = dto.SiraNo,
                Aktif = true,
                Tarih = DateTime.Now
            };

            _db.Kategoriler.Add(kategori);
            await _db.SaveChangesAsync();

            return Ok(new { kategori.Id });
        }

        // ====================================================
        // PUT /api/Kategori/{id}
        // AMAÇ:
        // - Kategori düzenleme / aktif-pasif
        // ====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateKategoriDto dto)
        {
            var kategori = await _db.Kategoriler.FindAsync(id);
            if (kategori == null)
                return NotFound();

            kategori.Ad = dto.Ad;
            kategori.Renk = dto.Renk;
            kategori.SiraNo = dto.SiraNo;
            kategori.Aktif = dto.Aktif;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ====================================================
        // GET /api/Kategori/{id}/AltKategoriler
        // AMAÇ:
        // - Bir kategorinin alt kategorilerini getirir
        // ====================================================
        [HttpGet("{id}/AltKategoriler")]
        public async Task<IActionResult> GetAltKategoriler(int id)
        {
            var list = await _db.Kategoriler
                .Where(k => k.UstId == id && k.Aktif)
                .OrderBy(k => k.SiraNo)
                .Select(k => new
                {
                    k.Id,
                    k.Ad
                })
                .ToListAsync();

            return Ok(list);
        }

        // ====================================================
        // GET /api/Kategori/{id}/Urunler
        // AMAÇ:
        // - Kategoriye bağlı ürünleri listeler
        // ====================================================
        [HttpGet("{id}/Urunler")]
        public async Task<IActionResult> GetUrunler(int id)
        {
            var urunler = await _db.Urunler
                .Where(u =>
                    u.Aktif &&
                    u.UrunKategoriler.Any(uk => uk.KategoriId == id))
                .Select(u => new
                {
                    u.Id,
                    u.Ad,
                    u.Fiyat
                })
                .ToListAsync();

            return Ok(urunler);
        }

        // ====================================================
        // POST /api/Kategori/BolumKategori
        // AMAÇ:
        // - Bölüm → Kategori yetkilendirmesi
        // ====================================================
        [HttpPost("BolumKategori")]
        public async Task<IActionResult> AddBolumKategori(BolumKategoriDto dto)
        {
            bool exists = await _db.BolumKategori
                .AnyAsync(x => x.BolumId == dto.BolumId && x.KategoriId == dto.KategoriId);

            if (exists)
                return BadRequest("Bu eşleşme zaten var.");

            _db.BolumKategori.Add(new BolumKategori
            {
                BolumId = dto.BolumId,
                KategoriId = dto.KategoriId
            });

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ====================================================
        // POST /api/Kategori/EkranKategori
        // AMAÇ:
        // - Kategori → Ekran eşleşmesi
        // ====================================================
        [HttpPost("EkranKategori")]
        public async Task<IActionResult> AddEkranKategori(EkranKategoriDto dto)
        {
            bool exists = await _db.EkranKategoriler
                .AnyAsync(x => x.EkranId == dto.EkranId && x.KategoriId == dto.KategoriId);

            if (exists)
                return BadRequest("Bu eşleşme zaten var.");

            _db.EkranKategoriler.Add(new EkranKategori
            {
                EkranId = dto.EkranId,
                KategoriId = dto.KategoriId
            });

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ===================== DTO =========================

        public class CreateKategoriDto
        {
            public string Ad { get; set; } = null!;
            public int? UstId { get; set; }
            public string? Renk { get; set; }
            public int? SiraNo { get; set; }
        }

        public class UpdateKategoriDto
        {
            public string Ad { get; set; } = null!;
            public string? Renk { get; set; }
            public int? SiraNo { get; set; }
            public bool Aktif { get; set; }
        }

        public class BolumKategoriDto
        {
            public int BolumId { get; set; }
            public int KategoriId { get; set; }
        }

        public class EkranKategoriDto
        {
            public int EkranId { get; set; }
            public int KategoriId { get; set; }
        }
    }
}
