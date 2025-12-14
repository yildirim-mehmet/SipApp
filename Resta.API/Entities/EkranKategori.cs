using System.ComponentModel.DataAnnotations.Schema;

namespace Resta.API.Entities
{
    [Table("EkranKategori")]
    public class EkranKategori
    {
        [Column("Eid")]
        public int EkranId { get; set; }

        [Column("Kid")]
        public int KategoriId { get; set; }

        // 🔗 Navigation Properties

        // Bir Ekran → bir EkranKategori ilişkisi
        public Ekran Ekran { get; set; } = null!;

        // Bir Kategori → bir EkranKategori ilişkisi
        public Kategori Kategori { get; set; } = null!;
    }
}


//namespace Resta.API.Entities
//{
//    public class EkranKategori
//    {
//        public int EkranId { get; set; }
//        public int KategoriId { get; set; }

//        public Ekran Ekran { get; set; } = null!;
//        public Kategori Kategori { get; set; } = null!;
//    }
//}
