namespace FootballStatsApi.Model
{
    public class MatchdayScore
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public double ScoringScore { get; set; }
        public double ConcedingScore { get; set; }
    }
}
