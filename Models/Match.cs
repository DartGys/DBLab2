using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallWebLaba1.Models;

public partial class Match
{
    public int MatchId { get; set; }
    [Display(Name = "Дата матчу")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateTime MatchDate { get; set; }
    [Display(Name = "Тривалість")]
    [Range(90,125, ErrorMessage = "Тривалість матча від 90 до 120 хвилин + 5 хвилин додаткового часу")]
    public short MatchDuration { get; set; }
    [Display(Name = "Стадіон")]
    public int StaidumId { get; set; }
    [Display(Name = "Команда хазяїв")]
    public int HostClubId { get; set; }
    [Display(Name = "Команда гостей")]
    public int GuestClubId { get; set; }
    [Display(Name = "Чемпіонат")]
    public int ChampionshipId { get; set; }

    public virtual Championship Championship { get; set; }
    [Display(Name = "Команда гостей")]
    public virtual Club GuestClub { get; set; }
    [Display(Name = "Команда хазяїв")]
    public virtual Club HostClub { get; set; }

    public virtual ICollection<ScoredGoal> ScoredGoals { get; } = new List<ScoredGoal>();
    [Display(Name = "Стадіон")]
    public virtual Stadium Stadium { get; set; }
}
