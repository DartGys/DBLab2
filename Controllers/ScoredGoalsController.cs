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
    public class ScoredGoalsController : Controller
    {
        private readonly FootBallBdContext _context;

        public ScoredGoalsController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: ScoredGoals
        public async Task<IActionResult> Index(int? id)
        {

            if (id == null) return RedirectToAction("Matches", "Index");

            ViewBag.MatchId = id; 
            var matchesGoal = _context.ScoredGoals.Where(b => b.MatchId == id).Include(b => b.Match).Include(b => b.Player);
            return View(await matchesGoal.ToListAsync());
        }

        // GET: ScoredGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ScoredGoals == null)
            {
                return NotFound();
            }

            var scoredGoal = await _context.ScoredGoals
                .Include(s => s.Match)
                .Include(s => s.Player)
                .FirstOrDefaultAsync(m => m.ScoredGoalId == id);


            if (scoredGoal == null)
            {
                return NotFound();
            }

            return RedirectToAction("Details", "Players", new { id = scoredGoal.PlayerId });
        }

        // GET: ScoredGoals/Create
        public IActionResult Create(int MatchId)
        {
            //ViewData["MatchId"] = new SelectList(_context.Matches, "MatchId", "MatchId");
            int hostClubId = _context.Matches.FirstOrDefault(m => m.MatchId == MatchId).HostClubId;
            int guestClubId = _context.Matches.FirstOrDefault(m => m.MatchId == MatchId).GuestClubId;
            var player = _context.Players.Where(p => p.ClubId == hostClubId).Union(_context.Players.Where(p => p.ClubId == guestClubId));
            ViewData["PlayerId"] = new SelectList(player, "PlayerId", "PlayerName");
            ViewBag.MatchId = MatchId;
            return View();
        }

        // POST: ScoredGoals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int MatchId,[Bind("ScoredGoalId,ScoredMinute,PlayerId,MatchId")] ScoredGoal scoredGoal)
        {
            scoredGoal.MatchId = MatchId;
            if (ModelState.IsValid)
            {
                var match = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == MatchId);

                if(match.MatchDuration < scoredGoal.ScoredMinute || scoredGoal.ScoredMinute < 1)
                {
                    int hostClubId = _context.Matches.FirstOrDefault(m => m.MatchId == MatchId).HostClubId;
                    int guestClubId = _context.Matches.FirstOrDefault(m => m.MatchId == MatchId).GuestClubId;
                    ViewData["PlayerId"] = new SelectList(_context.Players.Where(p => p.ClubId == hostClubId && p.ClubId == guestClubId), "PlayerId", "PlayerName");
                    ModelState.AddModelError("ScoredMinute", "Вказана хвилина виходить за рамки тривалості матчу");
                    ViewBag.MatchId = MatchId;
                    return View(scoredGoal);
                }

                _context.Add(scoredGoal);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "ScoredGoals", new { id = MatchId, name = _context.Matches.Where(c => c.MatchId == MatchId).FirstOrDefault().MatchDate });
            }
            //ViewData["MatchId"] = new SelectList(_context.Matches, "MatchId", "MatchId", scoredGoal.MatchId);
            //ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerName", scoredGoal.PlayerId);
            return View(scoredGoal);
        }

        // GET: ScoredGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ScoredGoals == null)
            {
                return NotFound();
            }

            var scoredGoal = await _context.ScoredGoals.FindAsync(id);
            if (scoredGoal == null)
            {
                return NotFound();
            }
            ViewData["MatchId"] = new SelectList(_context.Matches, "MatchId", "MatchId", scoredGoal.MatchId);
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerName", scoredGoal.PlayerId);
            return View(scoredGoal);
        }

        // POST: ScoredGoals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScoredGoalId,ScoredMinute,PlayerId,MatchId")] ScoredGoal scoredGoal)
        {
            if (id != scoredGoal.ScoredGoalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scoredGoal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScoredGoalExists(scoredGoal.ScoredGoalId))
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
            ViewData["MatchId"] = new SelectList(_context.Matches, "MatchId", "MatchId", scoredGoal.MatchId);
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerName", scoredGoal.PlayerId);
            return View(scoredGoal);
        }

        // GET: ScoredGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ScoredGoals == null)
            {
                return NotFound();
            }

            var scoredGoal = await _context.ScoredGoals
                .Include(s => s.Match)
                .Include(s => s.Player)
                .FirstOrDefaultAsync(m => m.ScoredGoalId == id);
            if (scoredGoal == null)
            {
                return NotFound();
            }

            return View(scoredGoal);
        }

        // POST: ScoredGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ScoredGoals == null)
            {
                return Problem("Entity set 'FootBallBdContext.ScoredGoals'  is null.");
            }
            var scoredGoal = await _context.ScoredGoals.FindAsync(id);
            int MatchId = scoredGoal.MatchId;
            if (scoredGoal != null)
            {
                _context.ScoredGoals.Remove(scoredGoal);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "ScoredGoals", new { id = MatchId, name = _context.Matches.Where(c => c.MatchId == MatchId).FirstOrDefault().MatchDate });
        }

        private bool ScoredGoalExists(int id)
        {
          return _context.ScoredGoals.Any(e => e.ScoredGoalId == id);
        }
    }
}
