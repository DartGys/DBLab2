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
            var match = _context.Matches.Where(m => m.ChampionshipId == id).ToList();

            List<Match> result = new List<Match>();

            for (int i = 0; i < match.Count; i++)
            {
                var scoredGoals = _context.ScoredGoals.Where(c => c.MatchId == match[i].MatchId).ToList();

                if (scoredGoals.Count > quantity)
                    result.Add(match[i]);
            }

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

                var checkAnotherChamp = await _context.Matches.FirstOrDefaultAsync(c => c.ChampionshipId != match.ChampionshipId && (c.HostClubId == match.HostClubId || c.GuestClubId == match.HostClubId || c.HostClubId == match.GuestClubId || c.GuestClubId == match.GuestClubId));
                    
                if(checkAnotherChamp != null)
                {
                    ModelState.AddModelError("MatchDuration", "Вказана команда уже бере участь в іншому чемпіонаті");
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



        public ActionResult ExportMatchesToExcel()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                // Отримати дані з бази даних
                var matches = _context.Matches
                    .Include(m => m.HostClub)
                    .Include(m => m.GuestClub)
                    .Include(m => m.ScoredGoals)
                        .ThenInclude(sg => sg.Player.Position)
                    .Include(m => m.Stadium)
                    .ToList();

                // Створити новий Excel 
                var worksheet = workbook.Worksheets.Add("Matches");

                // Додати заголовки стовпців
                worksheet.Cell(1, 1).Value = "MatchId";
                worksheet.Cell(1, 2).Value = "Команда хазяїв";
                worksheet.Cell(1, 3).Value = "Команда гостей";
                worksheet.Cell(1, 4).Value = "Забиті голи";
                worksheet.Cell(1, 5).Value = "Гравець";
                worksheet.Cell(1, 6).Value = "Позиція гравця";
                worksheet.Cell(1, 7).Value = "Локація";

                // Додати дані з бази даних
                for (int i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    worksheet.Cell(i + 2, 1).Value = match.MatchId;
                    worksheet.Cell(i + 2, 2).Value = match.HostClub.ClubName;
                    worksheet.Cell(i + 2, 3).Value = match.GuestClub.ClubName;
                    worksheet.Cell(i + 2, 7).Value = match.Stadium.StadiumLocation;
                    int row = i + 2;
                    foreach (var s in match.ScoredGoals)
                    {
                        worksheet.Cell(row, 4).Value = string.Join(",", match.ScoredGoals.Select(sg => sg.ScoredMinute));
                        worksheet.Cell(row, 5).Value = string.Join(",", match.ScoredGoals.Select(sg => sg.Player.PlayerName));
                        worksheet.Cell(row, 6).Value = string.Join(",", match.ScoredGoals.Select(sg => sg.Player.Position.PositionName));
                        
                    }
                }

                // Зберегти файл Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"mathes{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }

        public async Task<IActionResult> ImportMatchesFromExcel(IFormFile fileExel)
        {
            if (fileExel != null && fileExel.Length > 0)
            {
                using (var stream = fileExel.OpenReadStream())
                {
                    using (XLWorkbook workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var row = 2;
                        var matches = new List<Match>();
                        while (!worksheet.Cell(row, 1).IsEmpty())
                        {
                            var match = new Match();
                            match.HostClub = new Club();
                            match.GuestClub = new Club();
                            //match.ScoredGoals = new List<ScoredGoal>();
                            match.Stadium = new Stadium();

                            // Отримати дані з рядка
                            match.MatchId = worksheet.Cell(row, 1).GetValue<int>();
                            match.HostClub.ClubName = worksheet.Cell(row, 2).GetValue<string>();
                            match.GuestClub.ClubName = worksheet.Cell(row, 3).GetValue<string>();
                            var scoredMinutes = worksheet.Cell(row, 4).GetValue<string>();
                            var playerNames = worksheet.Cell(row, 5).GetValue<string>();
                            var positionNames = worksheet.Cell(row, 6).GetValue<string>();
                            match.Stadium.StadiumLocation = worksheet.Cell(row, 7).GetValue<string>();

                            // Розділити дані про забиті голи на масиви
                            var scoredMinutesArray = scoredMinutes.Split(',');
                            var playerNamesArray = playerNames.Split(',');
                            var positionNamesArray = positionNames.Split(',');

                            // Додати дані про голи до матчу
                            for (int i = 0; i < scoredMinutesArray.Length; i++)
                            {
                                var scoredGoal = new ScoredGoal();
                                scoredGoal.MatchId = match.MatchId;
                                scoredGoal.ScoredMinute = Convert.ToInt32(scoredMinutesArray[i]);
                                scoredGoal.Player = new Player();
                                scoredGoal.Player.PlayerName = playerNamesArray[i];
                                scoredGoal.Player.Position = new Position();
                                scoredGoal.Player.Position.PositionName = positionNamesArray[i];
                                match.ScoredGoals.Add(scoredGoal);
                                
                            }

                            matches.Add(match);
                            row++;
                        }

                        // Зберегти дані в базі даних
                        await _context.Matches.AddRangeAsync(matches);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}
