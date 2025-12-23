namespace Resta.API.DTOs.Muzik
{
    public class MuzikIstatistikDto
    {
        public int toplamSarki { get; set; }
        public int aktifCalmaListesi { get; set; }
        public int tamamlananCalma { get; set; }
        public int bugunkuIstekler { get; set; }
        public decimal toplamOdeme { get; set; }
        public EnCokEkleyenMasaDto? enCokEkleyenMasa { get; set; }

        public string toplamOdemeFormatted => toplamOdeme.ToString("C");
    }

    public class EnCokEkleyenMasaDto
    {
        public int masaId { get; set; }
        public string masaAdi { get; set; } = null!;
        public int sayi { get; set; }
    }
}