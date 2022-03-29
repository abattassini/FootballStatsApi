namespace FootballStatsApi.Model
{
    public class GetGoalsOfEachMatchdayResponse
    {
        public List<MatchdayGoals>? MatchdayGoals { get; set; }
        public string? SeasonYear { get; set; }
    }

    public class MatchdayGoals
    {
        public int GoalsScoredHome { get; set; }
        public int GoalsScoredAway { get; set; }
        public int Matchday { get; set; }
    }
}
