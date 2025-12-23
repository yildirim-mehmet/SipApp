using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("CalmaListesi")]
    public class CalmaListesi
    {
        [Column("id")]
        public int id { get; set; }

        [Column("sarkiId")]
        public int sarkiId { get; set; }

        [Column("masaId")]
        public int masaId { get; set; }  // Mevcut Masa tablosuna referans

        [Column("siraDegeri")]
        public int siraDegeri { get; set; } = 1;

        [Column("calindi")]
        public bool calindi { get; set; } = false;

        [Column("eklenmeZamani")]
        public DateTime eklenmeZamani { get; set; } = DateTime.UtcNow;

        [Column("odemeMiktari", TypeName = "decimal(10,2)")]
        public decimal odemeMiktari { get; set; } = 0;

        // Navigation properties
        [ForeignKey("sarkiId")]
        public virtual SarkiListesi Sarki { get; set; } = null!;

        [ForeignKey("masaId")]
        public virtual Masa Masa { get; set; } = null!;  // Mevcut Masa entity'si

        public virtual ICollection<CalinmaGecmisi> CalinmaGecmisleri { get; set; } = new List<CalinmaGecmisi>();
    }
}