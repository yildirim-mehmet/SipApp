using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("CalinmaGecmisi")]
    public class CalinmaGecmisi
    {
        [Column("id")]
        public int id { get; set; }

        [Column("calmaListesiId")]
        public int calmaListesiId { get; set; }

        [Column("masaId")]
        public int masaId { get; set; }  // Mevcut Masa tablosuna referans

        [Column("calinmaZamani")]
        public DateTime calinmaZamani { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("calmaListesiId")]
        public virtual CalmaListesi CalmaListesi { get; set; } = null!;

        [ForeignKey("masaId")]
        public virtual Masa Masa { get; set; } = null!;  // Mevcut Masa entity'si
    }
}