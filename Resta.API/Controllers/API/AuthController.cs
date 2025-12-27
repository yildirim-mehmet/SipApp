using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;

namespace Resta.API.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RestaContext _db;

        public AuthController(RestaContext db)
        {
            _db = db;
        }

        // POST /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Kullanicilar
                .Where(x =>
                    x.KullaniciAdi == dto.KullaniciAdi &&
                    x.Sifre == dto.Sifre &&
                    x.Aktif)
                .Select(x => new
                {
                    id = x.Id,
                    ad = x.Ad,
                    rolId = x.RolId   // 🔑 BURASI
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }

        public class LoginDto
        {
            public string KullaniciAdi { get; set; } = null!;
            public string Sifre { get; set; } = null!;
        }
    }
}
