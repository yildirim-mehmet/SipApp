namespace Resta.MVC.Models;

public class AdisyonYazdirVm
{
    public int Id { get; set; }
    public string MasaAdi { get; set; } = "";
    public DateTime Acilis { get; set; }
    public DateTime? Kapanis { get; set; }
    public decimal Toplam { get; set; }

    public List<AdisyonYazdirKalemVm> Kalemler { get; set; } = new();
}

public class AdisyonYazdirKalemVm
{
    public string Ad { get; set; } = "";
    public int Adet { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal AraToplam { get; set; }
}
