using FootballStatsApi.Data.Context;
using FootballStatsApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballStatsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FootballStatsController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<FootballStatsController> _logger;

        public readonly FootballStatsContext _context;

        public FootballStatsController(ILogger<FootballStatsController> logger, FootballStatsContext context)
        {
            _logger = logger;
            _context = context;
        }

        //[HttpGet(Name = "GetTeams")]
        //public IEnumerable<string> GetTeams()
        //{
        //    var teams = _context.Teams.Select(s => s.Name).ToList();
        //    return teams;
        //}

        [HttpGet(Name = "GetTest")]
        public Dictionary<int, MatchdayScore> GetTable(string seasonYear, int nextMatchday, int matchdaysToConsider)
        {
            var wholeSeasonStats = GetGroupOfMatchesStats(seasonYear, 1, nextMatchday - 1);
            var lastMatchesStats = GetGroupOfMatchesStats(seasonYear, nextMatchday - matchdaysToConsider, nextMatchday - 1);

            var seasonInfo = Utils.Utils.GetSeasonByYear(seasonYear);

            var nextMatchdayMatches = (from match in _context.Matches
                       where match.Matchday == nextMatchday && match.Date >= seasonInfo.StartDate && match.Date <= seasonInfo.FinalDate
                       select match).ToList();

            Dictionary<int, MatchdayScore> matchdayScores = new Dictionary<int, MatchdayScore>();

            foreach (var match in nextMatchdayMatches)
            {
                var homeTeamWholeSeasonStats = wholeSeasonStats.First(x => x.TeamId == match.HomeTeamId);
                var awayTeamWholeSeasonStats = wholeSeasonStats.First(x => x.TeamId == match.AwayTeamId);

                var homeTeamLastMatchesStats = lastMatchesStats.First(x => x.TeamId == match.HomeTeamId);
                var awayTeamLastMatchesStats = lastMatchesStats.First(x => x.TeamId == match.AwayTeamId);

                var homeTeamScoringScore = (homeTeamWholeSeasonStats.GoalsScored * awayTeamWholeSeasonStats.GoalsConceded * 0.5) / (nextMatchday - 1)
                                           + (homeTeamLastMatchesStats.GoalsScored * awayTeamWholeSeasonStats.GoalsConceded * 0.5) / matchdaysToConsider;

                var awayTeamScoringScore = ((awayTeamWholeSeasonStats.GoalsScored * homeTeamWholeSeasonStats.GoalsConceded * 0.5) / (nextMatchday - 1)
                                           + (awayTeamLastMatchesStats.GoalsScored * homeTeamLastMatchesStats.GoalsConceded * 0.5) / matchdaysToConsider) * 0.9;

                if (!matchdayScores.ContainsKey(match.HomeTeamId))
                {
                    matchdayScores.Add(match.HomeTeamId, new MatchdayScore
                    {
                        TeamId = match.HomeTeamId,
                        TeamName = homeTeamWholeSeasonStats.TeamName,
                        ScoringScore = (homeTeamScoringScore != null ? (double)homeTeamScoringScore : -1),
                        ConcedingScore = (awayTeamScoringScore != null ? (double)awayTeamScoringScore : -1),
                    });
                }

                if (!matchdayScores.ContainsKey(match.AwayTeamId))
                {
                    matchdayScores.Add(match.AwayTeamId, new MatchdayScore
                    {
                        TeamId = match.AwayTeamId,
                        TeamName = awayTeamWholeSeasonStats.TeamName,
                        ScoringScore = (awayTeamScoringScore != null ? (double)awayTeamScoringScore : -1),
                        ConcedingScore = (homeTeamScoringScore != null ? (double)homeTeamScoringScore : -1),
                    });
                }

                if (matchdayScores.Count >= 20)
                {
                    break;
                }
            }

            return matchdayScores;
        }

        private IQueryable<GroupOfMatchesStats> GetGroupOfMatchesStats(string seasonYear, int startMatchday, int finalMatchday)
        {
            var seasonInfo = Utils.Utils.GetSeasonByYear(seasonYear);

            var homeMatchesResults = from match in _context.Matches
                                     where match.Matchday >= startMatchday && match.Matchday <= finalMatchday &&
                                           match.Date >= seasonInfo.StartDate && match.Date <= seasonInfo.FinalDate
                                     group match by match.HomeTeamId into g
                                     select new GroupOfMatchesStats
                                     {
                                         TeamId = g.Key,
                                         GoalsScored = g.Sum(x => x.HomeTeamGoals),
                                         GoalsConceded = g.Sum(x => x.AwayTeamGoals),
                                         Victories = g.Sum(x => x.HomeTeamGoals > x.AwayTeamGoals ? 1 : 0),
                                         Ties = g.Sum(x => x.HomeTeamGoals == x.AwayTeamGoals ? 1 : 0),
                                         Losses = g.Sum(x => x.HomeTeamGoals < x.AwayTeamGoals ? 1 : 0),
                                         MatchesQty = g.Count()
                                     };

            var awayMatchesResults = from match in _context.Matches
                                     where match.Matchday >= startMatchday && match.Matchday <= finalMatchday &&
                                           match.Date >= seasonInfo.StartDate && match.Date <= seasonInfo.FinalDate
                                     group match by match.AwayTeamId into g
                                     select new GroupOfMatchesStats
                                     {
                                         TeamId = g.Key,
                                         GoalsScored = g.Sum(x => x.AwayTeamGoals),
                                         GoalsConceded = g.Sum(x => x.HomeTeamGoals),
                                         Victories = g.Sum(x => x.AwayTeamGoals > x.HomeTeamGoals ? 1 : 0),
                                         Ties = g.Sum(x => x.AwayTeamGoals == x.HomeTeamGoals ? 1 : 0),
                                         Losses = g.Sum(x => x.AwayTeamGoals < x.HomeTeamGoals ? 1 : 0),
                                         MatchesQty = g.Count()
                                     };


            var matchesResults = homeMatchesResults.Join(awayMatchesResults,
                                                         x => x.TeamId,
                                                         y => y.TeamId,
                                                         (x, y) => new GroupOfMatchesStats
                                                         {
                                                             TeamId = x.TeamId,
                                                             GoalsScored = x.GoalsScored + y.GoalsScored,
                                                             GoalsConceded = x.GoalsConceded + y.GoalsConceded,
                                                             Victories = x.Victories + y.Victories,
                                                             Ties = x.Ties + y.Ties,
                                                             Losses = x.Losses + y.Losses,
                                                             MatchesQty = x.MatchesQty + y.MatchesQty
                                                         });

            matchesResults = matchesResults.Join(_context.Teams,
                                                 x => x.TeamId,
                                                 y => y.Id,
                                                 (x, y) => new GroupOfMatchesStats
                                                 {
                                                     TeamId = x.TeamId,
                                                     TeamName = y.Name,
                                                     GoalsScored = x.GoalsScored,
                                                     GoalsConceded = x.GoalsConceded,
                                                     Victories = x.Victories,
                                                     Ties = x.Ties,
                                                     Losses = x.Losses,
                                                     MatchdayStart = startMatchday,
                                                     MatchdayEnd = finalMatchday,
                                                     SeasonYear = Convert.ToInt32(seasonYear),
                                                     MatchesQty = x.MatchesQty
                                                 });

            matchesResults.ToQueryString();

            return matchesResults;
        }
    }
}