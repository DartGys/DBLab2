using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallWebLaba1.Models;
using System.Numerics;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace FootBallWebLaba1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class ClubsController : Controller
    {
        private readonly FootBallBdContext _context;

        public ClubsController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: matches
        public async Task<IActionResult> Index(string importSuccess)
        {
            ViewBag.importSuccess = importSuccess;
            return View(await _context.Clubs.ToListAsync());
        }

        public IActionResult PosRequest()
        {
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionName");
            return View();
        }

        public async Task<IActionResult> PosRequestPost(int quantity, int positionId)
        {
            var clubs = _context.Clubs.ToList();

            List<Club> result = new List<Club>();

            for (int i = 0; i < clubs.Count; i++)
            {
                var request = _context.Players.Where(c => c.ClubId == clubs[i].ClubId && c.PositionId == positionId).ToList();

                if (request.Count >= quantity)
                    result.Add(clubs[i]);
            }
            ViewBag.hidden = 1;
            ExportStatic.clubSet(result);
            return View("Index", result);
        }

        public IActionResult PlayerGoalRequest()
        {
            return View();
        }

        public async Task<IActionResult> PlayerGoalRequestPost(int quantity)
        {
            var clubs = _context.Clubs.ToList();

            List<Club> result = new List<Club>();

            for (int i = 0; i < clubs.Count; i++)
            {
                var player = _context.Players.Where(c => c.ClubId == clubs[i].ClubId).ToList();

                for(int j = 0; j < player.Count; j++)
                {
                    var goal = _context.ScoredGoals.Where(p => p.PlayerId == player[j].PlayerId).ToList();

                    if (goal.Count > quantity)
                    {
                        result.Add(clubs[i]);
                        break;
                    }
                }

            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result);
        }

        public IActionResult GoalsRequest()
        {
            return View();
        }

        public async Task<IActionResult> GoalsRequestPost(int quantity)
        {
            var clubs = _context.Clubs.ToList();

            List<Club> result = new List<Club>();

            for (int i = 0; i < clubs.Count; i++)
            {
                var player = _context.Players.Where(c => c.ClubId == clubs[i].ClubId).ToList();

                int goalSum = 0;

                for (int j = 0; j < player.Count; j++)
                {
                    var goal = _context.ScoredGoals.Where(p => p.PlayerId == player[j].PlayerId).ToList();

                    goalSum += goal.Count;
                }

                if(goalSum > quantity)
                    result.Add(clubs[i]);

                goalSum = 0;
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result);
        }

        public IActionResult ClubRequest()
        {
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            return View();
        }

        public async Task<IActionResult> ClubRequestPost(int clubId)
        {
            var clubs = _context.Clubs.Where(c => c.ClubId != clubId).ToList();

            List<Club> result = new List<Club>();

            for (int i = 0; i < clubs.Count; i++)
            {
                var mathes = _context.Matches.FirstOrDefault(c => (c.HostClubId == clubs[i].ClubId || c.GuestClubId == clubs[i].ClubId) && (c.HostClubId == clubId || c.GuestClubId == clubId));

                if (mathes != null)
                    result.Add(clubs[i]);
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result);
        }

        public IActionResult MatchRequest()
        {
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            return View();
        }

        public async Task<IActionResult> MatchRequestPost(int clubId)
        {
            var clubs = _context.Clubs.Where(c => c.ClubId != clubId).ToList();

            var quantity = _context.Matches.Where(c => c.HostClubId == clubId || c.GuestClubId == clubId).Count();

            List<Club> result = new List<Club>();

            for (int i = 0; i < clubs.Count; i++)
            {
                var mathes = _context.Matches.Where(c => (c.HostClubId == clubs[i].ClubId || c.GuestClubId == clubs[i].ClubId) && (c.HostClubId == clubId || c.GuestClubId == clubId)).ToList();

                if (mathes.Count >= quantity)
                    result.Add(clubs[i]);
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result);
        }

        public IActionResult ClubAllStadiumRequest()
        {
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            return View();
        }

        public async Task<IActionResult> ClubAllStadiumRequestPost(int clubId)
        {
            List<Club> result = new List<Club>();
            
            using (SqlConnection connection = new SqlConnection(@"Server = DESKTOP-CFOLTDF\SQLEXPRESS; Database = FootBallBD; Trusted_Connection = true; MultipleActiveResultSets = true; TrustServerCertificate = true"))
            {

                string query = @"
SELECT c.ClubId
FROM Club c
WHERE NOT EXISTS (
    SELECT ch.ChampionshipId
    FROM Championship ch
    WHERE ch.ChampionshipId IN (
        SELECT DISTINCT m.ChampionshipId
        FROM Match m
        WHERE (m.HostClubId = (SELECT ClubId FROM Club WHERE ClubId = @ClubId)
               OR m.GuestClubId = (SELECT ClubId FROM Club WHERE ClubId = @ClubId))
    )
    AND ch.ChampionshipId NOT IN (
        SELECT DISTINCT m.ChampionshipId
        FROM Match m
        WHERE (m.HostClubId = c.ClubId OR m.GuestClubId = c.ClubId)
    )
) and c.ClubId <> @ClubId;";         


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubId", clubId);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int clubIdDb = reader.GetInt32(0);
                            result.Add(_context.Clubs.Find(clubIdDb));
                        }
                    }
                }
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result.ToList().Distinct());
        }

        public IActionResult ClubOnlyStadiumRequest()
        {
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            return View();
        }

        public async Task<IActionResult> ClubOnlyStadiumRequestPost(int clubId)
        {
            List<Club> result = new List<Club>();

            using (SqlConnection connection = new SqlConnection(@"Server = DESKTOP-CFOLTDF\SQLEXPRESS; Database = FootBallBD; Trusted_Connection = true; MultipleActiveResultSets = true; TrustServerCertificate = true"))
                {

                string query = @"
SELECT c.ClubId
FROM Club c
WHERE EXISTS (
    SELECT 1
    FROM Match m
    INNER JOIN Championship ch ON m.ChampionshipId = ch.ChampionshipId
    WHERE (m.HostClubId = c.ClubId OR m.GuestClubId = c.ClubId)
        AND ch.ChampionshipId IN (
            SELECT ch2.ChampionshipId
            FROM Club c2
            INNER JOIN Match m2 ON (m2.HostClubId = c2.ClubId OR m2.GuestClubId = c2.ClubId)
            INNER JOIN Championship ch2 ON m2.ChampionshipId = ch2.ChampionshipId
            WHERE c2.CLubId = @ClubId
        )
)
AND NOT EXISTS (
    SELECT 1
    FROM Match m3
    INNER JOIN Championship ch3 ON m3.ChampionshipId = ch3.ChampionshipId
    WHERE (m3.HostClubId = c.ClubId OR m3.GuestClubId = c.ClubId)
        AND ch3.ChampionshipId NOT IN (
            SELECT ch4.ChampionshipId
            FROM Club c4
            INNER JOIN Match m4 ON (m4.HostClubId = c4.ClubId OR m4.GuestClubId = c4.ClubId)
            INNER JOIN Championship ch4 ON m4.ChampionshipId = ch4.ChampionshipId
            WHERE c4.ClubId = @ClubId
        )
) and c.ClubId <> @ClubId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubId", clubId);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int clubIdDb = reader.GetInt32(0);
                            result.Add(_context.Clubs.Find(clubIdDb));
                        }
                    }
                }
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result.ToList().Distinct());
        }

        public IActionResult ClubNotStadiumRequest()
        {
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            return View();
        }

        public async Task<IActionResult> ClubNotStadiumRequestPost(int clubId)
        {
            List<Club> result = new List<Club>();

            using (SqlConnection connection = new SqlConnection(@"Server = DESKTOP-CFOLTDF\SQLEXPRESS; Database = FootBallBD; Trusted_Connection = true; MultipleActiveResultSets = true; TrustServerCertificate = true"))
            {

                string query = @"
SELECT c.ClubId
FROM Club c
WHERE NOT EXISTS (
    SELECT ch.ChampionshipId
    FROM Championship ch
    WHERE ch.ChampionshipId IN (
        SELECT DISTINCT m.ChampionshipId
        FROM Match m
        WHERE (m.HostClubId = (SELECT ClubId FROM Club WHERE ClubId = @ClubId)
               OR m.GuestClubId = (SELECT ClubId FROM Club WHERE ClubId = @ClubId))
    )
    AND ch.ChampionshipId IN (
        SELECT DISTINCT m.ChampionshipId
        FROM Match m
        WHERE (m.HostClubId = c.ClubId OR m.GuestClubId = c.ClubId)
    )
) and c.ClubId <> @ClubId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubId", clubId);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int clubIdDb = reader.GetInt32(0);
                            result.Add(_context.Clubs.Find(clubIdDb));
                        }
                    }
                }
            }
            ExportStatic.clubSet(result);
            ViewBag.hidden = 1;
            return View("Index", result.ToList().Distinct());
        }

        // GET: matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Clubs == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        public async Task<IActionResult> PlayersList(int? id)
        {
            if (id == null || _context.Clubs == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return RedirectToAction("IndexPlayers", "Players", new { id = club.ClubId, name = club.ClubId });
        }

        // GET: matches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: matches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClubId,ClubName,ClubOrigin,ClubPlayerQuantity,ClubCoachName,ClubEstablishmentDate")] Club club)
        {
            club.ClubPlayerQuantity = 0;
            if (ModelState.IsValid)
            {
                var clubName = _context.Clubs.FirstOrDefault(c => c.ClubName == club.ClubName);
                var clubCoach = _context.Clubs.FirstOrDefault(c => c.ClubCoachName == club.ClubCoachName);

                DateTime curDate = DateTime.Now;
                DateTime clubDate = club.ClubEstablishmentDate;

                if (clubDate > curDate)
                {
                    ModelState.AddModelError("ClubEstablishmentDate", "Дата створення клубу не відповідає дійсності");
                    return View(club);
                }

                if (clubCoach != null)
                {
                    ModelState.AddModelError("ClubCoachName", "Цей тренер уже тренує іншу команду");
                    return View(club);
                }

                if (clubName != null)
                {
                    ModelState.AddModelError("ClubName", "Команда з такою назвою уже існує");
                    return View(club);
                }

                _context.Add(club);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Stadiums", new { clubId = club.ClubId });
            }
            return View(club);
        }

        // GET: matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Clubs == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs.FindAsync(id);
            if (club == null)
            {
                return NotFound();
            }
            return View(club);
        }

        // POST: matches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClubId,ClubName,ClubOrigin,ClubPlayerQuantity,ClubCoachName,ClubEstablishmentDate")] Club club)
        {
            if (id != club.ClubId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var clubName = _context.Clubs.FirstOrDefault(c => c.ClubName == club.ClubName && c.ClubId != club.ClubId);
                var clubCoach = _context.Clubs.FirstOrDefault(c => c.ClubCoachName == club.ClubCoachName && c.ClubId != club.ClubId);

                if (clubCoach != null)
                {
                    ModelState.AddModelError("ClubCoachName", "Цей тренер уже тренує іншу команду");
                    return View(club);
                }

                if (clubName != null)
                {
                    ModelState.AddModelError("ClubName", "Команда з такою назвою уже існує");
                    return View(club);
                }

                try
                {
                    _context.Update(club);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubExists(club.ClubId))
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
            return View(club);
        }

        // GET: matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Clubs == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        // POST: matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Clubs == null)
            {
                return Problem("Entity set 'FootBallBdContext.matches'  is null.");
            }

            var club = await _context.Clubs
                .Include(c => c.Players)
                .Include(c => c.Stadiums)
                .FirstOrDefaultAsync(m => m.ClubId == id);

            var match = await _context.Matches
                .Where(m => m.HostClubId == id || m.GuestClubId == id)
                .Include(m => m.ScoredGoals)
                .FirstOrDefaultAsync();


            if (match != null)
            {
                foreach (var s in match.ScoredGoals)
                    _context.Remove(s);
                _context.Matches.Remove(match);
            }


            if (club != null)
            {
                foreach (var s in club.Stadiums)
                    _context.Remove(s);

                foreach (var p in club.Players)
                    _context.Remove(p);

                _context.Clubs.Remove(club);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ClubExists(int id)
        {
            return _context.Clubs.Any(e => e.ClubId == id);
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {

                var clubs = ExportStatic.Clubs;

                ViewBag.hidden = 1;
                if (clubs.Count == 0) return View("Index", clubs);

                foreach (var club in clubs)
                {
                    var worksheet = workbook.Worksheets.Add(club.ClubName);
                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Походження";
                    worksheet.Cell("C1").Value = "Кількість гравців";
                    worksheet.Cell("D1").Value = "Тренер";
                    worksheet.Cell("E1").Value = "Дата заснування";
                    worksheet.Row(1).Style.Font.Bold = true;

                    worksheet.Cell(2, 1).Value = club.ClubName;
                    worksheet.Cell(2, 2).Value = club.ClubOrigin;
                    worksheet.Cell(2, 3).Value = club.ClubPlayerQuantity;
                    worksheet.Cell(2, 4).Value = club.ClubCoachName;
                    worksheet.Cell(2, 5).Value = club.ClubEstablishmentDate.ToShortDateString();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"clubs.xlsx"
                    };
                }
            }
        }
    } 
}

