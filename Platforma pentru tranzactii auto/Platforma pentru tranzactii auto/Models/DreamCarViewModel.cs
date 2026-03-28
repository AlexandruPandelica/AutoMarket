using System.ComponentModel.DataAnnotations;

namespace Platforma_pentru_tranzactii_auto.Models
{
    public class DreamCarViewModel
    {
        [Required(ErrorMessage = "Te rugăm să introduci bugetul maxim.")]
        [Display(Name = "Buget Maxim (€)")]
        public decimal PretDorit { get; set; }

        [Required(ErrorMessage = "Te rugăm să introduci anul minim.")]
        [Display(Name = "Anul minim dorit")]
        public int AnMinim { get; set; }

        [Required(ErrorMessage = "Te rugăm să introduci kilometrajul maxim.")]
        [Display(Name = "Kilometraj Maxim acceptat")]
        public int KilometrajMaxim { get; set; }

        [Required(ErrorMessage = "Capacitatea cilindrică este necesară.")]
        [Display(Name = "Capacitate Motor dorită (cm3) - ex: 2000")]
        public int CapacitateMotorDorita { get; set; }

        [Required(ErrorMessage = "Puterea este necesară.")]
        [Display(Name = "Putere dorită (Cai Putere) - ex: 150")]
        public int PutereCPDorita { get; set; }
    }
}