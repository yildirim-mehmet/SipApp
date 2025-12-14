using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    public enum AdisyonDurum
    {
        Acik = 0,
        Odenmis = 1,
        Iptal = 2
    }

    [Table("Adisyon")]
    public class Adisyon
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("masaId")]
        public int MasaId { get; set; }

        [Column("durum")]
        public int? Durum { get; set; }   // 0 = Açık, 1 = Ödenmiş, 2 = İptal

        [Column("kisiSayisi")]
        public int? KisiSayisi { get; set; }

        [Column("toplamTutar")]
        public decimal? ToplamTutar { get; set; }

        [Column("odemeSekli")]
        public string? OdemeSekli { get; set; }

        [Column("acilisZamani")]
        public DateTime? AcilisZamani { get; set; }

        [Column("kapanisZamani")]
        public DateTime? KapanisZamani { get; set; }

        [Column("ekleyen")]
        public int? Ekleyen { get; set; }

        // 🔗 Navigation — ilişkiler

        // Adisyon → Masa (N:1)
        public Masa Masa { get; set; } = null!;

        // Adisyon → AdisyonKalemler (1:N)
        public ICollection<AdisyonKalem> Kalemler { get; set; } = new List<AdisyonKalem>();
    }
}
