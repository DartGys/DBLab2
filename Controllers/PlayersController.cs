using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallWebLaba1.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace FootBallWebLaba1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class PlayersController : Controller
    {
        private readonly FootBallBdContext _context;

        public PlayersController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            var footBallBdContext = _context.Players.Include(p => p.Club).Include(p => p.Position);
            return View(await footBallBdContext.ToListAsync());
        }

        public async Task<IActionResult> IndexPlayers(int? id)
        {
            if (id == null) return RedirectToAction("Matches", "Index");

            ViewBag.ClubId = id;
            var club = await _context.Clubs.FindAsync(id);
            ViewBag.ClubName = club.ClubName;
            var playersByClub = _context.Players.Where(b => b.ClubId == id).Include(b => b.Club).Include(p => p.Position);
            return View(await playersByClub.ToListAsync());
        }

        public IActionResult Request()
        {
            ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "PlayerName");
            return View();
        }

        public async Task<IActionResult> PlayerRequest(int playerId)
        {
            var players = _context.Players.Where(p => p.PlayerId != playerId).ToList();

            int quantity = _context.ScoredGoals.Where(p => p.PlayerId == playerId).Count();

            List<Player> result = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                var scoredGoals = _context.ScoredGoals.Where(p => p.PlayerId == players[i].PlayerId).ToList();

                if (scoredGoals.Count >= quantity)
                    result.Add(players[i]);
            }

            return View("Index", result);
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Club)
                .FirstOrDefaultAsync(m => m.PlayerId == id);
            if (player == null)
            {
                return NotFound();
            }

            return RedirectToAction("Details", "Clubs", new { id = player.ClubId });
        }

        // GET: Players/Create
        public IActionResult Create(int clubId)
        {
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
            ViewBag.ClubId = clubId;
            return View();
        }

        // POST: Players/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int clubId, [Bind("PlayerId,PlayerName,PlayerNumber,PositionId,PlayerSalary,PlayerBirthDate,ClubId")] Player player)
        {
            player.ClubId = clubId;
            if (ModelState.IsValid)
            {
                var playerName = _context.Players.FirstOrDefault(p => p.PlayerName == player.PlayerName);
                var playerInClub = _context.Players.FirstOrDefault(p => p.ClubId == player.ClubId && p.PlayerNumber == player.PlayerNumber);

                DateTime curDate = DateTime.Now;
                DateTime playerDate = player.PlayerBirthDate;

                int age = curDate.Year - playerDate.Year;
                if(age < 16 || age > 50)
                {
                    ViewBag.ClubId = clubId;
                    ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
                    ModelState.AddModelError("PlayerBirthDate", "Вік гравеця повинен бути від 16 до 50 років");
                    return View(player);
                }

                if (playerName != null)
                {
                    ViewBag.ClubId = clubId;
                    ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
                    ModelState.AddModelError("PlayerName", "Гравець з таким іменем існує");
                    return View(player);
                }

                if (playerInClub != null)
                {
                    ViewBag.ClubId = clubId;
                    ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
                    ModelState.AddModelError("PlayerNumber", "Цей номер уже зайнятий");
                    return View(player);
                }


                _context.Add(player);
                await _context.SaveChangesAsync();
                var club = await _context.Clubs.FindAsync(player.ClubId);
                club.ClubPlayerQuantity++;
                _context.Update(club);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Clubs");
            }
            //ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", player.ClubId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
            ViewBag.ClubId = clubId;
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", player.ClubId);
            return View(player);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerId,PlayerName,PlayerNumber,PlayerPosition,PlayerSalary,PlayerBirthDate,ClubId")] Player player)
        {
            if (id != player.PlayerId)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {

                var playerName = _context.Players.FirstOrDefault(p => p.PlayerName == player.PlayerName && p.PlayerId != player.PlayerId);
                var playerInClub = _context.Players.FirstOrDefault(p => p.ClubId == player.ClubId && p.PlayerNumber == player.PlayerNumber && p.PlayerId != player.PlayerId);

                if (playerName != null)
                {
                    ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", player.ClubId);
                    ModelState.AddModelError("PlayerName", "Гравець з таким іменем існує");
                    return View(player);
                }
                if(playerInClub != null)
                {
                    ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", player.ClubId);
                    ModelState.AddModelError("PlayerNumber", "Цей номер уже зайнятий");
                    return View(player);
                }

                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerId))
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
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubCoachName", player.ClubId);
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Club)
                .FirstOrDefaultAsync(m => m.PlayerId == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Players == null)
            {
                return Problem("Entity set 'FootBallBdContext.Players'  is null.");
            }
            var player = await _context.Players
                .Where(p => p.PlayerId == id)
                .Include(p => p.ScoredGoals)
                .FirstOrDefaultAsync();

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.ClubId == player.ClubId);
            if (player != null)
            {
                foreach (var g in player.ScoredGoals)
                    _context.Remove(g);

                _context.Players.Remove(player);

                club.ClubPlayerQuantity--;
                _context.Update(club);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index","Clubs");
        }

        private bool PlayerExists(int id)
        {
          return _context.Players.Any(e => e.PlayerId == id);
        }
    }
}
