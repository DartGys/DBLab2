using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallWebLaba1.Models;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace FootBallWebLaba1.Controllers
{
    [Authorize(Roles = "admin, user")]
    public class StadiumsController : Controller
    {
        private readonly FootBallBdContext _context;

        public StadiumsController(FootBallBdContext context)
        {
            _context = context;
        }

        // GET: Stadia
        public async Task<IActionResult> Index()
        {
            var footBallBdContext = _context.Stadiums.Include(s => s.Club);
            return View(await footBallBdContext.ToListAsync());
        }

        // GET: Stadia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Stadiums == null)
            {
                return NotFound();
            }

            var stadium = await _context.Stadiums
                .Include(s => s.Club)
                .FirstOrDefaultAsync(m => m.StadiumId == id);
            if (stadium == null)
            {
                return NotFound();
            }
            
            return RedirectToAction("Details", "Clubs", new { id = stadium.ClubId });
        }

        public async Task<IActionResult> DetailsStadium(int? id)
        {
            if (id == null || _context.Stadiums == null)
            {
                return NotFound();
            }

            var stadium = await _context.Stadiums
                .Include(s => s.Club)
                .FirstOrDefaultAsync(m => m.StadiumId == id);
            if (stadium == null)
            {
                return NotFound();
            }

            return View(stadium);
        }

        // GET: Stadia/Create
        public IActionResult Create(int clubId)
        {
            //ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName");
            ViewBag.ClubId = clubId;
            return View();
        }

        // POST: Stadia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int clubId,[Bind("StaidumId,StadiumLocation,StadiumCapacity,StadiumEstablismentDate,ClubId")] Stadium stadium)
        {
            stadium.ClubId = clubId;
            if (ModelState.IsValid)
            {
                DateTime curDate = DateTime.Now;
                DateTime estDate = stadium.StadiumEstablismentDate;

                if(estDate > curDate)
                {
                    ViewBag.ClubId = clubId;
                    ModelState.AddModelError("StadiumEstablismentDate", "Дата заснування не відповідає дійсності");
                    return View(stadium);
                }

                var locationStadium = await _context.Stadiums.FirstOrDefaultAsync(l => l.StadiumLocation == stadium.StadiumLocation);
                if(locationStadium != null)
                {
                    ViewBag.ClubId = clubId;
                    ModelState.AddModelError("StadiumLocation", "Стадіон з такою локацією уже існує");
                    return View(stadium);
                }

                _context.Add(stadium);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Clubs");
            }
            //ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubName", stadium.ClubId);
            return View(stadium);
        }

        // GET: Stadia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Stadiums == null)
            {
                return NotFound();
            }

            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium == null)
            {
                return NotFound();
            }
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubCoachName", stadium.ClubId);
            return View(stadium);
        }

        // POST: Stadia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StaidumId,StadiumLocation,StadiumCapacity,StadiumEstablismentDate,ClubId")] Stadium stadium)
        {
            if (id != stadium.StadiumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                DateTime curDate = DateTime.Now;
                DateTime estDate = stadium.StadiumEstablismentDate;

                if (estDate > curDate)
                {
                    ModelState.AddModelError("StadiumEstablismentDate", "Дата заснування не відповідає дійсності");
                    return View(stadium);
                }

                var locationStadium = await _context.Stadiums.FirstOrDefaultAsync(l => l.StadiumLocation == stadium.StadiumLocation && l.StadiumId != stadium.StadiumId);
                if (locationStadium != null)
                {
                    ModelState.AddModelError("StadiumLocation", "Стадіон з такою локацією уже існує");
                    return View(stadium);
                }

                try
                {
                    _context.Update(stadium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StadiumExists(stadium.StadiumId))
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
            ViewData["ClubId"] = new SelectList(_context.Clubs, "ClubId", "ClubCoachName", stadium.ClubId);
            return View(stadium);
        }

        // GET: Stadia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Stadiums == null)
            {
                return NotFound();
            }

            var stadium = await _context.Stadiums
                .Include(s => s.Club)
                .FirstOrDefaultAsync(m => m.StadiumId == id);
            if (stadium == null)
            {
                return NotFound();
            }

            return View(stadium);
        }

        // POST: Stadia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Stadiums == null)
            {
                return Problem("Entity set 'FootBallBdContext.Stadiums'  is null.");
            }
            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium != null)
            {
                _context.Stadiums.Remove(stadium);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StadiumExists(int id)
        {
          return _context.Stadiums.Any(e => e.StadiumId == id);
        }
    }
}
