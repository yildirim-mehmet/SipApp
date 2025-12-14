using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Urun")]
    public class Urun
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        public string Ad { get; set; } = null!;

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("fiyat")]
        public decimal Fiyat { get; set; }

        [Column("hazirlamaSuresiDakika")]
        public int? HazirlamaSuresiDakika { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("ekleyen")]
        public int? Ekleyen { get; set; }

        [Column("tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;

        // 🔗 ÜRÜN → KATEGORİ (N:N)
        public ICollection<UrunKategori> UrunKategoriler { get; set; }
            = new List<UrunKategori>();

        // 🔗 Ürün → AdisyonKalem
        public ICollection<AdisyonKalem> AdisyonKalemler { get; set; }
            = new List<AdisyonKalem>();
    }
}
