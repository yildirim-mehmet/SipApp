using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Masa")]
    public class Masa
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("bolumId")]
        public int BolumId { get; set; }

        [Column("kod")]
        public string Kod { get; set; } = null!;

        [Column("ad")]
        public string? Ad { get; set; }

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("ekleyen")]
        public int? Ekleyen { get; set; }

        [Column("tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;

        // 🔗 İLİŞKİLER
        // Masa → Bolum (N:1)
        public Bolum Bolum { get; set; } = null!;

        // Masa → Adisyon (1:N)
        public ICollection<Adisyon> Adisyonlar { get; set; } = new List<Adisyon>();
    }
}


//namespace Resta.API.Entities
//{
//    public class Masa
//    {
//        public int Id { get; set; }
//        public int BolumId { get; set; }
//        public string Kod { get; set; } = null!;
//        public string? Ad { get; set; }
//        public string? Aciklama { get; set; }
//        public bool Aktif { get; set; } = true;
//        public int? Ekleyen { get; set; }
//        public DateTime Tarih { get; set; } = DateTime.Now;

//        public Bolum Bolum { get; set; } = null!;
//        public ICollection<Adisyon> Adisyonlar { get; set; } = new List<Adisyon>();
//    }
//}
