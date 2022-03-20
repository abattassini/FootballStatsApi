namespace FootballStatsApi.Model
{
    public class GroupOfMatchesStats
    {
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public int? GoalsScored { get; set; }
        public int? GoalsConceded { get; set; }
        public int? MatchesQty { get; set; }
        public int? MatchdayStart { get; set; }
        public int? MatchdayEnd { get; set; }
        public int? SeasonYear { get; set; }
        public int? Victories { get; set; }
        public int? Ties { get; set; }
        public int? Losses { get; set; }

    }
}
