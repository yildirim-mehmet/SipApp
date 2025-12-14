using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("UrunKategori")]
    public class UrunKategori
    {
        [Column("UrunId")]
        public int UrunId { get; set; }

        [Column("kategoriId")]
        public int KategoriId { get; set; }

        // 🔗 Navigation
        public Urun Urun { get; set; } = null!;
        public Kategori Kategori { get; set; } = null!;
    }
}
