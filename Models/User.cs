using Microsoft.AspNetCore.Identity;

namespace FootBallWebLaba1.Models
{
    public class User : IdentityUser
    {
        public int Year { get; set; }
    }
}
