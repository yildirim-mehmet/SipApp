using Microsoft.EntityFrameworkCore;
using Resta.API.Entities;

namespace Resta.API.Data
{
    public class RestaContext : DbContext
    {
        public RestaContext(DbContextOptions<RestaContext> options) : base(options) { }

        public DbSet<Bolum> Bolumler => Set<Bolum>();
        public DbSet<Masa> Masalar => Set<Masa>();
        public DbSet<Kategori> Kategoriler => Set<Kategori>();
        public DbSet<Urun> Urunler => Set<Urun>();
        public DbSet<Ekran> Ekranlar => Set<Ekran>();
        public DbSet<EkranKategori> EkranKategoriler => Set<EkranKategori>();
        public DbSet<Adisyon> Adisyonlar => Set<Adisyon>();
        public DbSet<AdisyonKalem> AdisyonKalemler => Set<AdisyonKalem>();
        public DbSet<Ayarlar> Ayarlar => Set<Ayarlar>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<Bolum>().ToTable("Bolum");
            model.Entity<Masa>().ToTable("Masa");
            model.Entity<Kategori>().ToTable("Kategori");
            model.Entity<Urun>().ToTable("Urun");
            model.Entity<Ekran>().ToTable("Ekran");
            model.Entity<EkranKategori>().ToTable("EkranKategori");
            model.Entity<Adisyon>().ToTable("Adisyon");
            model.Entity<AdisyonKalem>().ToTable("AdisyonKalem");
            model.Entity<Ayarlar>().ToTable("Ayarlar");

            model.Entity<Kategori>()
                .HasOne(k => k.UstKategori)
                .WithMany(k => k!.AltKategoriler)
                .HasForeignKey(k => k.UstId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Masa>()
                .HasOne(m => m.Bolum)
                .WithMany(b => b.Masalar)
                .HasForeignKey(m => m.BolumId);

            model.Entity<EkranKategori>()
                .HasKey(e => new { e.EkranId, e.KategoriId });

            model.Entity<EkranKategori>()
                .HasOne(e => e.Ekran)
                .WithMany(x => x.EkranKategoriler)
                .HasForeignKey(e => e.EkranId);

            model.Entity<EkranKategori>()
                .HasOne(e => e.Kategori)
                .WithMany(k => k.EkranKategoriler)
                .HasForeignKey(e => e.KategoriId);

            model.Entity<Adisyon>()
                .HasOne(a => a.Masa)
                .WithMany(m => m.Adisyonlar)
                .HasForeignKey(a => a.MasaId);

            model.Entity<AdisyonKalem>()
                .HasOne(k => k.Adisyon)
                .WithMany(a => a.Kalemler)
                .HasForeignKey(k => k.AdisyonId);

            model.Entity<AdisyonKalem>()
                .HasOne(k => k.Urun)
                .WithMany(u => u.AdisyonKalemler)
                .HasForeignKey(k => k.UrunId);
        }
    }
}
