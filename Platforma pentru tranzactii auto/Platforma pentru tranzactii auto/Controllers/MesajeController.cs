using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using System.Security.Claims;

namespace Platforma_pentru_tranzactii_auto.Controllers
{
    [Authorize]
    public class MesajeController : Controller
    {
        private readonly PlatformaDbContext _context;
        private readonly UserManager<Utilizator> _userManager;

        public MesajeController(PlatformaDbContext context, UserManager<Utilizator> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Mesaje
        public async Task<IActionResult> Index(int? conversatieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Preluăm toate mesajele unde utilizatorul este implicat
            var toateMesajele = await _context.Mesaje
                .Include(m => m.Expeditor)
                .Include(m => m.Destinatar)
                .Include(m => m.Anunt)
                .Where(m => m.DestinatarId == user.Id || m.ExpeditorId == user.Id)
                .OrderByDescending(m => m.DataTrimiterii)
                .ToListAsync();

            // 2. Grupăm mesajele pentru a crea lista de conversații (stânga)
            var conversatii = toateMesajele
                .GroupBy(m => new {
                    PartnerId = m.ExpeditorId == user.Id ? m.DestinatarId : m.ExpeditorId,
                    AnuntId = m.ID_Anunt
                })
                .Select(g => g.First())
                .ToList();

            ViewBag.Conversatii = conversatii;
            ViewBag.CurrentUserId = user.Id;

            // 3. Dacă o conversație este selectată, încărcăm tot istoricul (dreapta)
            if (conversatieId.HasValue)
            {
                var msgReferinta = toateMesajele.FirstOrDefault(m => m.ID_Mesaj == conversatieId.Value);
                if (msgReferinta != null)
                {
                    int partnerId = msgReferinta.ExpeditorId == user.Id ? msgReferinta.DestinatarId : msgReferinta.ExpeditorId;

                    var chatActiv = toateMesajele
                        .Where(m => m.ID_Anunt == msgReferinta.ID_Anunt &&
                                   ((m.ExpeditorId == user.Id && m.DestinatarId == partnerId) ||
                                    (m.ExpeditorId == partnerId && m.DestinatarId == user.Id)))
                        .OrderBy(m => m.DataTrimiterii)
                        .ToList();

                    ViewBag.ChatActiv = chatActiv;
                    ViewBag.SelectedPartner = msgReferinta.ExpeditorId == user.Id ? msgReferinta.Destinatar : msgReferinta.Expeditor;
                    ViewBag.SelectedAnunt = msgReferinta.Anunt;
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Raspunde(int anuntId, int destinatarId, string continut)
        {
            var expeditor = await _userManager.GetUserAsync(User);
            if (expeditor == null || string.IsNullOrWhiteSpace(continut)) return BadRequest();

            var mesajNou = new Mesaje
            {
                ExpeditorId = expeditor.Id,
                DestinatarId = destinatarId,
                ID_Anunt = anuntId,
                Continut = continut,
                DataTrimiterii = DateTime.UtcNow
            };

            _context.Mesaje.Add(mesajNou);
            await _context.SaveChangesAsync();

            // Redirecționăm înapoi la conversație folosind ID-ul noului mesaj
            return RedirectToAction(nameof(Index), new { conversatieId = mesajNou.ID_Mesaj });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Trimite(int anuntId, int destinatarId, string continut)
        {
            var expeditor = await _userManager.GetUserAsync(User);
            if (expeditor == null || string.IsNullOrWhiteSpace(continut)) return BadRequest();

            // Validare: să nu-ți trimiți mesaj singur
            if (expeditor.Id == destinatarId) return RedirectToAction("Details", "Anunturi", new { id = anuntId });

            var mesajNou = new Mesaje
            {
                ExpeditorId = expeditor.Id,
                DestinatarId = destinatarId,
                ID_Anunt = anuntId,
                Continut = continut,
                DataTrimiterii = DateTime.UtcNow
            };

            _context.Mesaje.Add(mesajNou);
            await _context.SaveChangesAsync();

            // Redirecționăm utilizatorul direct în Inbox la noua conversație
            return RedirectToAction("Index", "Mesaje", new { conversatieId = mesajNou.ID_Mesaj });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editeaza(int id, string noulContinut, int currentConversatieId)
        {
            var mesaj = await _context.Mesaje.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            // Verificăm dacă mesajul există și dacă aparține utilizatorului logat
            if (mesaj != null && mesaj.ExpeditorId == user.Id)
            {
                if (!string.IsNullOrWhiteSpace(noulContinut))
                {
                    mesaj.Continut = noulContinut;
                    // Opțional: poți marca mesajul ca fiind editat
                    // mesaj.Continut += " (editat)"; 

                    _context.Update(mesaj);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index), new { conversatieId = currentConversatieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sterge(int id, int? currentConversatieId)
        {
            var mesaj = await _context.Mesaje.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (mesaj != null && (mesaj.DestinatarId == user.Id || mesaj.ExpeditorId == user.Id))
            {
                _context.Mesaje.Remove(mesaj);
                await _context.SaveChangesAsync();
            }

            // Dacă avem un ID de conversație activă, rămânem acolo
            if (currentConversatieId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { conversatieId = currentConversatieId.Value });
            }

            return RedirectToAction(nameof(Index));
        }

        // Șterge TOATĂ conversația (Metoda NOUĂ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeConversatie(int anuntId, int partnerId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Selectăm toate mesajele dintre utilizatorul curent și partener pentru anunțul specific
            var mesajeDeSters = await _context.Mesaje
                .Where(m => m.ID_Anunt == anuntId &&
                           ((m.ExpeditorId == user.Id && m.DestinatarId == partnerId) ||
                            (m.ExpeditorId == partnerId && m.DestinatarId == user.Id)))
                .ToListAsync();

            if (mesajeDeSters.Any())
            {
                _context.Mesaje.RemoveRange(mesajeDeSters);
                await _context.SaveChangesAsync();
            }

            // Fiindcă am șters totul, ne întoarcem la Inbox-ul principal (fără nicio conversație selectată)
            return RedirectToAction(nameof(Index));
        }


    }
}