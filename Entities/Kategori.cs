using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Kategori")]
    public class Kategori
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ustId")]
        public int? UstId { get; set; }

        [Column("bolumId")]
        public int? BolumId { get; set; }

        [Column("ad")]
        public string? Ad { get; set; }

        [Column("renk")]
        public string? Renk { get; set; }

        [Column("ekleyen")]
        public int? Ekleyen { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("tarih")]
        public DateTime? Tarih { get; set; } = DateTime.Now;

        // 🔗 SELF-REFERENCE: Üst Kategori
        public Kategori? UstKategori { get; set; }

        // 🔗 SELF-REFERENCE: Alt Kategoriler
        public ICollection<Kategori> AltKategoriler { get; set; } = new List<Kategori>();

        // 🔗 Kategori → Bolum (N:1)
        public Bolum? Bolum { get; set; }

        // 🔗 Kategori → Urun (1:N)
        public ICollection<Urun> Urunler { get; set; } = new List<Urun>();

        // 🔗 Kategori → EkranKategori (1:N)
        public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
    }
}


//namespace Resta.API.Entities
//{
//    public class Kategori
//    {
//        public int Id { get; set; }
//        public int? UstId { get; set; }
//        public int? BolumId { get; set; }
//        public string Ad { get; set; } = null!;
//        public string? Renk { get; set; }
//        public int? SiraNo { get; set; }
//        public int? Ekleyen { get; set; }
//        public bool Aktif { get; set; } = true;
//        public DateTime Tarih { get; set; } = DateTime.Now;

//        public Kategori? UstKategori { get; set; }
//        public ICollection<Kategori> AltKategoriler { get; set; } = new List<Kategori>();
//        public Bolum? Bolum { get; set; }
//        public ICollection<Urun> Urunler { get; set; } = new List<Urun>();
//        public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
//    }
//}
