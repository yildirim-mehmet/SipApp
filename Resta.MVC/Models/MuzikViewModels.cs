namespace Resta.MVC.Models
{
    public class SarkiViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string DosyaYolu { get; set; } = null!;
        public int Sure { get; set; }
        public bool Aktif { get; set; }
        public DateTime EklenmeTarihi { get; set; }

        // View için özel property'ler
        public string SureFormatted => TimeSpan.FromSeconds(Sure).ToString(@"mm\:ss");
        public string Durum => Aktif ? "Aktif" : "Pasif";
    }

    public class CalmaListesiViewModel
    {
        public int Id { get; set; }
        public int SarkiId { get; set; }
        public string SarkiAdi { get; set; } = null!;
        public string DosyaYolu { get; set; } = null!;
        public int Sure { get; set; }
        public int MasaId { get; set; }
        public string MasaAdi { get; set; } = null!;  // Mevcut Masa'dan
        public int SiraDegeri { get; set; }
        public bool Calindi { get; set; }
        public decimal OdemeMiktari { get; set; }
        public DateTime EklemeZamani { get; set; }

        // View için özel property'ler
        public string SureFormatted => TimeSpan.FromSeconds(Sure).ToString(@"mm\:ss");
        public string EklemeZamaniFormatted => EklemeZamani.ToLocalTime().ToString("HH:mm");
        public string Durum => Calindi ? "Çalındı" : "Bekliyor";
        public string OncelikDurumu => SiraDegeri > 1 ? $"Öncelikli ({SiraDegeri})" : "Normal";
        public bool OdemeVar => OdemeMiktari > 0;
    }

    public class SarkiEkleViewModel
    {
        public int SarkiId { get; set; }
        public int MasaId { get; set; }
    }

    public class OdemeliSarkiEkleViewModel : SarkiEkleViewModel
    {
        public int OdemeMiktari { get; set; }
        public string OdemeReferans { get; set; } = null!;
    }

    public class MuzikIstatistikViewModel
    {
        public int ToplamSarki { get; set; }
        public int AktifCalmaListesi { get; set; }
        public int TamamlananCalma { get; set; }
        public int BugunkuIstekler { get; set; }
        public decimal ToplamOdeme { get; set; }
        public EnCokEkleyenMasaViewModel? EnCokEkleyenMasa { get; set; }

        public string ToplamOdemeFormatted => ToplamOdeme.ToString("C");
    }

    public class EnCokEkleyenMasaViewModel
    {
        public int MasaId { get; set; }
        public string MasaAdi { get; set; } = null!;
        public int Sayi { get; set; }
    }
}