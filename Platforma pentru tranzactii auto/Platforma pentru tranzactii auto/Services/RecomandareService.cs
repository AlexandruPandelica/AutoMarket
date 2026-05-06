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

        public async Task<List<DreamCarResultViewModel>> CalculeazaDreamCar(DreamCarViewModel dreamCar, int topRecomandari = 3)
        {
            // PASUL 1: FILTRAREA HARD
            var query = _context.Anunt.AsQueryable();

            if (dreamCar.PretDorit > 0)
                query = query.Where(a => a.Pret <= dreamCar.PretDorit);
            if (dreamCar.AnMinim > 0)
                query = query.Where(a => a.An_Fabricatie >= dreamCar.AnMinim);
            if (dreamCar.KilometrajMaxim > 0)
                query = query.Where(a => a.Kilometraj <= dreamCar.KilometrajMaxim);

            var masiniValide = await query.ToListAsync();
            if (!masiniValide.Any()) return new List<DreamCarResultViewModel>();

            // PASUL 2: NORMALIZARE
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

            // PASUL 3: VECTOR UTILIZATOR
            double[] vectorUtilizator = {
        1.0 - Normalize(dreamCar.PretDorit, minPret, maxPret),
        1.0,
        0.0,
        Normalize(dreamCar.CapacitateMotorDorita, minCapacitate, maxCapacitate),
        Normalize(dreamCar.PutereCPDorita, minPutere, maxPutere)
    };

            var recomandari = new Dictionary<Anunturi, double>();

            // PASUL 4: COSINE SIMILARITY
            foreach (var masina in masiniValide)
            {
                double[] vectorMasina = {
            1.0 - Normalize(masina.Pret, minPret, maxPret),
            Normalize(masina.An_Fabricatie, minAn, maxAn),
            1.0 - Normalize(masina.Kilometraj, minKm, maxKm),
            Normalize(masina.CapacitateMotor, minCapacitate, maxCapacitate),
            Normalize(masina.PutereCP, minPutere, maxPutere)
        };

                recomandari.Add(masina, CosineSimilarity(vectorUtilizator, vectorMasina));
            }

            // PASUL 5: TOP 3 + calcul scor deal
            var top = recomandari
                .OrderByDescending(r => r.Value)
                .Take(topRecomandari)
                .ToList();

            var rezultate = new List<DreamCarResultViewModel>();

            foreach (var item in top)
            {
                var masina = item.Key;

                // Găsim mașini similare (aceeași marcă sau capacitate apropiată ±200cm3)
                var similare = masiniValide
                    .Where(a => a.ID_Anunt != masina.ID_Anunt &&
                               (a.Marca == masina.Marca ||
                                Math.Abs(a.CapacitateMotor - masina.CapacitateMotor) <= 200))
                    .ToList();

                decimal mediaPret = similare.Any()
                        ? (decimal)similare.Average(a => (double)a.Pret)
    :                     (decimal)masiniValide.Average(a => (double)a.Pret);

                decimal diferenta = masina.Pret - mediaPret;
                decimal procentDiferenta = mediaPret > 0 ? (diferenta / mediaPret) * 100 : 0;

                string label, color;
                if (procentDiferenta <= -10) { label = "🟢 Deal Bun"; color = "success"; }
                else if (procentDiferenta <= 10) { label = "🟡 Preț Corect"; color = "warning"; }
                else { label = "🔴 Suprapreț"; color = "danger"; }

                rezultate.Add(new DreamCarResultViewModel
                {
                    Anunt = masina,
                    ScorSimilaritate = Math.Round(item.Value * 100, 1),
                    DealLabel = label,
                    DealColor = color,
                    MediaPretSimilare = Math.Round(mediaPret, 0),
                    DiferentaPret = Math.Round(diferenta, 0),
                    // ← adaugă astea
                    GlobalMinAn = minAn,
                    GlobalMaxAn = maxAn,
                    GlobalMinKm = minKm,
                    GlobalMaxKm = maxKm,
                    GlobalMinPutere = minPutere,
                    GlobalMaxPutere = maxPutere,
                    GlobalMinCapacitate = minCapacitate,
                    GlobalMaxCapacitate = maxCapacitate,
                    GlobalMinPret = minPret,
                    GlobalMaxPret = maxPret,
                });
            }

            return rezultate;
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