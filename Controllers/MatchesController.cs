using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallWebLaba1.Models;
using ClosedXML.Excel;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;

namespace FootBallWebLaba1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class MatchesController : Controller
    {
        private readonly FootBallBdContext _context;

        public MatchesController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Championships", "Index");

            ViewBag.ChampionshipId = id;  
            ViewBag.ChampionshipName = name;
            var matchesByChampionships = _context.Matches.Where(b => b.ChampionshipId == id).Include(b => b.Championship).Include(b => b.HostClub).Include(b => b.GuestClub).Include(b => b.Stadium);
            return View(await matchesByChampionships.ToListAsync());
        }

        public IActionResult Request(int? id)
        {
            ViewBag.ChampionshipId = id;
            return View();
        }

        public async Task<IActionResult> MatchRequest(int? id, int quantity)
        {
            var match = _context.Matches.Where(m => m.ChampionshipId == id).Include(m => m.HostClub).Include(m => m.GuestClub).Include(m => m.Championship).Include(m => m.Stadium).ToList();

            List<Match> result = new List<Match>();

            for (int i = 0; i < match.Count; i++)
            {
                var scoredGoals = _context.ScoredGoals.Where(c => c.MatchId == match[i].MatchId).ToList();

                if (scoredGoals.Count > quantity)
                    result.Add(match[i]);
            }
            ExportStatic.matchSet(result);
            ViewBag.hidden = 1;
            return View("Index", result);
            
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Championship)
                .Include(m => m.GuestClub)
                .Include(m => m.HostClub)
                .Include(m => m.Stadium)
                .FirstOrDefaultAsync(m => m.MatchId == id);
            if (match == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "ScoredGoals", new { id = match.MatchId});
        }



        // GET: Matches/Create
        public IActionResult Create(int championshipId)
        {
            //ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry");
            ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            //ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation");
            ViewBag.ChampionshipId = championshipId;
            ViewBag.ChampionshipName = _context.Championships.Where(c => c.ChampionshipId == championshipId).FirstOrDefault().ChampionshipName;
            return View();
        }

        // POST: Matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int championshipId,[Bind("MatchId,MatchDate,MatchDuration,StaidumId,HostClubId,GuestClubId,ChampionshipId")] Match match)
        {
            match.ChampionshipId = championshipId;
            var stadium = await _context.Stadiums.FirstAsync(s => s.ClubId == match.HostClubId);
            match.StaidumId = stadium.StadiumId;
            if (ModelState.IsValid)
            {
                if(match.HostClubId == match.GuestClubId)
                {
                    ModelState.AddModelError("MatchDuration", "В матчі беруть участь дві різні команди");
                    ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
                    ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
                    ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
                    ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
                    ViewBag.ChampionshipId = championshipId;
                    return View(match);
                }

                var guestClubPlayers = await _context.Players.FirstOrDefaultAsync(g => g.ClubId == match.GuestClubId);
                var hostClubPlayers = await _context.Players.FirstOrDefaultAsync(g => g.ClubId == match.HostClubId);


                if(hostClubPlayers == null || hostClubPlayers == null)
                {
                    ModelState.AddModelError("MatchDuration", "Зазначені команди не мають жодного гравця в складі");
                    ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
                    ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
                    ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
                    ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
                    ViewBag.ChampionshipId = championshipId;
                    return View(match);
                }

                var guestClub = await _context.Clubs.FirstOrDefaultAsync(g => g.ClubId == match.GuestClubId);
                var hostClub = await _context.Clubs.FirstOrDefaultAsync(g => g.ClubId == match.HostClubId);

                DateTime dateTime = DateTime.Now;

                if (guestClub.ClubEstablishmentDate > match.MatchDate || hostClub.ClubEstablishmentDate > match.MatchDate)
                {
                    ModelState.AddModelError("MatchDate", "Дата проведення матчу не може передувати даті створення команд");
                    ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
                    ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
                    ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
                    ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
                    ViewBag.ChampionshipId = championshipId;
                    return View(match);
                }

                if(match.MatchDate > dateTime)
                {
                    ModelState.AddModelError("MatchDate", "Дата проведення матчу не може бути назначена на майбутнє");
                    ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
                    ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
                    ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
                    ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
                    ViewBag.ChampionshipId = championshipId;
                    return View(match);
                }    

                _context.Add(match);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Matches", new { id = championshipId, name = _context.Championships.Where(c => c.ChampionshipId == championshipId).FirstOrDefault().ChampionshipName});
            }
            ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
            ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
            ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
            ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
            return View(match);
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            //ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
            ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
            ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
            ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
            return View(match);
        }

        // POST: Matches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MatchId,MatchDate,MatchDuration,StaidumId,HostClubId,GuestClubId,ChampionshipId")] Match match)
        {
            if (id != match.MatchId)
            {
                return NotFound();
            }

            var matchChamp = await _context.Matches.FindAsync(id);
            var champId = matchChamp.ChampionshipId;
            match.ChampionshipId = champId;

            if (ModelState.IsValid)
            {
                if (match.HostClubId == match.GuestClubId)
                {
                    ModelState.AddModelError("MatchDuration", "В матчі беруть участь дві різні команди");
                    ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
                    ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
                    ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
                    ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
                    ViewBag.ChampionshipId = match.ChampionshipId;
                    return View(match);
                }
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.MatchId))
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
            //ViewData["ChampionshipId"] = new SelectList(_context.Championships, "ChampionshipId", "ChampionshipCountry", match.ChampionshipId);
            ViewData["GuestClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.GuestClubId);
            ViewData["HostClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", match.HostClubId);
            ViewData["StaidumId"] = new SelectList(_context.Stadiums, "StaidumId", "StadiumLocation", match.StaidumId);
            return View(match);
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.GuestClub)
                .Include(m => m.HostClub)
                .Include(m => m.Stadium)
                .FirstOrDefaultAsync(m => m.MatchId == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Matches == null)
            {
                return Problem("Entity set 'FootBallBdContext.Matches'  is null.");
            }
            var match = await _context.Matches.FindAsync(id);
            var champId = match.ChampionshipId;
            if (match != null)
            {
                _context.Matches.Remove(match);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Championships", new {id = champId});
        }

        private bool MatchExists(int id)
        {
          return _context.Matches.Any(e => e.MatchId == id);
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {

                var matches = ExportStatic.Matches;

                ViewBag.hidden = 1;
                if (matches.Count == 0) return View("Index", matches);

                int title = 1;
                foreach (var match in matches)
                {
                    var worksheet = workbook.Worksheets.Add(title++);
                    worksheet.Cell("A1").Value = "Дата";
                    worksheet.Cell("B1").Value = "Тривалість";
                    worksheet.Cell("C1").Value = "Стадіон";
                    worksheet.Cell("D1").Value = "Хазяїва";
                    worksheet.Cell("E1").Value = "Гості";
                    worksheet.Cell("F1").Value = "Чемпіонат";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = match.MatchDate.ToShortDateString();
                    worksheet.Cell(2, 2).Value = match.MatchDuration;
                    worksheet.Cell(2, 3).Value = match.Stadium.StadiumLocation;
                    worksheet.Cell(2, 4).Value = match.HostClub.ClubName;
                    worksheet.Cell(2, 5).Value = match.GuestClub.ClubName;
                    worksheet.Cell(2, 6).Value = match.Championship.ChampionshipName;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"matches.xlsx"
                    };
                }
            }
        }

    }
}
