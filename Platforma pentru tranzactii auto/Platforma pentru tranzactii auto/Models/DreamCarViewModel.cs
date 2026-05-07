using System.ComponentModel.DataAnnotations;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class DreamCarViewModel
    {
        [Required(ErrorMessage = "Te rugăm să introduci bugetul maxim.")]
        [Range(1, 1000000, ErrorMessage = "Bugetul trebuie să fie mai mare de 0.")]
        [Display(Name = "Buget Maxim (€)")]
        public decimal PretDorit { get; set; }

        [Required(ErrorMessage = "Te rugăm să introduci anul minim.")]
        [Range(1900, 2100, ErrorMessage = "Introdu un an valid.")]
        [Display(Name = "Anul minim dorit")]
        public int AnMinim { get; set; }

        [Required(ErrorMessage = "Te rugăm să introduci kilometrajul maxim.")]
        [Range(0, 1000000, ErrorMessage = "Kilometrajul nu poate fi negativ.")]
        [Display(Name = "Kilometraj Maxim acceptat")]
        public int KilometrajMaxim { get; set; }

        [Required(ErrorMessage = "Capacitatea cilindrică este necesară.")]
        [Range(1, 10000, ErrorMessage = "Capacitatea motorului trebuie să fie o valoare pozitivă (ex: 2000).")]
        [Display(Name = "Capacitate Motor dorită (cm3) - ex: 2000")]
        public int CapacitateMotorDorita { get; set; }

        [Required(ErrorMessage = "Puterea este necesară.")]
        [Range(1, 2000, ErrorMessage = "Puterea trebuie să fie o valoare pozitivă.")]
        [Display(Name = "Putere dorită (Cai Putere) - ex: 150")]
        public int PutereCPDorita { get; set; }
    }
}