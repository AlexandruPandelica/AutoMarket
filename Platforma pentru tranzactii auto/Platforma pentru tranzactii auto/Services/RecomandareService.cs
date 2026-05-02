using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // PASUL 1: FILTRAREA HARD (Pre-procesare)
            // Nu băgăm în algoritm mașini care nu respectă limitele stricte ale utilizatorului.
            var query = _context.Anunt.AsQueryable();

            if (dreamCar.PretDorit > 0)
                query = query.Where(a => a.Pret <= dreamCar.PretDorit);

            if (dreamCar.AnMinim > 0)
                query = query.Where(a => a.An_Fabricatie >= dreamCar.AnMinim);

            if (dreamCar.KilometrajMaxim > 0)
                query = query.Where(a => a.Kilometraj <= dreamCar.KilometrajMaxim);

            // Aducem din baza de date DOAR mașinile valide
            var masiniValide = await query.ToListAsync();

            // Dacă nicio mașină nu se încadrează în buget/an/km, ne oprim aici
            if (!masiniValide.Any()) return new List<Anunturi>();

            // PASUL 2: NORMALIZAREA (pe baza mașinilor rămase, pentru precizie maximă)
            decimal maxPret = masiniValide.Max(a => a.Pret) == 0 ? 1 : masiniValide.Max(a => a.Pret);
            decimal minPret = masiniValide.Min(a => a.Pret);

            int maxAn = masiniValide.Max(a => a.An_Fabricatie) == 0 ? 1 : masiniValide.Max(a => a.An_Fabricatie);
            int minAn = masiniValide.Min(a => a.An_Fabricatie);

            int maxKm = masiniValide.Max(a => a.Kilometraj) == 0 ? 1 : masiniValide.Max(a => a.Kilometraj);
            int minKm = masiniValide.Min(a => a.Kilometraj);

            int maxCapacitate = masiniValide.Max(a => a.CapacitateMotor) == 0 ? 1 : masiniValide.Max(a => a.CapacitateMotor);
            int minCapacitate = masiniValide.Min(a => a.CapacitateMotor);

            int maxPutere = masiniValide.Max(a => a.PutereCP) == 0 ? 1 : masiniValide.Max(a => a.PutereCP);
            int minPutere = masiniValide.Min(a => a.PutereCP);

            // PASUL 3: Creăm Vectorul Ideal al utilizatorului
            double[] vectorUtilizator = {
            1.0 - Normalize(dreamCar.PretDorit, minPret, maxPret),
            1.0,  // vrea întotdeauna cel mai nou an posibil
            0.0,  // vrea întotdeauna cel mai mic kilometraj posibil
            Normalize(dreamCar.CapacitateMotorDorita, minCapacitate, maxCapacitate),
            Normalize(dreamCar.PutereCPDorita, minPutere, maxPutere)
            };

            var recomandari = new Dictionary<Anunturi, double>();

            // PASUL 4: Calculăm similaritatea doar pentru mașinile pe care și le permite
            foreach (var masina in masiniValide)
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

            // PASUL 5: Returnăm primele "topRecomandari" rezultate, sortate descrescător după scor
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

        // Metoda de Normalizare Min-Max (actualizată cu Clamp pentru siguranță)
        private double Normalize(decimal value, decimal min, decimal max)
        {
            if (max == min) return 0;

            double normalized = (double)((value - min) / (max - min));

            // Asigurăm că valoarea rămâne strict între 0 și 1
            return Math.Clamp(normalized, 0.0, 1.0);
        }
    }
}