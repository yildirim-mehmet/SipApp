using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;
using Resta.API.Helpers;

namespace Resta.API.Controllers.Api
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

        // ----------------------------------------------------------
        // 1) MASA OLUŞTUR
        // ----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMasaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Kod))
                return BadRequest("Kod boş olamaz.");

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

            return Ok(new { masa.Id, masa.Kod, masa.Ad });
        }

        public class CreateMasaDto
        {
            public int BolumId { get; set; }
            public string Kod { get; set; } = null!;
            public string? Ad { get; set; }
            public string? Aciklama { get; set; }
        }

        // ----------------------------------------------------------
        // 2) MASA DETAYI
        // ----------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var masa = await _db.Masalar
                .Include(m => m.Bolum)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (masa == null) return NotFound();

            return Ok(masa);
        }

        // ----------------------------------------------------------
        // 3) QR PNG ÜRET (ImageSharp + QrHelper)
        // ----------------------------------------------------------
        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQr(int id)
        {
            var masa = await _db.Masalar.FirstOrDefaultAsync(m => m.Id == id);
            if (masa == null) return NotFound();

            // Ayarlar tablosundan QR base adresi (varsa)
            string? baseUrl = _db.Ayarlar.FirstOrDefault()?.QrBaseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = _config["Ayarlar:QrBaseUrl"] ?? "https://localhost:5001/masa/";

            string qrUrl = $"{baseUrl}{masa.Kod}";

            var pngBytes = QrHelper.GenerateQrPng(qrUrl);

            return File(pngBytes, "image/png");
        }

        // ----------------------------------------------------------
        // 4) QR'DAN MASA BUL (/api/masa/kod/XYZ)
        // ----------------------------------------------------------
        [HttpGet("kod/{kod}")]
        public async Task<IActionResult> GetByKod(string kod)
        {
            var masa = await _db.Masalar
                .Include(m => m.Bolum)
                .FirstOrDefaultAsync(m => m.Kod == kod);

            if (masa == null)
                return NotFound("Masa bulunamadı.");

            return Ok(masa);
        }

        // ----------------------------------------------------------
        // 5) MASA İÇİN AKTİF ADİSYONU GETİR VEYA OLUŞTUR
        // ----------------------------------------------------------
        [HttpGet("{masaId}/aktif-adisyon")]
        public async Task<IActionResult> GetOrCreateAdisyon(int masaId)
        {
            var masa = await _db.Masalar.FindAsync(masaId);
            if (masa == null) return NotFound("Masa yok.");

            var adisyon = await _db.Adisyonlar
                .Include(a => a.Kalemler)
                    .ThenInclude(k => k.Urun)
                .Where(a => a.MasaId == masaId && a.Durum == (int)AdisyonDurum.Acik)
                .FirstOrDefaultAsync();

            if (adisyon != null)
                return Ok(adisyon);

            // Yeni adisyon aç
            var yeni = new Adisyon
            {
                MasaId = masaId,
                Durum = (int)AdisyonDurum.Acik,
                AcilisZamani = DateTime.Now
            };

            _db.Adisyonlar.Add(yeni);
            await _db.SaveChangesAsync();

            return Ok(yeni);
        }

        // ----------------------------------------------------------
        // 6) MASA MENÜSÜ (Kategori + Ürün)
        // ----------------------------------------------------------
        [HttpGet("{masaId}/menu")]
        public async Task<IActionResult> GetMenu(int masaId)
        {
            var masa = await _db.Masalar
                .Include(m => m.Bolum)
                .FirstOrDefaultAsync(m => m.Id == masaId);

            if (masa == null) return NotFound("Masa bulunamadı.");

            int bolumId = masa.BolumId;

            // Bölümün kategorilerini al
            var kategoriler = await _db.Kategoriler
                .Where(k => k.BolumId == bolumId && k.Aktif)
                .Include(k => k.AltKategoriler)
                .Include(k => k.Urunler)
                .ToListAsync();

            // Root kategoriler
            var rootList = kategoriler
                .Where(k => k.UstId == null)
                .Select(k => MapKategori(k))
                .ToList();

            return Ok(rootList);
        }

        private object MapKategori(Kategori k)
        {
            return new
            {
                id = k.Id,
                ad = k.Ad,
                renk = k.Renk,
                alt = k.AltKategoriler.Select(a => MapKategori(a)),
                urunler = k.Urunler
                    .Where(u => u.Aktif)
                    .Select(u => new
                    {
                        id = u.Id,
                        ad = u.Ad,
                        fiyat = u.Fiyat,
                        aciklama = u.Aciklama
                    })
            };
        }
    }
}


//using Microsoft.AspNetCore.Mvc;

//namespace Resta.API.Controllers.API
//{
//    public class MasaController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}
