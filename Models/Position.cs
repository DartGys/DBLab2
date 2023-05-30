using System;
using System.Collections.Generic;

namespace FootBallWebLaba1.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; }

    public virtual ICollection<Player> Players { get; } = new List<Player>();
}
