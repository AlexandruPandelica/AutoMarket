using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using Platforma_pentru_tranzactii_auto.Models;

public class Utilizator : IdentityUser<int>
{
    public string Nume { get; set; }
    public string Prenume { get; set; }
    public string Telefon { get; set; }
    public string Adresa { get; set; }
    public byte[]? Imagine_Profil { get; set; }

    // Relații
    public ICollection<Anunturi>? Anunturi { get; set; }
    public ICollection<Favorite>? Favorite { get; set; }
    public ICollection<Comentarii>? Comentari { get; set; }
}
