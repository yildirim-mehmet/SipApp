using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("Bolum")]
    public class Bolum
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ad")]
        public string Ad { get; set; } = null!;


        [Column("kod")]
        public string? Kod { get; set; } = null!;



        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("ekleyen")]
        public int? Ekleyen { get; set; }

        [Column("tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;

        public ICollection<Masa> Masalar { get; set; } = new List<Masa>();
        public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();
    }
}


//namespace Resta.API.Entities
//{
//    public class Bolum
//    {
//        public int Id { get; set; }
//        public string Ad { get; set; } = null!;
//        public string? Aciklama { get; set; }
//        public bool Aktif { get; set; } = true;
//        public int? Ekleyen { get; set; }
//        public DateTime Tarih { get; set; } = DateTime.Now;

//        public ICollection<Masa> Masalar { get; set; } = new List<Masa>();
//        public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();
//    }
//}
