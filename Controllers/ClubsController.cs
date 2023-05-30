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
                    Select c.ClubId
from Club c
join Match m on m.HostClubId = c.ClubId or m.GuestClubId = c.ClubId
where m.MatchId IN(
    select m3.MatchId
    from Match m3
    where m3.StaidumId in (
        select m4.StaidumId
        From Match m4
        WHERE m4.HostClubId = @ClubId or m4.GuestClubId = @ClubId 
        )
) and c.ClubId <> @ClubId";         


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
Select c.ClubId
from Club c
join Match m on m.HostClubId = c.ClubId or m.GuestClubId = c.ClubId
where m.GuestClubId IN(
    select m3.GuestClubId
    from Match m3
    where m3.StaidumId in (
        select m4.StaidumId
        From Match m4
        WHERE m4.HostClubId = @ClubId
        )
)
and m.GuestClubId NOT IN(
    select m5.GuestClubId
    from Match m5
    where m5.StaidumId not in (
        select m6.StaidumId
        From Match m6
        WHERE m6.HostClubId = @ClubId
        )
) and c.ClubId <> @ClubId
";

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
                Select c.ClubId
from Club c
join Match m on m.HostClubId = c.ClubId or m.GuestClubId = c.ClubId
where m.HostClubId NOT IN(
    select m3.HostClubId
    from Match m3
    where m3.StaidumId in (
        select m4.StaidumId
        From Match m4
        WHERE m4.GuestClubId = @ClubId 
        )
    )
    and m.GuestClubId NOT IN(
    select m5.GuestClubId
    from Match m5
    where m5.StaidumId in (
        select m6.StaidumId
        From Match m6
        WHERE m6.HostClubId = @ClubId 
        )
    )";

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

        public ActionResult ExportToExcel()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                // Отримати дані з бази даних
                var clubs = _context.Clubs
                    .Include(c => c.Players)
                    .ThenInclude(p => p.Position)
                    .Include(c => c.Stadiums).ToList();

                // Створити новий Excel 
                var worksheet = workbook.Worksheets.Add("Matches");

                // Додати заголовки стовпців
                worksheet.Cell(1, 1).Value = "ClubId";
                worksheet.Cell(1, 2).Value = "ClubName";
                worksheet.Cell(1, 3).Value = "ClubOrigin";
                worksheet.Cell(1, 4).Value = "ClubCoachName";
                worksheet.Cell(1, 5).Value = "ClubEstablishmentDate";
                worksheet.Cell(1, 6).Value = "StadiumLocation";
                worksheet.Cell(1, 7).Value = "StadiumCapacity";
                worksheet.Cell(1, 8).Value = "StadiumEstablismentDate";
                worksheet.Cell(1, 9).Value = "PlayerName";
                worksheet.Cell(1, 10).Value = "PlayerNumber";
                worksheet.Cell(1, 11).Value = "PositionName";
                worksheet.Cell(1, 12).Value = "PlayerSalary";
                worksheet.Cell(1, 13).Value = "PlayerBirthDate";

                // Додати дані з бази даних
                for (int i = 0; i < clubs.Count; i++)
                {
                    var club = clubs[i];
                    worksheet.Cell(i + 2, 1).Value = club.ClubId;
                    worksheet.Cell(i + 2, 2).Value = club.ClubName;
                    worksheet.Cell(i + 2, 3).Value = club.ClubOrigin;
                    worksheet.Cell(i + 2, 4).Value = club.ClubCoachName;
                    worksheet.Cell(i + 2, 5).Value = club.ClubEstablishmentDate.ToShortDateString();
                    worksheet.Cell(i + 2, 6).Value = string.Join(",",club.Stadiums.Select(s => s.StadiumLocation));
                    worksheet.Cell(i + 2, 7).Value = string.Join(",", club.Stadiums.Select(s => s.StadiumCapacity));
                    worksheet.Cell(i + 2, 8).Value = string.Join(",",club.Stadiums.Select(s => s.StadiumEstablismentDate.ToShortDateString()));
                    worksheet.Cell(i + 2, 9).Value = string.Join(",", club.Players.Select(p => p.PlayerName));
                    worksheet.Cell(i + 2, 10).Value = string.Join(",", club.Players.Select(p => p.PlayerNumber));
                    worksheet.Cell(i + 2, 11).Value = string.Join(",", club.Players.Select(p => p.Position.PositionName));
                    worksheet.Cell(i + 2, 12).Value = string.Join(";", club.Players.Select(p => p.PlayerSalary));
                    worksheet.Cell(i + 2, 13).Value = string.Join(",", club.Players.Select(p => p.PlayerBirthDate.ToShortDateString()));
                }

                // Зберегти файл Excel
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


        public async Task<IActionResult> ImportFromExcel(IFormFile fileExel)
        {
            string importSuccess = "Файл завнтажено успішно. ";
            if (fileExel != null && fileExel.Length > 0)
            {
                using (var stream = fileExel.OpenReadStream())
                {
                    try
                    {
                        XLWorkbook workbook = new XLWorkbook(stream);
                    }
                    catch
                    {
                        return RedirectToAction("Index", new { importSuccess = "Формат файлу невірний" });
                    }
                    using (XLWorkbook workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var row = 2;
                        var failedAdd = new List<int>();
                        var clubs = new List<Club>();
                        while (!worksheet.Cell(row, 1).IsEmpty())
                        {
                            var club = new Club();

                            // Отримати дані з рядка
                  
                            club.ClubName = worksheet.Cell(row, 2).GetValue<string>();
                            club.ClubOrigin = worksheet.Cell(row, 3).GetValue<string>();
                            club.ClubCoachName = worksheet.Cell(row, 4).GetValue<string>();
                            club.ClubEstablishmentDate = Convert.ToDateTime(worksheet.Cell(row, 5).GetValue<string>());

                            var clubCheck = _context.Clubs.FirstOrDefault(c => c.ClubName == club.ClubName || c.ClubOrigin == club.ClubOrigin || c.ClubCoachName == club.ClubCoachName);

                            if (clubCheck != null)
                            {
                                failedAdd.Add(row);
                                string clubProperty = string.Empty;
                                if (clubCheck.ClubName == club.ClubName) clubProperty += $"назва: {club.ClubName}";
                                if (clubCheck.ClubOrigin == club.ClubOrigin) clubProperty += string.Join(", ", $"походження: {club.ClubOrigin}");
                                if (clubCheck.ClubCoachName == club.ClubCoachName) clubProperty += string.Join(", ", $"тренер: {club.ClubCoachName}");
                                importSuccess += $"Команда з назвою {club.ClubName} не доданаc, через те що {clubProperty} уже зайняті іншими командами. ";
                            }

                            if(clubCheck == null)
                            clubs.Add(club);
                            row++;
                        }

                        // Зберегти дані в базі даних
                        await _context.Clubs.AddRangeAsync(clubs);
                        await _context.SaveChangesAsync();
                        row = 2;
                        int clubCount = 0;
                        var stadiums = new List<Stadium>();
                        var players = new List<Player>();
                        while (!worksheet.Cell(row, 1).IsEmpty())
                        {
                            if (failedAdd.Contains(row))
                            {
                                row++;
                                continue;
                            }

                            var stadium = new Stadium();

                            stadium.ClubId = clubs[clubCount].ClubId;
                            stadium.StadiumLocation = worksheet.Cell(row, 6).GetValue<string>();
                            stadium.StadiumCapacity = Convert.ToInt32(worksheet.Cell(row, 7).GetValue<string>());
                            stadium.StadiumEstablismentDate = Convert.ToDateTime(worksheet.Cell(row, 8).GetValue<string>());

                            var stadiumCheck = _context.Stadiums.FirstOrDefault(s => s.StadiumLocation == stadium.StadiumLocation);
                            if (stadiumCheck != null)
                            {
                                importSuccess += $"Команда {clubs[clubCount].ClubName} не була додана, тому що локація {stadium.StadiumLocation} зайнята. Змініть її і спробуйте додати команду ще раз. ";
                                _context.Clubs.Remove(clubs[clubCount]);
                                clubs.RemoveAt(clubCount);
                                await _context.SaveChangesAsync();
                                row++;
                                clubCount++;
                                continue;
                            }


                            var playerName = worksheet.Cell(row, 9).GetValue<string>();
                            var playerNumber = worksheet.Cell(row, 10).GetValue<string>();
                            var playerPosition = worksheet.Cell(row, 11).GetValue<string>();
                            var playerSalary = worksheet.Cell(row, 12).GetValue<string>();
                            var playerBirthDate = worksheet.Cell(row, 13).GetValue<string>();

                            var playerNameArray = playerName.Split(',');
                            var playerNumberArray = playerNumber.Split(',');
                            var playerPositionArray = playerPosition.Split(',');
                            var playerSalaryArray = playerSalary.Split(';');
                            var playerBirthDateArray = playerBirthDate.Split(',');

                            for(int i = 0; i< playerNameArray.Length; i++)
                            {
                                var player = new Player();
                                player.ClubId = clubs[clubCount].ClubId;
                                player.PlayerName = playerNameArray[i];
                                player.PlayerNumber = Convert.ToInt32(playerNumberArray[i]);
                                player.PositionId = _context.Positions.First(p => p.PositionName == playerPositionArray[i]).PositionId;
                                player.PlayerSalary = Convert.ToDecimal(playerSalaryArray[i]);
                                player.PlayerBirthDate = Convert.ToDateTime(playerBirthDateArray[i]);

                                var playerCheck = _context.Players.FirstOrDefault(p => p.PlayerName == player.PlayerName);
                                if (playerCheck != null)
                                    importSuccess += $"Гравець {player.PlayerName} команди {clubs[clubCount].ClubName} не був доданий, тому що таке гравець з таким іменем уже існує. ";

                                if (playerCheck == null)
                                {
                                    clubs[clubCount].ClubPlayerQuantity++;
                                    players.Add(player);
                                }
                            }
                            if(stadiumCheck == null)
                            stadiums.Add(stadium);
                            row++;
                            clubCount++;
                        }

                        _context.Clubs.UpdateRange(clubs);
                        await _context.SaveChangesAsync();

                        await _context.Stadiums.AddRangeAsync(stadiums);
                        await _context.SaveChangesAsync();

                        await _context.Players.AddRangeAsync(players);
                        await _context.SaveChangesAsync();

                    }
                }
            }

            if (fileExel == null) return RedirectToAction("Index", new { importSuccess = "Ви не вибрали файл для завантаженн" });
            if (fileExel.Length < 0) return RedirectToAction("Index", new { importSuccess = "Вибраний файл пустий, або містить хибну інформацію" });
            return RedirectToAction("Index", new { importSuccess = importSuccess });
        }
    } 
}

