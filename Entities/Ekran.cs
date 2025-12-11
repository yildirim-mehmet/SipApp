using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Ekran")]
    public class Ekran
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        public string Ad { get; set; } = null!;

        [Column("tip")]
        public string Tip { get; set; } = null!;

        [Column("ipAdresi")]
        public string? IpAdresi { get; set; }

        [Column("baglantiKodu")]
        public string? BaglantiKodu { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // 🔗 İlişkiler
        // Ekran → EkranKategori (1:N)
        public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
    }
}


//namespace Resta.API.Entities
//{
//    public class Ekran
//    {
//        public int Id { get; set; }
//        public string Ad { get; set; } = null!;
//        public string Tip { get; set; } = null!;
//        public string? IpAdresi { get; set; }
//        public string? BaglantiKodu { get; set; }
//        public bool Aktif { get; set; } = true;
//        public DateTime Tarih { get; set; } = DateTime.Now;

//        public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
//    }
//}
