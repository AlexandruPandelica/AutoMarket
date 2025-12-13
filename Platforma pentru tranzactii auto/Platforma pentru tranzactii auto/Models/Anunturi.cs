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
        public int Pret { get; set; }
        public int An_Fabricatie { get; set; }
        public int Kilometraj { get; set; }
        public string Descriere { get; set; }
        public DateTime Data_Postarii { get; set; } = DateTime.Now;
        public int Nr_Vizualizari { get; set; }
        public string Locatie { get; set; }

        public byte[]? Imagine_Anunt { get; set; }
        public List<ImaginiAnunt>? GalerieImagini { get; set; }

        // FK către utilizator (proprietar)
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Utilizator? User { get; set; }

        // Relații multiple
        public ICollection<Comentarii>? Comentari { get; set; }
        public ICollection<Favorite>? Favorite { get; set; }

    }
}
