using Microsoft.AspNetCore.Mvc;
using Resta.API.Data;
using Resta.API.Entities;

[ApiController]
[Route("api/[controller]")]
public class KullaniciController : ControllerBase
{
    private readonly RestaContext _db;
    public KullaniciController(RestaContext db) { _db = db; }

    [HttpPost]
    public async Task<IActionResult> Create(CreateKullaniciDto dto)
    {
        var user = new Kullanici
        {
            Ad = dto.Ad,
            KullaniciAdi = dto.KullaniciAdi,
            Sifre = dto.Sifre,
            RolId = dto.RolId,
            Aktif = true,
            Tarih = DateTime.Now
        };

        _db.Kullanicilar.Add(user);
        await _db.SaveChangesAsync();

        return Ok();
    }

    public class CreateKullaniciDto
    {
        public string Ad { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public int RolId { get; set; }
    }
}
