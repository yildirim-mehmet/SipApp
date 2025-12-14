using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrunController : ControllerBase
    {
        private readonly RestaContext _db;

        public UrunController(RestaContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------
        // GET /api/Urun/{id}
        // AMAÇ:
        // - Ürün detayını getirir
        // - Kategorileriyle birlikte
        // ----------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var urun = await _db.Urunler
                .Include(u => u.UrunKategoriler)
                    .ThenInclude(uk => uk.Kategori)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (urun == null)
                return NotFound("Ürün bulunamadı.");

            return Ok(urun);
        }

        // ----------------------------------------------------
        // POST /api/Urun
        // AMAÇ:
        // - Yeni ürün oluşturur
        // - Menüye dahil olacak temel varlık
        // ----------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUrunDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ad))
                return BadRequest("Ürün adı boş olamaz.");

            var urun = new Urun
            {
                Ad = dto.Ad,
                Aciklama = dto.Aciklama,
                Fiyat = dto.Fiyat,
                HazirlamaSuresiDakika = dto.HazirlamaSuresiDakika,
                Aktif = true,
                Ekleyen = dto.Ekleyen,
                Tarih = DateTime.Now
            };

            _db.Urunler.Add(urun);
            await _db.SaveChangesAsync();

            return Ok(urun);
        }

        // ----------------------------------------------------
        // PUT /api/Urun/{id}
        // AMAÇ:
        // - Ürün bilgilerini günceller
        // - Fiyat, açıklama, aktiflik
        // ----------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUrunDto dto)
        {
            var urun = await _db.Urunler.FindAsync(id);
            if (urun == null)
                return NotFound("Ürün bulunamadı.");

            urun.Ad = dto.Ad;
            urun.Aciklama = dto.Aciklama;
            urun.Fiyat = dto.Fiyat;
            urun.HazirlamaSuresiDakika = dto.HazirlamaSuresiDakika;
            urun.Aktif = dto.Aktif;

            await _db.SaveChangesAsync();
            return Ok(urun);
        }

        // ----------------------------------------------------
        // POST /api/UrunKategori
        // AMAÇ:
        // - Bir ürünü birden fazla kategoriye bağlar
        // - Menü + ekran zincirinin temelidir
        // ----------------------------------------------------
        [HttpPost("/api/UrunKategori")]
        public async Task<IActionResult> AddUrunKategori([FromBody] AddUrunKategoriDto dto)
        {
            var urun = await _db.Urunler.FindAsync(dto.UrunId);
            if (urun == null)
                return NotFound("Ürün bulunamadı.");

            var kategori = await _db.Kategoriler.FindAsync(dto.KategoriId);
            if (kategori == null)
                return NotFound("Kategori bulunamadı.");

            bool varMi = await _db.UrunKategori
                .AnyAsync(x => x.UrunId == dto.UrunId && x.KategoriId == dto.KategoriId);

            if (varMi)
                return BadRequest("Bu ürün zaten bu kategoriye bağlı.");

            var link = new UrunKategori
            {
                UrunId = dto.UrunId,
                KategoriId = dto.KategoriId
            };

            _db.UrunKategori.Add(link);
            await _db.SaveChangesAsync();

            return Ok(link);
        }

        // ----------------------------------------------------
        // GET /api/Urun/{id}/Ekranlar
        // AMAÇ:
        // - Ürünün hangi ekranlara düşeceğini HESAPLAR
        // - Hard-code YOK
        // ----------------------------------------------------
        [HttpGet("{id}/Ekranlar")]
        public async Task<IActionResult> GetUrunEkranlar(int id)
        {
            var ekranlar = await _db.UrunKategori
                .Where(uk => uk.UrunId == id)
                .SelectMany(uk => uk.Kategori.EkranKategoriler)
                .Select(ek => new
                {
                    ek.Ekran.Id,
                    ek.Ekran.Ad,
                    ek.Ekran.Tip
                })
                .Distinct()
                .ToListAsync();

            return Ok(ekranlar);
        }

        // ----------------------------------------------------
        // DTO'LAR
        // ----------------------------------------------------

        public class CreateUrunDto
        {
            public string Ad { get; set; } = null!;
            public string? Aciklama { get; set; }
            public decimal Fiyat { get; set; }
            public int? HazirlamaSuresiDakika { get; set; }
            public int? Ekleyen { get; set; }
        }

        public class UpdateUrunDto
        {
            public string Ad { get; set; } = null!;
            public string? Aciklama { get; set; }
            public decimal Fiyat { get; set; }
            public int? HazirlamaSuresiDakika { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddUrunKategoriDto
        {
            public int UrunId { get; set; }
            public int KategoriId { get; set; }
        }
    }
}
