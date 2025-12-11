using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Urun")]
    public class Urun
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("kategoriId")]
        public int KategoriId { get; set; }

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

        // 🔗 İLİŞKİLER

        // Urun → Kategori (N:1)
        public Kategori Kategori { get; set; } = null!;

        // Urun → AdisyonKalem (1:N)
        public ICollection<AdisyonKalem> AdisyonKalemler { get; set; } = new List<AdisyonKalem>();
    }
}


//namespace Resta.API.Entities
//{
//    public class Urun
//    {
//        public int Id { get; set; }
//        public int KategoriId { get; set; }
//        public string Ad { get; set; } = null!;
//        public string? Aciklama { get; set; }
//        public decimal Fiyat { get; set; }
//        public int? HazirlamaSuresiDakika { get; set; }
//        public bool Aktif { get; set; } = true;
//        public int? Ekleyen { get; set; }
//        public DateTime Tarih { get; set; } = DateTime.Now;

//        public Kategori Kategori { get; set; } = null!;
//        public ICollection<AdisyonKalem> AdisyonKalemler { get; set; } = new List<AdisyonKalem>();
//    }
//}
