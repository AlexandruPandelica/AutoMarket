using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class Favorite
    {
        [Key]
        public int ID_Favorite { get; set; }
        public int UserId { get; set; }
        public int ID_Anunt { get; set; }
        // FK către utilizator

        [ForeignKey("UserId")]
        public Utilizator User { get; set; }
        // FK către anunț

        [ForeignKey("ID_Anunt")]
        public Anunturi Anunt { get; set; }
    }
}
