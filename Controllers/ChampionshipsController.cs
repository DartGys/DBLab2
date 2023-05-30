using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallWebLaba1.Models;
using Microsoft.AspNetCore.Authorization;

namespace FootBallWebLaba1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class ChampionshipsController : Controller
    {
        private readonly FootBallBdContext _context;

        public ChampionshipsController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: Championships
        public async Task<IActionResult> Index()
        {
            return View(await _context.Championships.ToListAsync());
        }

        public IActionResult Request()
        {
            return View();
        }

        public async Task<IActionResult> ChampRequest(int quantity)
        {
            var champ = _context.Championships.ToList();

            List<Championship> result = new List<Championship>();

            for(int i = 0; i < champ.Count; i++)
            {
                var request = _context.Matches.Where(c => c.ChampionshipId == champ[i].ChampionshipId).ToList();

                if (request.Count > quantity)
                    result.Add(champ[i]);
            }

            return View("Index", result);
        }

        // GET: Championships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Championships == null)
            {
                return NotFound();
            }

            var championship = await _context.Championships
                .FirstOrDefaultAsync(m => m.ChampionshipId == id);
            if (championship == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Matches", new { id = championship.ChampionshipId, name = championship.ChampionshipName });
        }

        // GET: Championships/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Championships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChampionshipId,ChampionshipCountry,ChampionshipName,ChampionshipClubQuantity")] Championship championship)
        {
            if (ModelState.IsValid)
            {
                var championsips = await _context.Championships.FirstOrDefaultAsync(c => c.ChampionshipName == championship.ChampionshipName);
                if(championsips != null)
                {
                    ModelState.AddModelError("ChampionshipName", "Чемпіонат з такою назвою уже існіє");
                    return View(championship);
                }

                _context.Add(championship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(championship);
        }

        // GET: Championships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Championships == null)
            {
                return NotFound();
            }

            var championship = await _context.Championships.FindAsync(id);
            if (championship == null)
            {
                return NotFound();
            }
            return View(championship);
        }

        // POST: Championships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChampionshipId,ChampionshipCountry,ChampionshipName,ChampionshipClubQuantity")] Championship championship)
        {
            if (id != championship.ChampionshipId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var championsips = await _context.Championships.FirstOrDefaultAsync(c => c.ChampionshipName == championship.ChampionshipName && c.ChampionshipId != championship.ChampionshipId);
                if (championsips != null)
                {
                    ModelState.AddModelError("ChampionshipName", "Чемпіонат з такою назвою уже існіє");
                    return View(championship);
                }

                try
                {
                    _context.Update(championship);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChampionshipExists(championship.ChampionshipId))
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
            return View(championship);
        }

        // GET: Championships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Championships == null)
            {
                return NotFound();
            }

            var championship = await _context.Championships
                .FirstOrDefaultAsync(m => m.ChampionshipId == id);
            if (championship == null)
            {
                return NotFound();
            }

            return View(championship);
        }

        // POST: Championships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Championships == null)
            {
                return Problem("Entity set 'FootBallBdContext.Championships'  is null.");
            }
            var championship = await _context.Championships.FindAsync(id);

            var matches = await _context.Matches
                .Where(m => m.ChampionshipId == id)
                .Include(m => m.ScoredGoals)
                .FirstOrDefaultAsync();
            if(matches != null)
            {
                foreach (var s in matches.ScoredGoals)
                    _context.Remove(s);

                _context.Matches.Remove(matches);
            }

            if (championship != null)
            {
                _context.Championships.Remove(championship);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChampionshipExists(int id)
        {
          return _context.Championships.Any(e => e.ChampionshipId == id);
        }
    }
}
