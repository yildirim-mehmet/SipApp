using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resta.API.Data;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bolumler = await _db.Bolumler.ToListAsync(); // EF'nin ürettiği DbSet adı "Bolum" veya "Bolums" olabilir
            return Ok(bolumler);
        }
    }
}
