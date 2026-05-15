namespace Platforma_pentru_tranzactii_auto.Models
{
    public class DreamCarComparisonViewModel
    {
        public List<DreamCarResultViewModel> RecomandariStil { get; set; } = new List<DreamCarResultViewModel>();
        public List<DreamCarResultViewModel> RecomandariPrecizie { get; set; } = new List<DreamCarResultViewModel>();
    }
}