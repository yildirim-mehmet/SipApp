using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    public enum SiparisDurumu
    {
        Hazirlaniyor = 0,
        Masada = 1,
        Iptal = 2
    }

    [Table("AdisyonKalem")]
    public class AdisyonKalem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("adisyonId")]
        public int AdisyonId { get; set; }

        [Column("urunId")]
        public int UrunId { get; set; }

        [Column("adet")]
        public int Adet { get; set; }

        [Column("birimFiyat")]
        public decimal? BirimFiyat { get; set; }

        [Column("araToplam")]
        public decimal? AraToplam { get; set; }

        [Column("siparisDurumu")]
        public int? SiparisDurumu { get; set; }

        [Column("siparisVerilmeZamani")]
        public DateTime? SiparisVerilmeZamani { get; set; }

        [Column("hazirlamaBaslangicZamani")]
        public DateTime? HazirlamaBaslangicZamani { get; set; }

        [Column("masayaGelisZamani")]
        public DateTime? MasayaGelisZamani { get; set; }

        [Column("iptalNedeni")]
        public string? IptalNedeni { get; set; }

        [Column("iptalEdenKullaniciId")]
        public int? IptalEdenKullaniciId { get; set; }

        [Column("iptalZamani")]
        public DateTime? IptalZamani { get; set; }

        [Column("ekleyenCihazTipi")]
        public string? EkleyenCihazTipi { get; set; }

        [Column("ekleyenCihazToken")]
        public string? EkleyenCihazToken { get; set; }

        // 🔗 Navigation — ilişkiler

        // AdisyonKalem → Adisyon (N:1)
        public Adisyon Adisyon { get; set; } = null!;

        // AdisyonKalem → Urun (N:1)
        public Urun Urun { get; set; } = null!;
    }
}


//namespace Resta.API.Entities
//{
//    public enum SiparisDurumu
//    {
//        Hazirlaniyor = 0,
//        Masada = 1,
//        Iptal = 2
//    }

//    public class AdisyonKalem
//    {
//        public int Id { get; set; }
//        public int AdisyonId { get; set; }
//        public int UrunId { get; set; }
//        public int Adet { get; set; }
//        public decimal BirimFiyat { get; set; }
//        public decimal AraToplam { get; set; }
//        public SiparisDurumu? SiparisDurumu { get; set; }
//        public DateTime? SiparisVerilmeZamani { get; set; }
//        public DateTime? HazirlamaBaslangicZamani { get; set; }
//        public DateTime? MasayaGelisZamani { get; set; }
//        public string? IptalNedeni { get; set; }
//        public int? IptalEdenKullaniciId { get; set; }
//        public DateTime? IptalZamani { get; set; }
//        public string? EkleyenCihazTipi { get; set; }
//        public string? EkleyenCihazToken { get; set; }

//        public Adisyon Adisyon { get; set; } = null!;
//        public Urun Urun { get; set; } = null!;
//    }
//}
