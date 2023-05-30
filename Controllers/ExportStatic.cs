using FootBallWebLaba1.Models;

namespace FootBallWebLaba1.Controllers
{
    public class ExportStatic
    {
        public static List<Club> Clubs = new List<Club>();
        public static List<Match> Matches = new List<Match>();
        public static List<Championship> Championships = new List<Championship>();
        public static List<Player> Players = new List<Player>();

        public static void clubSet(List<Club> objects)
        {
            Clubs.Clear();
            foreach (var obj in objects)
            {
                Clubs.Add(obj);
            }
        }
        public static void matchSet(List<Match> objects)
        {
            Matches.Clear();
            foreach (var obj in objects)
            {
                Matches.Add(obj);
            }
        }
        public static void championshipSet(List<Championship> objects)
        {
            Championships.Clear();
            foreach (var obj in objects)
            {
                Championships.Add(obj);
            }
        }
        public static void playerSet(List<Player> objects)
        {
            Players.Clear();
            foreach (var obj in objects)
            {
                Players.Add(obj);
            }
        }
    }
}
