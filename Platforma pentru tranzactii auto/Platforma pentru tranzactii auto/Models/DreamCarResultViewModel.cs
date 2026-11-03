namespace Platforma_pentru_tranzactii_auto.Models
{
    public class DreamCarResultViewModel
    {
        public Anunturi Anunt { get; set; }
        public double ScorSimilaritate { get; set; }
        public double ScorEuclidian { get; set; }
        public string DealLabel { get; set; }    
        public string DealColor { get; set; }    
        public decimal MediaPretSimilare { get; set; }
        public decimal DiferentaPret { get; set; } 

        public int GlobalMinAn { get; set; }
        public int GlobalMaxAn { get; set; }
        public int GlobalMinKm { get; set; }
        public int GlobalMaxKm { get; set; }
        public int GlobalMinPutere { get; set; }
        public int GlobalMaxPutere { get; set; }
        public int GlobalMinCapacitate { get; set; }
        public int GlobalMaxCapacitate { get; set; }
        public decimal GlobalMinPret { get; set; }
        public decimal GlobalMaxPret { get; set; }
    }
}