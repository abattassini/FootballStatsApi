namespace FootballStatsApi.Model
{
    public class MatchdayScore
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public double ScoringScore { get; set; }
        public double ConcedingScore { get; set; }
        public int GoalsScoredWholeSeason { get; set; }
        public int GoalsConcededWholeSeason { get; set; }
        public int GoalsScoredRecently { get; set; }
        public int GoalsConcededRecently { get; set; }
        public int OpponentId { get; set; }
        public bool PlaysAtHome { get; set; }
    }
}
