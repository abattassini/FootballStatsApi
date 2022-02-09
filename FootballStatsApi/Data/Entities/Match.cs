using System;
using System.Collections.Generic;

namespace FootballStatsApi.Data.Entities
{
    public partial class Match
    {
        public short Id { get; set; }
        public byte Matchday { get; set; }
        public DateTime Date { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int HomeTeamGoals { get; set; }
        public int AwayTeamGoals { get; set; }

        public virtual Team AwayTeam { get; set; } = null!;
        public virtual Team HomeTeam { get; set; } = null!;
    }
}
