using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class Comentarii
    {
        [Key]
        public int ID_Comentariu { get; set; }

        [Required]
        public string Text_Comentariu { get; set; }

        [Range(1, 5)]
        public int Recenzie { get; set; }
        public DateTime DataComentariu { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public int ID_Anunt { get; set; }

        [ForeignKey("UserId")]
        public Utilizator User { get; set; }

        [ForeignKey("ID_Anunt")]
        public Anunturi Anunt { get; set; }

    }
}
