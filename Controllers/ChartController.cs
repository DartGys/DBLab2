using FootBallWebLaba1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FootBallWebLaba1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly FootBallBdContext _context;

        public ChartController(FootBallBdContext context)
        {
            _context = context;
        }
        
        [HttpGet("JsonDataClub")]
        public JsonResult JsonDataClub()
        {
            var Club = _context.Clubs.Include(c => c.Players).ToList();
            List<object> playerClub = new List<object>();
            playerClub.Add(new[] { "Команда", "Кількість гравців" });
            foreach (var p in Club)
                playerClub.Add(new object[] { p.ClubName, p.Players.Count() });

            return new JsonResult(playerClub);
        }
        [HttpGet("JsonDataChamp")]
        public JsonResult JsonDataChamp()
        {
            var champ = _context.Championships.Include(c => c.Matches).ToList();
            List<object> matchChamp = new List<object>();
            matchChamp.Add(new[] { "Чемпіонат", "Кількість матчів" });
            foreach (var p in champ)
                matchChamp.Add(new object[] { p.ChampionshipName, p.Matches.Count() });

            return new JsonResult(matchChamp);
        }

    }
}