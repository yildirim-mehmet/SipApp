using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;
using Resta.API.Entities;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class OdemeController : ControllerBase
    {
        private readonly RestaContext _db;

        public OdemeController(RestaContext db)
        {
            _db = db;
        }

        // ====================================================
        // POST /api/Odeme
        // AMAÇ:
        // - Adisyon için ödeme alır
        // - Adisyonu kapatır
        // - Masa boş duruma düşer
        //
        // NOT:
        // - Online ödeme entegrasyonu YOK (şimdilik)
        // - SignalR push daha sonra bağlanacak
        // ====================================================
        [HttpPost]
        public async Task<IActionResult> OdemeAl([FromBody] OdemeDto dto)
        {
            var adisyon = await _db.Adisyonlar
                .Include(a => a.Kalemler)
                .FirstOrDefaultAsync(a => a.Id == dto.AdisyonId);

            if (adisyon == null)
                return NotFound("Adisyon bulunamadı.");

            if (adisyon.Durum != (int)AdisyonDurum.Acik)
                return BadRequest("Bu adisyon zaten kapalı.");

            // ------------------------------------
            // Toplam tutarı hesapla
            // ------------------------------------
            decimal toplam = adisyon.Kalemler
    .Where(k => k.SiparisDurumu != 2)
    .Sum(k => k.AraToplam);
    //.Sum(k => k.AraToplam ?? 0m);


            // ------------------------------------
            // Ödeme kaydı oluştur
            // ------------------------------------
            var odeme = new Odeme
            {
                AdisyonId = adisyon.Id,
                Tutar = toplam,
                Tip = dto.Tip,           // masada / online
                Durum = 1,               // başarılı
                Tarih = DateTime.Now
            };

            _db.Odemeler.Add(odeme);

            // ------------------------------------
            // Adisyonu kapat
            // ------------------------------------
            adisyon.ToplamTutar = toplam;
            adisyon.Durum = (int)AdisyonDurum.Odenmis;
            adisyon.KapanisZamani = DateTime.Now;

            await _db.SaveChangesAsync();

            // ------------------------------------
            // 🔔 SIGNALR (SONRA BAĞLANACAK)
            // ------------------------------------
            // await _hub.Clients.Group($"MASA_{adisyon.MasaId}")
            //     .SendAsync("AdisyonKapandi", adisyon.Id);

            // await _hub.Clients.Group("KASA")
            //     .SendAsync("OdemeAlindi", adisyon.Id);

            return Ok(new
            {
                adisyon.Id,
                toplam,
                mesaj = "Ödeme alındı, adisyon kapatıldı."
            });
        }

        // ====================================================
        // DTO
        // ====================================================
        public class OdemeDto
        {
            public int AdisyonId { get; set; }
            public string Tip { get; set; } = "masada"; // masada / online
        }
    }
}




//✅ Bonus(tavsiye): Entity tarafını DB ile hizala

//SQL’de araToplam decimal(10,2) NOT NULL ve birimFiyat decimal NOT NULL.

//Eğer entity’lerinde bunlar decimal? ise, ileride benzer şeyler sürekli çıkar.

//AdisyonKalem entity içinde şu ikisini nullable değil yapman daha doğru olur:

//public decimal BirimFiyat { get; set; }
//public decimal AraToplam { get; set; }


//Ama bunu şimdi yapmak istemezsen, yukarıdaki ?? 0m yamasıyla sistem yine çalışır.

//Sonraki adım

//Bu iki fix’i yapınca OdemeController derlenecek.