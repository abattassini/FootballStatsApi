namespace FootballStatsApi.Model
{
    public class GetTeamsStatsResponse
    {
        public List<MatchdayScore>? MatchdayScores { get; set; }
        public string? Season { get; set; }
        public string? Matchday { get; set; }
    }
}
