using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Kullanici")]
    public class Kullanici
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        public string Ad { get; set; } = null!;

        [Column("sifreHash")]
        public string SifreHash { get; set; } = null!;

        [Column("rolId")]
        public int RolId { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;

        // 🔗 Kullanıcı → Rol (N:1)
        public Rol Rol { get; set; } = null!;
    }
}


//namespace Resta.API.Entities
//{
//    public class Kullanici
//    {
//        public int Id { get; set; }
//        public string AdSoyad { get; set; } = null!;
//        public string KullaniciAdi { get; set; } = null!;
//        public string SifreHash { get; set; } = null!;
//        public int RolId { get; set; }
//        public bool Aktif { get; set; }
//        public DateTime Tarih { get; set; }

//        public Rol Rol { get; set; } = null!;
//    }
//}
