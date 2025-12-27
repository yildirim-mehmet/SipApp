using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

using Resta.API.Hubs;




namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiparisController : ControllerBase
    {
        private readonly RestaContext _db;

        private readonly IHubContext<SiparisHub> _hub;

        public SiparisController(
            RestaContext db,
            IHubContext<SiparisHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ====================================================
        // POST /api/Siparis/ver
        // AMAÇ:
        // - Client (QR) tarafında cache'te tutulan siparişleri
        //   DB'ye yazar
        // - Masada aktif adisyon yoksa otomatik açar
        // - AdisyonKalem kayıtlarını oluşturur
        // - SignalR tetiklemeye hazır noktadır
        //
        // KURAL:
        // - Sipariş verildiği ana kadar DB yazımı YOK
        // ====================================================
        [HttpPost("ver")]
        public async Task<IActionResult> SiparisVer([FromBody] SiparisVerDto dto)
        {
            // 1) Masa kontrolü
            var masa = await _db.Masalar
                .FirstOrDefaultAsync(m => m.Id == dto.MasaId && m.Aktif);

            if (masa == null)
                return NotFound("Masa bulunamadı.");

            // 2) Aktif adisyon var mı?
            var adisyon = await _db.Adisyonlar
                .FirstOrDefaultAsync(a =>
                    a.MasaId == dto.MasaId &&
                    a.Durum == (int)AdisyonDurum.Acik);

            // 3) Yoksa otomatik aç
            if (adisyon == null)
            {
                adisyon = new Adisyon
                {
                    MasaId = dto.MasaId,
                    Durum = (int)AdisyonDurum.Acik,
                    AcilisZamani = DateTime.Now,
                    ToplamTutar = 0
                };

                _db.Adisyonlar.Add(adisyon);
                await _db.SaveChangesAsync();
            }

            // 4) Sipariş kalemleri
            var kalemler = new List<AdisyonKalem>();

            foreach (var item in dto.Urunler)
            {
                var urun = await _db.Urunler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == item.UrunId && u.Aktif);

                if (urun == null)
                    return BadRequest($"Ürün bulunamadı: {item.UrunId}");

                var kalem = new AdisyonKalem
                {
                    AdisyonId = adisyon.Id,
                    UrunId = urun.Id,
                    Adet = item.Adet,
                    BirimFiyat = urun.Fiyat,
                    AraToplam = urun.Fiyat * item.Adet,
                    SiparisDurumu = null, // NULL = onay bekliyor
                    SiparisVerilmeZamani = DateTime.Now,
                    EkleyenCihazTipi = dto.CihazTipi,
                    EkleyenCihazToken = dto.CihazToken
                };

                kalemler.Add(kalem);
            }

            _db.AdisyonKalemler.AddRange(kalemler);
            await _db.SaveChangesAsync();

            // 5) SignalR tetikleme noktası (bir sonraki adım)
            // await _hub.Clients.Group("EKRAN_x").SendAsync("YeniSiparis", ...);
            // 🔔 MASA ARTIK DOLU → TÜM DİNLENEN CLIENTLARA BİLDİR
            // kalemin bağlı olduğu adisyonu bul
            //var adisyon = await _db.Adisyonlar
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(a => a.Id == kalem.AdisyonId);

            // ✅ 1) Masa dolu bildirimi (sende var)

            await _hub.Clients
            .Group("EKRAN_ALL") // şimdilik tüm ekranlar
            .SendAsync("YeniSiparis", new
            {
                masaId = adisyon.MasaId,
                adisyonId = adisyon.Id
            });


            if (adisyon != null)
            {
                await _hub.Clients
                    .Group($"MASA_{adisyon.MasaId}")
                    .SendAsync("MasaDurumDegisti", new
                    {
                        masaId = adisyon.MasaId,
                        durum = "dolu"
                    });
            }

            // ✅ 2) Bu sipariş hangi ekranlara düşmeli? (urun->kategori->EkranKategori)
            var urunIds = dto.Urunler.Select(x => x.UrunId).Distinct().ToList();

            var ekranIds = await _db.UrunKategori
                .Where(uk => urunIds.Contains(uk.UrunId))
                .Join(_db.EkranKategoriler,
                    uk => uk.KategoriId,
                    ek => ek.KategoriId,
                    (uk, ek) => ek.EkranId
                )
                .Distinct()
                .ToListAsync();

            // ✅ 3) Ekranlara “YeniSiparis” event’i gönder
            foreach (var ekranId in ekranIds)
            {
                await _hub.Clients
                    .Group($"EKRAN_{ekranId}")
                    .SendAsync("YeniSiparis", new
                    {
                        ekranId,
                        masaId = adisyon.MasaId,
                        adisyonId = adisyon.Id,
                        kalemSayisi = kalemler.Count
                    });
            }


            return Ok(new
            {
                adisyonId = adisyon.Id,
                eklenenKalemSayisi = kalemler.Count
            });
        }

        // ====================================================
        // PUT /api/Siparis/{id}/durum
        // AMAÇ:
        // - Sipariş kaleminin durumunu günceller
        //
        // DURUMLAR:
        // NULL -> onay bekliyor
        // 0    -> hazırlanıyor
        // 1    -> masada
        // 2    -> iptal (sadece yetkili)
        //
        // KURAL:
        // - Müşteri iptal edemez
        // ====================================================
        [HttpPut("{id}/durum")]
        public async Task<IActionResult> UpdateSiparisDurum(
            int id,
            [FromBody] UpdateSiparisDurumDto dto)
        {
            var kalem = await _db.AdisyonKalemler
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kalem == null)
                return NotFound("Sipariş kalemi bulunamadı.");

            // müşteri iptal edemez
            if (dto.YeniDurum == 2 && !dto.YetkiliMi)
                return BadRequest("Bu işlem için yetkiniz yok.");

            kalem.SiparisDurumu = dto.YeniDurum;

            if (dto.YeniDurum == 0)
                kalem.HazirlamaBaslangicZamani = DateTime.Now;

            if (dto.YeniDurum == 1)
                kalem.MasayaGelisZamani = DateTime.Now;

            if (dto.YeniDurum == 2)
            {
                kalem.IptalNedeni = dto.IptalNedeni;
                kalem.IptalEdenKullaniciId = dto.KullaniciId;
                kalem.IptalZamani = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            await _hub.Clients
            .All
            .SendAsync("SiparisDurumDegisti", new
            {
                kalemId = kalem.Id,
                yeniDurum = dto.YeniDurum
            });


            // ===============================
            // SIGNALR – MASA DOLU BİLDİRİMİ
            // ===============================
            // kalemin bağlı olduğu adisyonu bul
            var adisyon = await _db.Adisyonlar
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == kalem.AdisyonId);

            if (adisyon != null)
            {
                await _hub.Clients
                    .Group($"MASA_{adisyon.MasaId}")
                    .SendAsync("MasaDurumDegisti", new
                    {
                        masaId = adisyon.MasaId,
                        durum = "dolu"
                    });
            }


            // SignalR: durum değişti
            return Ok(kalem);
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






        // ====================================================
        // DTO'LAR
        // ====================================================

        public class SiparisVerDto
        {
            public int MasaId { get; set; }
            public List<SiparisUrunDto> Urunler { get; set; } = new();

            // client bilgisi (QR / Web / Tablet)
            public string? CihazTipi { get; set; }
            public string? CihazToken { get; set; }
        }

        public class SiparisUrunDto
        {
            public int UrunId { get; set; }
            public int Adet { get; set; }
        }

        public class UpdateSiparisDurumDto
        {
            public int YeniDurum { get; set; }
            public bool YetkiliMi { get; set; }

            // iptal için
            public int? KullaniciId { get; set; }
            public string? IptalNedeni { get; set; }
        }
    }
}
