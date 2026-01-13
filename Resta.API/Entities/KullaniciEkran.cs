using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("KullaniciEkran")]
    public class KullaniciEkran
    {
        [Column("id")]
        public int Id { get; set; }   // 🔴 DB’de IDENTITY var

        [Column("kullaniciId")]
        public int KullaniciId { get; set; }

        [Column("ekranId")]
        public int EkranId { get; set; }

        public Kullanici Kullanici { get; set; } = null!;
        public Ekran Ekran { get; set; } = null!;
    }
}
