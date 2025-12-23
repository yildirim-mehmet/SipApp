namespace Resta.API.DTOs.Muzik
{
    public class SarkiDto
    {
        public int id { get; set; }
        public string ad { get; set; } = null!;
        public string dosyaYolu { get; set; } = null!;
        public int sure { get; set; }
        public bool aktif { get; set; }
        public DateTime eklenmeTarihi { get; set; }

        // Hesaplanmış property'ler
        public string sureFormatted => TimeSpan.FromSeconds(sure).ToString(@"mm\:ss");
        public string durum => aktif ? "Aktif" : "Pasif";
    }
}