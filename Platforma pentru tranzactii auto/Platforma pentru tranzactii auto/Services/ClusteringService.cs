using Microsoft.Extensions.Options;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Services
{
    public class ClusteringService
    {
        private readonly ClusteringSettings _settings;

        public ClusteringService(IOptions<ClusteringSettings> settings)
        {
            _settings = settings.Value;
        }

        private double Normalize(double val, double min, double max)
        {
            if (max == min) return 0;
            double result = (val - min) / (max - min);
            return Math.Clamp(result, 0.0, 1.0);
        }

        private double DistantaEuclid(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }

        private double[] NormalizeazaVector(double[] vector)
        {
            return new double[]
            {
                Normalize(vector[0], _settings.MinValues[0], _settings.MaxValues[0]),
                Normalize(vector[1], _settings.MinValues[1], _settings.MaxValues[1]),
                Normalize(vector[2], _settings.MinValues[2], _settings.MaxValues[2]),
                Normalize(vector[3], _settings.MinValues[3], _settings.MaxValues[3]),
                Normalize(vector[4], _settings.MinValues[4], _settings.MaxValues[4]),
            };
        }

        public int AsigneazaCluster(Anunturi anunt)
        {
            double[] vectorAnunt = NormalizeazaVector(new double[]
            {
                anunt.Kilometraj,
                anunt.CapacitateMotor,
                (double)anunt.Pret,
                anunt.An_Fabricatie,
                anunt.PutereCP > 0 ? anunt.PutereCP : 200
            });

            int clusterOptim = 0;
            double distantaMin = double.MaxValue;

            foreach (var centroid in _settings.Centroizi)
            {
                double[] vectorCentroid = NormalizeazaVector(new double[]
                {
                    centroid.Kilometraj,
                    centroid.CapacitateMotor,
                    centroid.PretEUR,
                    centroid.An,
                    centroid.Putere
                });

                double d = DistantaEuclid(vectorAnunt, vectorCentroid);
                if (d < distantaMin)
                {
                    distantaMin = d;
                    clusterOptim = centroid.ClusterId;
                }
            }

            return clusterOptim;
        }

        public ClusterInfo GetClusterInfo(int clusterId)
        {
            return _settings.Centroizi.FirstOrDefault(c => c.ClusterId == clusterId)
                   ?? _settings.Centroizi[0];
        }

        public ClusterInfo GetClusterInfoPentruAnunt(Anunturi anunt)
        {
            int clusterId = AsigneazaCluster(anunt);
            return GetClusterInfo(clusterId);
        }

        public List<ClusterInfo> GetToateClustrele()
        {
            return _settings.Centroizi;
        }
    }
}