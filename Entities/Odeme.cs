namespace Resta.API.Entities
{
    public class Odeme
    {
        public int Id { get; set; }
        public int AdisyonId { get; set; }
        public decimal Tutar { get; set; }
        public string Tip { get; set; } = null!; // MASA / ONLINE
        public string? OdemeSaglayiciRef { get; set; }
        public int Durum { get; set; } // 0=Beklemede,1=Başarılı,2=Hatalı
        public DateTime Tarih { get; set; }

        public Adisyon Adisyon { get; set; } = null!;
    }
}
