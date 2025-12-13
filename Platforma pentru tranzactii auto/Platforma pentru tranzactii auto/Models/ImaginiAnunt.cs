using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class ImaginiAnunt
    {
        [Key]
        public int ID_Imagine { get; set; }

        public byte[] Imagine { get; set; } // Aici stocăm poza propriu-zisă

        // Legătura cu Anunțul (Foreign Key)
        public int ID_Anunt { get; set; }

        [ForeignKey("ID_Anunt")]
        public Anunturi Anunt { get; set; }
    }
}