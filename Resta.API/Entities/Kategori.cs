using Resta.API.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Kategori")]
public class Kategori
{
    public int Id { get; set; }
    public int? UstId { get; set; }
    public int? SiraNo { get; set; }
    public string? Ad { get; set; }
    public string? Renk { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime? Tarih { get; set; }

    // Self-reference
    public Kategori? UstKategori { get; set; }
    public ICollection<Kategori> AltKategoriler { get; set; } = new List<Kategori>();

    // ❌ BOLUMLA ALAKALI TEK SATIR YOK

    public ICollection<Urun> Urunler { get; set; } = new List<Urun>();
    public ICollection<EkranKategori> EkranKategoriler { get; set; } = new List<EkranKategori>();
}
