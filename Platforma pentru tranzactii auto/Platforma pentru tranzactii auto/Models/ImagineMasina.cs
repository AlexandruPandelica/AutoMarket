
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class ImagineMasina
    {
        [Key]
        public int ID_ImagineMasina { get; set; }
        public string Cale_Imagine { get; set; }

        // FK către anunț
        public int ID_Anunt { get; set; }

        [ForeignKey("ID_Anunt")]
        public Anunturi Anunt { get; set; }
    }
}
