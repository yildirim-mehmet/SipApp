namespace Resta.MVC.Models;

// API /api/Menu/masa/{masaId} cevabını karşılar
public class MenuResponseVm
{
    public int MasaId { get; set; }
    public int BolumId { get; set; }
    public List<MenuKategoriVm> Kategoriler { get; set; } = new();
}

public class MenuKategoriVm
{
    public int Id { get; set; }
    public int? UstId { get; set; }
    public string? Ad { get; set; }
    public string? Renk { get; set; }
    public int? SiraNo { get; set; }

    public List<MenuKategoriVm> AltKategoriler { get; set; } = new();
    public List<MenuUrunVm> Urunler { get; set; } = new();
}

public class MenuUrunVm
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public decimal Fiyat { get; set; }
    public int? HazirlamaSuresiDakika { get; set; }
}
