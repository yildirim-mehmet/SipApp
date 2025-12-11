using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Rol")]
    public class Rol
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        public string Ad { get; set; } = null!;

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        // 🔗 Rol → Kullanıcılar (1:N)
        public ICollection<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
    }
}
