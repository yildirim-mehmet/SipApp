namespace Resta.API.DTOs.Menu
{
    public class MenuKategoriDto
    {
        public int Id { get; set; }
        public int? UstId { get; set; }
        public string? Ad { get; set; }
        public string? Renk { get; set; }
        public int? SiraNo { get; set; }

        public List<MenuKategoriDto> AltKategoriler { get; set; } = new();
        public List<MenuUrunDto> Urunler { get; set; } = new();
    }
}
