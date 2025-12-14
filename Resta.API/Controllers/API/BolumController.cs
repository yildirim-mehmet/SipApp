using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class BolumController : ControllerBase
    {
        private readonly RestaContext _db;

        public BolumController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // GET /api/Bolum
        // AMAÇ:
        // - Sistemdeki tüm bölümleri listeler
        // - Yönetim paneli, masa oluşturma vb. için kullanılır
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Bolumler
                .OrderBy(b => b.Ad)
                .Select(b => new
                {
                    b.Id,
                    b.Ad,
                    b.Kod,
                    b.Aktif
                })
                .ToListAsync();

            return Ok(list);
        }

        // ====================================================
        // GET /api/Bolum/{id}
        // AMAÇ:
        // - Tek bir bölümün detayını getirir
        // ====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bolum = await _db.Bolumler
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    b.Ad,
                    b.Kod,
                    b.Aciklama,
                    b.Aktif
                })
                .FirstOrDefaultAsync();

            if (bolum == null)
                return NotFound("Bölüm bulunamadı.");

            return Ok(bolum);
        }

        // ====================================================
        // POST /api/Bolum
        // AMAÇ:
        // - Yeni bölüm oluşturur
        // - Kod opsiyoneldir, benzersiz olmak zorunda değildir
        // ====================================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateBolumDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ad))
                return BadRequest("Bölüm adı boş olamaz.");

            var bolum = new Bolum
            {
                Ad = dto.Ad,
                Kod = dto.Kod,
                Aciklama = dto.Aciklama,
                Aktif = dto.Aktif,
                Ekleyen = dto.Ekleyen,
                Tarih = DateTime.Now
            };

            _db.Bolumler.Add(bolum);
            await _db.SaveChangesAsync();

            return Ok(new { bolum.Id });
        }

        // ====================================================
        // PUT /api/Bolum/{id}
        // AMAÇ:
        // - Bölüm bilgilerini günceller
        // - Aktif / Pasif yapılabilir
        // ====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBolumDto dto)
        {
            var bolum = await _db.Bolumler.FindAsync(id);
            if (bolum == null)
                return NotFound("Bölüm bulunamadı.");

            bolum.Ad = dto.Ad;
            bolum.Kod = dto.Kod;
            bolum.Aciklama = dto.Aciklama;
            bolum.Aktif = dto.Aktif;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ====================================================
        // GET /api/Bolum/{bolumId}/Masalar
        // AMAÇ:
        // - Seçilen bölüme bağlı AKTİF masaları listeler
        // - Garson / Kasa ekranı için
        // ====================================================
        [HttpGet("{bolumId}/Masalar")]
        public async Task<IActionResult> GetMasalar(int bolumId)
        {
            var masalar = await _db.Masalar
                .Where(m => m.BolumId == bolumId && m.Aktif)
                .Select(m => new
                {
                    m.Id,
                    m.Ad,
                    m.Kod
                })
                .ToListAsync();

            return Ok(masalar);
        }

        // ====================================================
        // GET /api/Bolum/{bolumId}/Kategoriler
        // AMAÇ:
        // - Bu bölümde GÖRÜNTÜLENECEK kategorileri getirir
        // - BolumKategori üzerinden yetkilendirme yapılır
        // - Menü ağacının temelidir
        // ====================================================
        [HttpGet("{bolumId}/Kategoriler")]
        public async Task<IActionResult> GetYetkiliKategoriler(int bolumId)
        {
            var kategoriIds = await _db.BolumKategori
                .Where(bk => bk.BolumId == bolumId)
                .Select(bk => bk.KategoriId)
                .ToListAsync();

            var kategoriler = await _db.Kategoriler
                .Where(k => kategoriIds.Contains(k.Id) && k.Aktif)
                .Select(k => new
                {
                    k.Id,
                    k.Ad,
                    k.UstId,
                    k.SiraNo
                })
                .ToListAsync();

            return Ok(kategoriler);
        }

        // ===================== DTO =========================

        public class CreateBolumDto
        {
            public string Ad { get; set; } = null!;
            public string? Kod { get; set; }
            public string? Aciklama { get; set; }
            public bool Aktif { get; set; } = true;
            public int? Ekleyen { get; set; }
        }

        public class UpdateBolumDto
        {
            public string Ad { get; set; } = null!;
            public string? Kod { get; set; }
            public string? Aciklama { get; set; }
            public bool Aktif { get; set; }
        }
    }
}
