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

        //[Column("bolumId")]
        //public int? BolumId { get; set; }

        [Column("siraNo")]
        public int? SiraNo { get; set; }

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

        //// 🔗 Kategori → Bolum (N:1)
        //public Bolum? Bolum { get; set; }

        // 🔗 Kategori → Urun (1:N)
        public ICollection<Urun> Urunler { get; set; } = new List<Urun>();

        // 🔗 Kategori → EkranKategori (1:N)
        public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
    }
}


