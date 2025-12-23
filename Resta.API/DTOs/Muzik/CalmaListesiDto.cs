namespace Resta.API.DTOs.Muzik
{
    public class CalmaListesiDto
    {
        public int id { get; set; }
        public int sarkiId { get; set; }
        public string sarkiAdi { get; set; } = null!;
        public string dosyaYolu { get; set; } = null!;
        public int sure { get; set; }
        public int masaId { get; set; }
        public string masaAdi { get; set; } = null!;  // Masa adı için
        public int siraDegeri { get; set; }
        public bool calindi { get; set; }
        public decimal odemeMiktari { get; set; }
        public DateTime eklenmeZamani { get; set; }

        // Hesaplanmış property'ler
        public string sureFormatted => TimeSpan.FromSeconds(sure).ToString(@"mm\:ss");
        public string eklenmeZamaniFormatted => eklenmeZamani.ToLocalTime().ToString("HH:mm");
        public string durum => calindi ? "Çalındı" : "Bekliyor";
        public string oncelikDurumu => siraDegeri > 1 ? $"Öncelikli ({siraDegeri})" : "Normal";
    }
}