using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class Anunturi
    {
        [Key]
        public int ID_Anunt { get; set; }

        [Required]
        public string Marca { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Prețul trebuie să fie mai mare decât 0.")]
        public int Pret { get; set; }

        [Range(1900, 2025, ErrorMessage = "Anul trebuie să fie între 1900 și 2025.")]
        public int An_Fabricatie { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Kilometrajul nu poate fi negativ.")]
        public int Kilometraj { get; set; }

        public string Descriere { get; set; }

        public DateTime Data_Postarii { get; set; } = DateTime.Now;
        public int Nr_Vizualizari { get; set; }
        public string Locatie { get; set; }

        // --- PROPRIETATI NOI PENTRU MACHINE LEARNING ---
        public string? Combustibil { get; set; } // Benzina, Diesel, Electric, Hibrid
        public string? Transmisie { get; set; }  // Manuala, Automata

        [Range(0, int.MaxValue, ErrorMessage = "Capacitatea motorului nu poate fi negativă.")]
        public int CapacitateMotor { get; set; } // Ex: 1998 (in cm3)

        [Range(0, int.MaxValue, ErrorMessage = "Puterea nu poate fi negativă.")]
        public int PutereCP { get; set; }        // Ex: 150 (Cai putere)
        public string? TipCaroserie { get; set; } // Sedan, SUV, Hatchback, Break

        public byte[]? Imagine_Anunt { get; set; }
        // Calea catre videoclipul salvat fizic pe server ---
        public string? VideoPath { get; set; }
        public List<ImaginiAnunt>? GalerieImagini { get; set; }

        // FK catre utilizator (proprietar)
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Utilizator? User { get; set; }

        // Relații multiple
        public ICollection<Comentarii>? Comentari { get; set; }
        public ICollection<Favorite>? Favorite { get; set; }

    }
}
