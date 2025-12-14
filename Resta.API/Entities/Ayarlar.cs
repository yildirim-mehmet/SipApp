using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Ayarlar")]
    public class Ayarlar
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("IsletmeAdi")]
        public string? IsletmeAdi { get; set; }

        [Column("logoPath")]
        public string? LogoPath { get; set; }

        [Column("SistemAdres")]
        public string? SistemAdres { get; set; }

        [Column("qrBaseUrl")]
        public string? QrBaseUrl { get; set; }

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;
    }
}


//namespace Resta.API.Entities
//{
//    public class Ayarlar
//    {
//        public int Id { get; set; }
//        public string? IsletmeAdi { get; set; }
//        public string? LogoPath { get; set; }
//        public string? SistemAdres { get; set; }
//        public string? QrBaseUrl { get; set; }
//        public DateTime Tarih { get; set; } = DateTime.Now;
//    }
//}
