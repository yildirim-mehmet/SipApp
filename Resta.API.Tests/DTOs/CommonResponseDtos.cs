namespace Resta.API.Tests.DTOs;

// API'den { id: 5 } gibi dönen response
public class IdResponseDto
{
    public int Id { get; set; }
}

// POST /api/Siparis/ver dönüşü
public class SiparisVerResponseDto
{
    public int AdisyonId { get; set; }
}

// GET /api/Masa/{id}/durum dönüşü
public class MasaDurumDto
{
    public string Durum { get; set; } = null!;
}



public class AdisyonMiniDto
{
    public int Id { get; set; }
    public int MasaId { get; set; }
    public int Durum { get; set; }
}