using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallWebLaba1.Models;

public partial class Stadium
{
    public int StadiumId { get; set; }
    [Display(Name = "Локація")]
    public string StadiumLocation { get; set; }
    [Display(Name = "Місткість")]
    [Range(1000,50000, ErrorMessage = "Стадіон може вміщувати від 1000 до 50000 вболівальників")]
    public int StadiumCapacity { get; set; }
    [Display(Name = "Дата заснування")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateTime StadiumEstablismentDate { get; set; }

    public int ClubId { get; set; }
    [Display(Name = "Команда власник")]
    public virtual Club Club { get; set; }

    public virtual ICollection<Match> Matches { get; } = new List<Match>();
}
