using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly double[] _minValues;
        private readonly double[] _maxValues;

        public RecomandareService(PlatformaDbContext context, IConfiguration configuration)
        {
            _context = context;
            // Preluăm limitele globale din appsettings.json pentru normalizarea KNN
            _minValues = configuration.GetSection("KNNSettings:MinValues").Get<double[]>();
            _maxValues = configuration.GetSection("KNNSettings:MaxValues").Get<double[]>();
        }

        public async Task<DreamCarComparisonViewModel> CalculeazaDreamCarComparativ(DreamCarViewModel dreamCar, int topRecomandari = 3)
        {
            // PASUL 1: FILTRARE HARD
            var query = _context.Anunt.AsQueryable();

            if (dreamCar.PretDorit > 0)
                query = query.Where(a => a.Pret <= dreamCar.PretDorit);
            if (dreamCar.AnMinim > 0)
                query = query.Where(a => a.An_Fabricatie >= dreamCar.AnMinim);
            if (dreamCar.KilometrajMaxim > 0)
                query = query.Where(a => a.Kilometraj <= dreamCar.KilometrajMaxim); 

            var masiniValide = await query.ToListAsync();
            if (!masiniValide.Any()) return new DreamCarComparisonViewModel();

            // Limite locale pentru Radar Chart
            decimal localMinPret = masiniValide.Min(a => a.Pret);
            decimal localMaxPret = masiniValide.Max(a => a.Pret);
            int localMinAn = masiniValide.Min(a => a.An_Fabricatie);
            int localMaxAn = masiniValide.Max(a => a.An_Fabricatie);
            int localMinKm = masiniValide.Min(a => a.Kilometraj);
            int localMaxKm = masiniValide.Max(a => a.Kilometraj);
            int localMinCap = masiniValide.Min(a => a.CapacitateMotor);
            int localMaxCap = masiniValide.Max(a => a.CapacitateMotor);
            int localMinPutere = masiniValide.Min(a => a.PutereCP);
            int localMaxPutere = masiniValide.Max(a => a.PutereCP);

            // PASUL 2: VECTORI UTILIZATOR — DIFERIȚI pentru fiecare algoritm
            // Cosine: valori ideale (direcție optimă)
            double[] vectorCosine = {
            1.0 - NormalizeKNN((double)dreamCar.PretDorit, 0), // preț cât mai mic
            1.0,  // an cât mai nou
            0.0,  // km cât mai puțini
            NormalizeKNN(dreamCar.CapacitateMotorDorita, 3),
            NormalizeKNN(dreamCar.PutereCPDorita, 4)
            };

            // KNN: valorile EXACTE introduse de utilizator (distanță fizică)
            double[] vectorKNN = {
            NormalizeKNN((double)dreamCar.PretDorit, 0),
            NormalizeKNN(dreamCar.AnMinim, 1),
            NormalizeKNN(dreamCar.KilometrajMaxim, 2),
            NormalizeKNN(dreamCar.CapacitateMotorDorita, 3),
            NormalizeKNN(dreamCar.PutereCPDorita, 4)
            };
            var toateRezultatele = new List<DreamCarResultViewModel>();

            // PASUL 3: CALCUL SCORURI PENTRU TOATE MAȘINILE
            foreach (var masina in masiniValide)
            {
                double[] vectorMasina = {
                    NormalizeKNN((double)masina.Pret, 0),
                    NormalizeKNN(masina.An_Fabricatie, 1),
                    NormalizeKNN(masina.Kilometraj, 2),
                    NormalizeKNN(masina.CapacitateMotor, 3),
                    NormalizeKNN(masina.PutereCP, 4)
                };

                // După — fiecare folosește vectorul său propriu
                double simCosine = CosineSimilarity(vectorCosine, vectorMasina);
                double distEuclidean = EuclideanDistance(vectorKNN, vectorMasina);
                double simEuclidean = 1.0 / (1.0 + distEuclidean);

                // Logica Deal
                var similare = masiniValide.Where(a => a.ID_Anunt != masina.ID_Anunt &&
                    (a.Marca == masina.Marca || Math.Abs(a.CapacitateMotor - masina.CapacitateMotor) <= 200)).ToList();

                decimal mediaPret = similare.Any() ? (decimal)similare.Average(a => (double)a.Pret) : (decimal)masiniValide.Average(a => (double)a.Pret);
                decimal diferenta = masina.Pret - mediaPret;
                decimal procent = mediaPret > 0 ? (diferenta / mediaPret) * 100 : 0;

                string label, color;
                if (procent <= -10) { label = "🟢 Deal Bun"; color = "success"; }
                else if (procent <= 10) { label = "🟡 Preț Corect"; color = "warning"; }
                else { label = "🔴 Suprapreț"; color = "danger"; }

                toateRezultatele.Add(new DreamCarResultViewModel
                {
                    Anunt = masina,
                    ScorSimilaritate = Math.Round(simCosine * 100, 1),
                    ScorEuclidian = Math.Round(simEuclidean * 100, 1),
                    DealLabel = label,
                    DealColor = color,
                    MediaPretSimilare = Math.Round(mediaPret, 0),
                    DiferentaPret = Math.Round(diferenta, 0),
                    GlobalMinAn = localMinAn,
                    GlobalMaxAn = localMaxAn,
                    GlobalMinKm = localMinKm,
                    GlobalMaxKm = localMaxKm,
                    GlobalMinPret = localMinPret,
                    GlobalMaxPret = localMaxPret,
                    GlobalMinPutere = localMinPutere,
                    GlobalMaxPutere = localMaxPutere,
                    GlobalMinCapacitate = localMinCap,
                    GlobalMaxCapacitate = localMaxCap
                });
            }

            // PASUL 4: SEPARAREA ÎN DOUĂ LISTE
            return new DreamCarComparisonViewModel
            {
                // Top bazat pe Cosine (Stil/Profil)
                RecomandariStil = toateRezultatele
                    .OrderByDescending(r => r.ScorSimilaritate)
                    .Take(topRecomandari)
                    .ToList(),

                // Top bazat pe KNN (Cifre exacte/Proximitate)
                RecomandariPrecizie = toateRezultatele
                    .OrderByDescending(r => r.ScorEuclidian)
                    .Take(topRecomandari)
                    .ToList()
            };
        }

        private double CosineSimilarity(double[] vA, double[] vB)
        {
            double dot = 0, nA = 0, nB = 0;
            for (int i = 0; i < vA.Length; i++)
            {
                dot += vA[i] * vB[i];
                nA += Math.Pow(vA[i], 2);
                nB += Math.Pow(vB[i], 2);
            }
            return (nA == 0 || nB == 0) ? 0 : dot / (Math.Sqrt(nA) * Math.Sqrt(nB));
        }

        private double EuclideanDistance(double[] vA, double[] vB)
        {
            double sum = 0;
            for (int i = 0; i < vA.Length; i++) sum += Math.Pow(vA[i] - vB[i], 2);
            return Math.Sqrt(sum);
        }

        private double NormalizeKNN(double val, int idx)
        {
            if (_minValues == null || _maxValues == null) return 0;
            double min = _minValues[idx], max = _maxValues[idx];
            return max == min ? 0 : Math.Clamp((val - min) / (max - min), 0.0, 1.0);
        }
    }
}