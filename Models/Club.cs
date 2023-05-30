using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallWebLaba1.Models;

public partial class Club
{
    public int ClubId { get; set; }
    [Display(Name = "Назва")]
    public string ClubName { get; set; }
    [Display(Name = "Походженя")]
    public string ClubOrigin { get; set; }
    [Display(Name = "Кількість гравців")]
    public int ClubPlayerQuantity { get; set; }
    [Display(Name = "Тренер")]
    public string ClubCoachName { get; set; }
    [Display(Name = "Дата заснування")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateTime ClubEstablishmentDate { get; set; }

    public virtual ICollection<Match> MatchGuestClubs { get; } = new List<Match>();

    public virtual ICollection<Match> MatchHostClubs { get; } = new List<Match>();

    public virtual ICollection<Player> Players { get; } = new List<Player>();

    public virtual ICollection<Stadium> Stadiums { get; } = new List<Stadium>();
}
