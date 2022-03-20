namespace FootballStatsApi.Model
{
    public class GroupOfMatchesPercentages
    {
        public int GoalsScoredHome { get; set; }
        public int GoalsScoredAway { get; set; }
        public double HomeScoringPercentage { get; set; }
        public double AwayScoringPercentage { get; set; }
        public int MatchesConsidered { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
    }
}
