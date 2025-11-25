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
    public DbSet<ImagineMasina>? Imagine { get; set; }
    public DbSet<Favorite>? Favorite { get; set; }
    public DbSet<Comentarii>? Commentarii { get; set; }
    public DbSet<Utilizator>? Utilizatori { get; set; }


}
