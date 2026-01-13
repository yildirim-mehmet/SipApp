using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("SarkiListesi")]
    public class SarkiListesi
    {
        [Column("id")]
        public int id { get; set; }

        [Column("ad")]
        public string ad { get; set; } = null!;

        [Column("dosyaYolu")]
        public string dosyaYolu { get; set; } = null!;

        [Column("sure")]
        public int sure { get; set; }

        [Column("aktif")]
        public bool aktif { get; set; } = true;

        [Column("eklenmeTarihi")]
        public DateTime eklenmeTarihi { get; set; } = DateTime.UtcNow;

        

        // Navigation properties
        public virtual ICollection<CalmaListesi> CalmaListeleri { get; set; } = new List<CalmaListesi>();
    }
}