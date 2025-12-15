namespace Resta.MVC.Models
{
    // API'den gelen Masa bilgisini temsil eder
    public class MasaDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Kod { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public bool Aktif { get; set; }
    }
}
