namespace Platforma_pentru_tranzactii_auto.Models
{
    public class ClusterInfo
    {
        public int ClusterId { get; set; }
        public string NumeCluster { get; set; }
        public string Descriere { get; set; }
        public string CuloareBadge { get; set; }
        public string Emoji { get; set; }

        // Valorile centroidului — citite din appsettings.json
        public double Kilometraj { get; set; }
        public double CapacitateMotor { get; set; }
        public double PretEUR { get; set; }
        public double An { get; set; }
        public double Putere { get; set; }
    }

    public class ClusteringSettings
    {
        public List<ClusterInfo> Centroizi { get; set; }
        public double[] MinValues { get; set; }
        public double[] MaxValues { get; set; }
    }
}