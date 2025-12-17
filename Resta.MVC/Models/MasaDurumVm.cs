namespace Resta.MVC.Models;

// API /api/Masa/{id}/durum -> { "durum": "bos" | "dolu" }
public class MasaDurumVm
{
    public string Durum { get; set; } = "bos";
}
