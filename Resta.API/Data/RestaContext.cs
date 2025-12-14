using System.Reflection.Emit;
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

        // sağlam yapı eklemeleri
        public DbSet<BolumKategori> BolumKategori => Set<BolumKategori>(); // yeni eklendi

        public DbSet<UrunKategori> UrunKategori => Set<UrunKategori>();
        public DbSet<Odeme> Odemeler => Set<Odeme>();





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

            model.Entity<Odeme>().ToTable("Odeme");

            //
            // 🚫 Kategori → Bolum ilişkisinin KESİN olarak olmadığını söyle
            //model.Entity<Kategori>().Ignore("BolumId");
            model.Entity<Kategori>().Ignore("Bolum");
            model.Entity<Kategori>().Ignore("BolumId");
            model.Entity<Kategori>().Ignore("BolumId1");


            //model.Entity<Kategori>().Ignore(k => k.Bolum);
            //model.Entity<Kategori>()
            //.HasOne(k => k.UstKategori)
            //.WithMany(k => k.AltKategoriler)
            //.HasForeignKey(k => k.UstId);

            // Kategori hiyerarşisi
            model.Entity<Kategori>()
                .HasOne(k => k.UstKategori)
                .WithMany(k => k!.AltKategoriler)
                .HasForeignKey(k => k.UstId)
                .OnDelete(DeleteBehavior.Restrict);

            // Masa -> Bolum
            model.Entity<Masa>()
                .HasOne(m => m.Bolum)
                .WithMany(b => b.Masalar)
                .HasForeignKey(m => m.BolumId);

            // EkranKategori
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

            // 🔴 YENİ: BolumKategori
            model.Entity<BolumKategori>()
                .ToTable("BolumKategori");

            model.Entity<BolumKategori>()
                .HasKey(x => new { x.BolumId, x.KategoriId });

            model.Entity<BolumKategori>()
                .HasOne(x => x.Bolum)
                .WithMany()
                .HasForeignKey(x => x.BolumId);

            model.Entity<BolumKategori>()
                .HasOne(x => x.Kategori)
                .WithMany()
                .HasForeignKey(x => x.KategoriId);

            // Adisyon
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

            // --------------------------------------------------
            // URUN ↔ KATEGORİ (N:N)
            // --------------------------------------------------
            model.Entity<UrunKategori>()
                .HasKey(x => new { x.UrunId, x.KategoriId });

            model.Entity<UrunKategori>()
                .HasOne(x => x.Urun)
                .WithMany(u => u.UrunKategoriler)
                .HasForeignKey(x => x.UrunId);

            model.Entity<UrunKategori>()
                .HasOne(x => x.Kategori)
                .WithMany()
                .HasForeignKey(x => x.KategoriId);

            


        }
    }
}
