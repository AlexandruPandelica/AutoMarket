using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;


namespace Platforma_pentru_tranzactii_auto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PlatformaDbContext _context;

        public HomeController(ILogger<HomeController> logger, PlatformaDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lu?m cele mai noi 3 anun?uri ordonate dup? data post?rii
            var ultimeleAnunturi = await _context.Anunt
                .OrderByDescending(a => a.Data_Postarii)
                .Take(3)
                .ToListAsync();

            // Trimitem lista de 3 anun?uri c?tre View
            return View(ultimeleAnunturi);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
