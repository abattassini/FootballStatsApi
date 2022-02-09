using System;
using System.Collections.Generic;

namespace FootballStatsApi.Data.Entities
{
    public partial class Team
    {
        public Team()
        {
            MatchAwayTeams = new HashSet<Match>();
            MatchHomeTeams = new HashSet<Match>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageFile { get; set; }

        public virtual ICollection<Match> MatchAwayTeams { get; set; }
        public virtual ICollection<Match> MatchHomeTeams { get; set; }
    }
}
