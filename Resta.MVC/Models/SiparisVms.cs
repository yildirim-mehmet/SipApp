namespace Resta.MVC.Models;

// MVC'den JS ile gönderilecek istek
public class SiparisVerRequestVm
{
    public int MasaId { get; set; }
    public List<SiparisUrunVm> Urunler { get; set; } = new();
}

public class SiparisUrunVm
{
    public int UrunId { get; set; }
    public int Adet { get; set; }
}

// API /api/Siparis/ver response
public class SiparisVerResponseVm
{
    public int AdisyonId { get; set; }
}
