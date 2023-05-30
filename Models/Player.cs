using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallWebLaba1.Models;

public partial class Player
{
    public int PlayerId { get; set; }
    [Display(Name = "Ім`я")]
    public string PlayerName { get; set; }
    [Display(Name = "Номер")]
    [Range(1,99, ErrorMessage = "Гравець може мати номер від 1 до 99")]
    public int PlayerNumber { get; set; }
    [Display(Name = "Позиція")]
    public int PositionId { get; set; }
    [Display(Name = "Зарплата")]
    [Range(100,300000000, ErrorMessage = "Запрлата варіюється від 100 до 300.000.000")]
    public decimal PlayerSalary { get; set; }
    [Display(Name = "Рік народження")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateTime PlayerBirthDate { get; set; }
    [Display(Name = "Команда")]
    public int ClubId { get; set; }
    [Display(Name = "Команда")]
    public virtual Club Club { get; set; }
    [Display(Name = "Позиція")]
    public virtual Position Position { get; set; }

    public virtual ICollection<ScoredGoal> ScoredGoals { get; } = new List<ScoredGoal>();
}
