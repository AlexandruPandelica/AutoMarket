namespace Platforma_pentru_tranzactii_auto.Models
{
    public class ScorRecomandare
    {
        public Anunturi Anunt { get; set; }
        public double ScorCosine { get; set; }
        public double ScorEuclidian { get; set; }
    }
}
