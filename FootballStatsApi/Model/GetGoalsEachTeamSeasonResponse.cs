namespace FootballStatsApi.Model
{
    public class GetStatsEachTeamSeasonResponse
    {
        public List<TeamStatsSeason>? StatsEachTeam { get; set; }
        public string? SeasonYear { get; set; }
    }

    public class TeamStatsSeason
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public int GoalsScoredHome { get; set; }
        public int VictoriesHome { get; set; }
        public int VictoriesAway { get; set; }
        public int VictoriesTotal { get; set; }
        public int LossesHome { get; set; }
        public int LossesAway { get; set; }
        public int LossesTotal { get; set; }
        public int TiesHome { get; set; }
        public int TiesAway { get; set; }
        public int TiesTotal { get; set; }
        public int GoalsScoredAway { get; set; }
        public int GoalsScoredTotal { get; set; }
        public int GoalsConcededHome { get; set; }
        public int GoalsConcededAway { get; set; }
        public int GoalsConcededTotal { get; set; }
    }
}
