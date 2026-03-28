using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Services
{
    public class RecomandareService
    {
        private readonly PlatformaDbContext _context;

        public RecomandareService(PlatformaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Anunturi>> CalculeazaDreamCar(DreamCarViewModel dreamCar, int topRecomandari = 3)
        {
            var toateAnunturile = await _context.Anunt.ToListAsync();

            if (!toateAnunturile.Any()) return new List<Anunturi>();

            // 1. Aflăm valorile Minime și Maxime din baza de date pentru NORMALIZARE
            decimal maxPret = toateAnunturile.Max(a => a.Pret) == 0 ? 1 : toateAnunturile.Max(a => a.Pret);
            decimal minPret = toateAnunturile.Min(a => a.Pret);

            int maxAn = toateAnunturile.Max(a => a.An_Fabricatie) == 0 ? 1 : toateAnunturile.Max(a => a.An_Fabricatie);
            int minAn = toateAnunturile.Min(a => a.An_Fabricatie);

            int maxKm = toateAnunturile.Max(a => a.Kilometraj) == 0 ? 1 : toateAnunturile.Max(a => a.Kilometraj);
            int minKm = toateAnunturile.Min(a => a.Kilometraj);

            int maxCapacitate = toateAnunturile.Max(a => a.CapacitateMotor) == 0 ? 1 : toateAnunturile.Max(a => a.CapacitateMotor);
            int minCapacitate = toateAnunturile.Min(a => a.CapacitateMotor);

            int maxPutere = toateAnunturile.Max(a => a.PutereCP) == 0 ? 1 : toateAnunturile.Max(a => a.PutereCP);
            int minPutere = toateAnunturile.Min(a => a.PutereCP);

            // 2. Creăm Vectorul Ideal al utilizatorului (Normalizat între 0 și 1)
            // Atenție: Pentru Preț și Kilometraj vrem valori cât mai Mici, deci logica e inversă (1 - valoare)
            double[] vectorUtilizator = {
                1.0 - Normalize(dreamCar.PretDorit, minPret, maxPret),
                Normalize(dreamCar.AnMinim, minAn, maxAn),
                1.0 - Normalize(dreamCar.KilometrajMaxim, minKm, maxKm),
                Normalize(dreamCar.CapacitateMotorDorita, minCapacitate, maxCapacitate),
                Normalize(dreamCar.PutereCPDorita, minPutere, maxPutere)
            };

            var recomandari = new Dictionary<Anunturi, double>();

            // 3. Calculăm similaritatea pentru fiecare mașină din DB
            foreach (var masina in toateAnunturile)
            {
                double[] vectorMasina = {
                    1.0 - Normalize(masina.Pret, minPret, maxPret),
                    Normalize(masina.An_Fabricatie, minAn, maxAn),
                    1.0 - Normalize(masina.Kilometraj, minKm, maxKm),
                    Normalize(masina.CapacitateMotor, minCapacitate, maxCapacitate),
                    Normalize(masina.PutereCP, minPutere, maxPutere)
                };

                double similaritate = CosineSimilarity(vectorUtilizator, vectorMasina);
                recomandari.Add(masina, similaritate);
            }

            // 4. Returnăm primele "topRecomandari" rezultate, sortate descrescător după scor
            return recomandari.OrderByDescending(r => r.Value)
                              .Take(topRecomandari)
                              .Select(r => r.Key)
                              .ToList();
        }

        // Metoda matematică Cosine Similarity
        private double CosineSimilarity(double[] vectorA, double[] vectorB)
        {
            double dotProduct = 0;
            double normA = 0;
            double normB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += Math.Pow(vectorA[i], 2);
                normB += Math.Pow(vectorB[i], 2);
            }

            if (normA == 0 || normB == 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        // Metoda de Normalizare Min-Max
        private double Normalize(decimal value, decimal min, decimal max)
        {
            if (max == min) return 0;
            return (double)((value - min) / (max - min));
        }
    }
}