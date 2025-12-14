namespace Resta.API.DTOs.Menu
{
    public class MenuUrunDto
    {
        public int Id { get; set; }
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
        public decimal Fiyat { get; set; }
        public int? HazirlamaSuresiDakika { get; set; }
    }
}
