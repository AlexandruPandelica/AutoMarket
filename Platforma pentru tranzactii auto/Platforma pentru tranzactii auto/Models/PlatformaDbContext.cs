using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml.Linq;

public class PlatformaDbContext : IdentityDbContext<Utilizator, IdentityRole<int>, int>
{
    public PlatformaDbContext(DbContextOptions<PlatformaDbContext> options)
        : base(options)
    {
    }

    // === DbSet-urile tale ===
    public DbSet<Anunturi>? Anunt { get; set; }
    public DbSet<Favorite>? Favorite { get; set; }
    public DbSet<Comentarii>? Comentarii { get; set; }
    public DbSet<Utilizator>? Utilizatori { get; set; }
    public DbSet<ImaginiAnunt> ImaginiAnunt { get; set; }

    // 🆕 TABELUL NOU
    public DbSet<Mesaje>? Mesaje { get; set; }

    // 🆕 CONFIGURARE SPECIALĂ (Foarte Important!)
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurare relație Expeditor -> Mesaje
        builder.Entity<Mesaje>()
            .HasOne(m => m.Expeditor)
            .WithMany()
            .HasForeignKey(m => m.ExpeditorId)
            .OnDelete(DeleteBehavior.Restrict); // Evităm ștergerea în cascadă

        // Configurare relație Destinatar -> Mesaje
        builder.Entity<Mesaje>()
            .HasOne(m => m.Destinatar)
            .WithMany()
            .HasForeignKey(m => m.DestinatarId)
            .OnDelete(DeleteBehavior.Restrict); // Evităm ștergerea în cascadă
    }

}
