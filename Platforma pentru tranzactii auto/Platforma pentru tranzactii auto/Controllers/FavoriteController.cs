using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly PlatformaDbContext _context;

        public FavoriteController(PlatformaDbContext context)
        {
            _context = context;
        }

        

[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int anuntId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Verificăm dacă există deja în favorite
        var existing = await _context.Favorite
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ID_Anunt == anuntId);

        if (existing != null)
        {
            _context.Favorite.Remove(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Anunturi");
        }

        // Adăugăm
        var fav = new Favorite
        {
            UserId = userId,
            ID_Anunt = anuntId
        };

        _context.Favorite.Add(fav);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Anunturi");
    }


    // GET: Favorite
    public async Task<IActionResult> Index()
        {
            var platformaDbContext = _context.Favorite.Include(f => f.Anunt).Include(f => f.User);
            return View(await platformaDbContext.ToListAsync());
        }

        // GET: Favorite/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorite
                .Include(f => f.Anunt)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ID_Favorite == id);
            if (favorite == null)
            {
                return NotFound();
            }

            return View(favorite);
        }

        // GET: Favorite/Create
        public IActionResult Create()
        {
            ViewData["ID_Anunt"] = new SelectList(_context.Anunt, "ID_Anunt", "Marca");
            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id");
            return View();
        }

        // POST: Favorite/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Favorite,UserId,ID_Anunt")] Favorite favorite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(favorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ID_Anunt"] = new SelectList(_context.Anunt, "ID_Anunt", "Marca", favorite.ID_Anunt);
            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", favorite.UserId);
            return View(favorite);
        }

        // GET: Favorite/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorite.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }
            ViewData["ID_Anunt"] = new SelectList(_context.Anunt, "ID_Anunt", "Marca", favorite.ID_Anunt);
            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", favorite.UserId);
            return View(favorite);
        }

        // POST: Favorite/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Favorite,UserId,ID_Anunt")] Favorite favorite)
        {
            if (id != favorite.ID_Favorite)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favorite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavoriteExists(favorite.ID_Favorite))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ID_Anunt"] = new SelectList(_context.Anunt, "ID_Anunt", "Marca", favorite.ID_Anunt);
            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", favorite.UserId);
            return View(favorite);
        }

        // GET: Favorite/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorite
                .Include(f => f.Anunt)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ID_Favorite == id);
            if (favorite == null)
            {
                return NotFound();
            }

            return View(favorite);
        }

        // POST: Favorite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favorite = await _context.Favorite.FindAsync(id);
            if (favorite != null)
            {
                _context.Favorite.Remove(favorite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FavoriteExists(int id)
        {
            return _context.Favorite.Any(e => e.ID_Favorite == id);
        }
    }
}
