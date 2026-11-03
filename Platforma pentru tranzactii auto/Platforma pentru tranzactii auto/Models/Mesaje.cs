using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class Mesaje
    {
        [Key]
        public int ID_Mesaj { get; set; }

        [Required(ErrorMessage = "Mesajul nu poate fi gol.")]
        public string Continut { get; set; }

        public DateTime DataTrimiterii { get; set; } = DateTime.UtcNow; 

        // Relatii

        // Cumparatorul
        public int ExpeditorId { get; set; }

        [ForeignKey("ExpeditorId")]
        public Utilizator Expeditor { get; set; }

        // Vanzatorul
        public int DestinatarId { get; set; }

        [ForeignKey("DestinatarId")]
        public Utilizator Destinatar { get; set; }

        public int ID_Anunt { get; set; }

        [ForeignKey("ID_Anunt")]
        public Anunturi Anunt { get; set; }
    }
}