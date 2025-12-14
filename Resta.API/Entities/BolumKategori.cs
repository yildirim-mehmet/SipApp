using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("BolumKategori")]
    public class BolumKategori
    {
        public int BolumId { get; set; }
        public int KategoriId { get; set; }

        // Navigation (ileride kullanılacak)
        public Bolum Bolum { get; set; } = null!;
        public Kategori Kategori { get; set; } = null!;
    }
}
