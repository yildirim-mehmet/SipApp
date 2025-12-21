using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.DTOs.Menu;
using Resta.API.Entities;


namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly RestaContext _db;

        public MenuController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // GET /api/Menu/masa/{masaId}
        // AMAÇ:
        // - QR okutulduğunda müşteri için MENÜYÜ döner
        // - SADECE OKUMA: Adisyon YOK, Sipariş YOK, DB yazımı YOK
        //
        // AKIŞ:
        // 1) masaId -> Masa bulunur
        // 2) Masa -> BolumId bulunur
        // 3) BolumKategori -> bu bölümün görmesi gereken kategoriId listesi alınır
        // 4) Kategori ağacı (UstId hiyerarşisi) recursive çıkarılır
        // 5) Her kategori altında: alt kategoriler + ürünler döner (UrunKategori üzerinden)
        // ====================================================
        //[HttpGet("masa/{masaId}")]
        [HttpGet("masa/{masaId}")]
        public async Task<IActionResult> GetMenuByMasa(int masaId)
        {
            var masa = await _db.Masalar
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == masaId && m.Aktif);

            if (masa == null)
                return NotFound("Masa bulunamadı.");

            var yetkiliKategoriIds = await _db.BolumKategori
                .AsNoTracking()
                .Where(x => x.BolumId == masa.BolumId)
                .Select(x => x.KategoriId)
                .ToListAsync();

            // ⚠️ KRİTİK: HER ZAMAN JSON ARRAY DÖN
            if (yetkiliKategoriIds.Count == 0)
                return Ok(new List<MenuKategoriDto>());

            var kategoriler = await _db.Kategoriler
                .AsNoTracking()
                .Where(k => k.Aktif && yetkiliKategoriIds.Contains(k.Id))
                .Select(k => new MenuKategoriDto
                {
                    Id = k.Id,
                    UstId = k.UstId,
                    Ad = k.Ad,
                    Renk = k.Renk,
                    SiraNo = k.SiraNo
                })
                .ToListAsync();

            var urunlerRaw = await _db.UrunKategori
                .AsNoTracking()
                .Where(uk =>
                    yetkiliKategoriIds.Contains(uk.KategoriId) &&
                    uk.Urun.Aktif)
                .Select(uk => new
                {
                    uk.KategoriId,
                    Urun = new MenuUrunDto
                    {
                        Id = uk.Urun.Id,
                        Ad = uk.Urun.Ad,
                        Aciklama = uk.Urun.Aciklama,
                        Fiyat = uk.Urun.Fiyat,
                        HazirlamaSuresiDakika = uk.Urun.HazirlamaSuresiDakika
                    }
                })
                .ToListAsync();

            var urunlerByKategori = urunlerRaw
                .GroupBy(x => x.KategoriId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Urun).Distinct().ToList()
                );

            var rootKategoriler = kategoriler
                .Where(k => k.UstId == null)
                .OrderBy(k => k.SiraNo ?? int.MaxValue)
                .ThenBy(k => k.Ad)
                .Select(k => BuildKategoriNode(k.Id, kategoriler, urunlerByKategori))
                .ToList();

            // ✅ ARTIK TESTLER BURADAN GEÇECEK
            return Ok(rootKategoriler);
        }


        // ----------------------------------------------------
        // Recursive kategori node builder
        // ----------------------------------------------------

        private MenuKategoriDto BuildKategoriNode(
    int kategoriId,
    List<MenuKategoriDto> kategoriler,
    Dictionary<int, List<MenuUrunDto>> urunlerByKategori)
        {
            var kategori = kategoriler.First(k => k.Id == kategoriId);

            var altKategoriler = kategoriler
                .Where(k => k.UstId == kategoriId)
                .OrderBy(k => k.SiraNo ?? int.MaxValue)
                .ThenBy(k => k.Ad)
                .Select(k => BuildKategoriNode(k.Id, kategoriler, urunlerByKategori))
                .ToList();

            urunlerByKategori.TryGetValue(kategoriId, out var urunler);
            urunler ??= new List<MenuUrunDto>();

            return new MenuKategoriDto
            {
                Id = kategori.Id,
                UstId = kategori.UstId,
                Ad = kategori.Ad,
                Renk = kategori.Renk,
                SiraNo = kategori.SiraNo,
                AltKategoriler = altKategoriler,
                Urunler = urunler
            };
        }




        [HttpGet("aktif-adisyon/{masaId}")]
        public async Task<IActionResult> AktifAdisyonDetay(int masaId)
        {
            var adisyon = await _db.Adisyonlar
                .Where(a => a.MasaId == masaId && a.Durum == (int)AdisyonDurum.Acik)
                .SelectMany(a => a.Kalemler)
                .Select(k => new {
                    urunAdi = k.Urun.Ad,
                    adet = k.Adet,
                    siparisDurumu = k.SiparisDurumu
                })
                .ToListAsync();

            return Ok(adisyon);
        }

    }
}
