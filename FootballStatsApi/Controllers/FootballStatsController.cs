using FootballStatsApi.Data.Context;
using FootballStatsApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;

namespace FootballStatsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FootballStatsController : ControllerBase
    {
        private readonly ILogger<FootballStatsController> _logger;

        public readonly FootballStatsContext _context;

        public FootballStatsController(ILogger<FootballStatsController> logger, FootballStatsContext context)
        {
            _logger = logger;
            _context = context;
        }

        [EnableCors("FootballStatsCorsPolicy")]
        [HttpGet("GetGoalsOfEachMatchday")]
        public GetGoalsOfEachMatchdayResponse GetGoalsOfEachMatchday(int seasonYear)
        {
            var seasonInfo = Utils.Utils.GetSeasonByYear(seasonYear.ToString());

            var matchdayGoals = _context.Matches
                       .Where(x => x.Date >= seasonInfo.StartDate && x.Date <= seasonInfo.FinalDate)
                       .GroupBy(x => x.Matchday)
                       .Select(x => new MatchdayGoals
                       {
                           GoalsScoredHome = x.Sum(y => y.HomeTeamGoals),
                           GoalsScoredAway = x.Sum(y => y.AwayTeamGoals),
                           Matchday = x.Key
                       }).ToList();

            var response = new GetGoalsOfEachMatchdayResponse
            {
                MatchdayGoals = matchdayGoals,
                SeasonYear = seasonYear.ToString()
            };

            return response;
        }

        [EnableCors("FootballStatsCorsPolicy")]
        [HttpGet("GetStatsEachTeamSeason")]
        public GetStatsEachTeamSeasonResponse GetStatsEachTeamSeason(int seasonYear)
        {
            var seasonInfo = Utils.Utils.GetSeasonByYear(seasonYear.ToString());


            var homeGoals = (from match in _context.Matches
                            where match.Date >= seasonInfo.StartDate && match.Date <= seasonInfo.FinalDate
                            group match by match.HomeTeamId into g
                            select new
                            {
                                TeamId = g.Key,
                                GoalsScored = g.Sum(x => x.HomeTeamGoals),
                                GoalsConceded = g.Sum(x => x.AwayTeamGoals),
                                Victories = g.Sum(x => x.HomeTeamGoals > x.AwayTeamGoals ? 1 : 0),
                                Ties = g.Sum(x => x.HomeTeamGoals == x.AwayTeamGoals ? 1 : 0),
                                Losses = g.Sum(x => x.HomeTeamGoals < x.AwayTeamGoals ? 1 : 0),

                            }).ToList();

            var awayGoals = (from match in _context.Matches
                            where match.Date >= seasonInfo.StartDate && match.Date <= new DateTime(seasonYear, 12, 30)
                            group match by match.AwayTeamId into g
                            select new
                            {
                                TeamId = g.Key,
                                GoalsScored = g.Sum(x => x.AwayTeamGoals),
                                GoalsConceded = g.Sum(x => x.HomeTeamGoals),
                                Victories = g.Sum(x => x.AwayTeamGoals > x.HomeTeamGoals ? 1 : 0),
                                Ties = g.Sum(x => x.HomeTeamGoals == x.AwayTeamGoals ? 1 : 0),
                                Losses = g.Sum(x => x.AwayTeamGoals < x.HomeTeamGoals ? 1 : 0),
                            }).ToList();


            var teamsGoals = homeGoals.Join(awayGoals,
                                            x => x.TeamId,
                                            y => y.TeamId,
                                            (x, y) => new TeamStatsSeason
                                            {
                                                TeamId = x.TeamId,
                                                GoalsScoredHome = x.GoalsScored,
                                                GoalsScoredAway = y.GoalsScored,
                                                GoalsScoredTotal = x.GoalsScored + y.GoalsScored,
                                                GoalsConcededHome = x.GoalsConceded,
                                                GoalsConcededAway = y.GoalsConceded,
                                                GoalsConcededTotal = x.GoalsConceded + y.GoalsConceded,
                                                VictoriesHome = x.Victories,
                                                VictoriesAway = y.Victories,
                                                VictoriesTotal = x.Victories + y.Victories,
                                                TiesHome = x.Ties,
                                                TiesAway = y.Ties,
                                                TiesTotal = x.Ties + y.Ties,
                                                LossesHome = x.Losses,
                                                LossesAway = y.Losses,
                                                LossesTotal = x.Losses + y.Losses
                                            });

            teamsGoals = teamsGoals.Join(_context.Teams,
                                     x => x.TeamId,
                                     y => y.Id,
                                     (x, y) => new TeamStatsSeason
                                     {
                                         TeamId = x.TeamId,
                                         TeamName = y.Name,
                                         GoalsScoredHome = x.GoalsScoredHome,
                                         GoalsScoredAway = x.GoalsScoredAway,
                                         GoalsScoredTotal = x.GoalsScoredTotal,
                                         GoalsConcededHome = x.GoalsConcededHome,
                                         GoalsConcededAway = x.GoalsConcededAway,
                                         GoalsConcededTotal = x.GoalsConcededTotal,
                                         VictoriesHome = x.VictoriesHome,
                                         VictoriesAway = x.VictoriesAway, 
                                         VictoriesTotal = x.VictoriesTotal,
                                         TiesHome = x.TiesHome,
                                         TiesAway = x.TiesAway,
                                         TiesTotal = x.TiesTotal,
                                         LossesHome = x.LossesHome,
                                         LossesAway = x.LossesAway,
                                         LossesTotal = x.LossesTotal,
                                     }); // TODO: Find a way to do this without having to rewrite all the object.

            var response = new GetStatsEachTeamSeasonResponse
            {
                StatsEachTeam = teamsGoals.OrderBy(o => o.TeamName).ToList(),
                SeasonYear = seasonYear.ToString()
            };

            return response;
        }

        [EnableCors("FootballStatsCorsPolicy")]
        [HttpGet("GetTeamsMatchdayScores")]
        public GetTeamsStatsResponse GetTeamsMatchdayScores(string seasonYear, int nextMatchday, int matchdaysToConsider)
        {
            var wholeSeasonStats = GetGroupOfMatchesStats(seasonYear, 1, nextMatchday - 1);
            var lastMatchesStats = GetGroupOfMatchesStats(seasonYear, nextMatchday - matchdaysToConsider, nextMatchday - 1);

            var seasonInfo = Utils.Utils.GetSeasonByYear(seasonYear);

            var nextMatchdayMatches = (from match in _context.Matches
                       where match.Matchday == nextMatchday && match.Date >= seasonInfo.StartDate && match.Date <= seasonInfo.FinalDate
                       select match).ToList();

            Dictionary<int, MatchdayScore> matchdayScores = new Dictionary<int, MatchdayScore>();

            List<double> scoringScores = new List<double>();

            foreach (var match in nextMatchdayMatches)
            {
                var homeTeamWholeSeasonStats = wholeSeasonStats.First(x => x.TeamId == match.HomeTeamId);
                var awayTeamWholeSeasonStats = wholeSeasonStats.First(x => x.TeamId == match.AwayTeamId);

                var homeTeamLastMatchesStats = lastMatchesStats.First(x => x.TeamId == match.HomeTeamId);
                var awayTeamLastMatchesStats = lastMatchesStats.First(x => x.TeamId == match.AwayTeamId);

                var homeTeamScoringScore = (homeTeamWholeSeasonStats.GoalsScored * awayTeamWholeSeasonStats.GoalsConceded * 0.5) / homeTeamWholeSeasonStats.MatchesQty
                                           + (homeTeamLastMatchesStats.GoalsScored * awayTeamWholeSeasonStats.GoalsConceded * 0.5) / homeTeamLastMatchesStats.MatchesQty;

                var awayTeamScoringScore = ((awayTeamWholeSeasonStats.GoalsScored * homeTeamWholeSeasonStats.GoalsConceded * 0.5) / awayTeamWholeSeasonStats.MatchesQty
                                           + (awayTeamLastMatchesStats.GoalsScored * homeTeamLastMatchesStats.GoalsConceded * 0.5) / awayTeamLastMatchesStats.MatchesQty) * 0.65;

                scoringScores.Add((homeTeamScoringScore != null ? (double)homeTeamScoringScore : 0));
                scoringScores.Add((homeTeamScoringScore != null ? (double)homeTeamScoringScore : 0));

                if (!matchdayScores.ContainsKey(match.HomeTeamId))
                {
                    matchdayScores.Add(match.HomeTeamId, new MatchdayScore
                    {
                        TeamId = match.HomeTeamId,
                        TeamName = homeTeamWholeSeasonStats.TeamName,
                        ScoringScore = (homeTeamScoringScore != null ? (double)homeTeamScoringScore : -1),
                        ConcedingScore = (awayTeamScoringScore != null ? (double)awayTeamScoringScore : -1),
                        GoalsScoredWholeSeason = homeTeamWholeSeasonStats.GoalsScored != null ? Convert.ToInt32(homeTeamWholeSeasonStats.GoalsScored) : 0,
                        GoalsConcededWholeSeason = homeTeamWholeSeasonStats.GoalsConceded != null ? Convert.ToInt32(homeTeamWholeSeasonStats.GoalsConceded) : 0,
                        GoalsScoredRecently = homeTeamLastMatchesStats.GoalsScored != null ? Convert.ToInt32(homeTeamLastMatchesStats.GoalsScored) : 0,
                        GoalsConcededRecently = homeTeamLastMatchesStats.GoalsConceded != null ? Convert.ToInt32(homeTeamLastMatchesStats.GoalsConceded) : 0,
                        OpponentId = match.AwayTeamId,
                        PlaysAtHome = true,                      
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
                        GoalsScoredWholeSeason = awayTeamWholeSeasonStats.GoalsScored != null ? Convert.ToInt32(awayTeamWholeSeasonStats.GoalsScored) : 0,
                        GoalsConcededWholeSeason = awayTeamWholeSeasonStats.GoalsConceded != null ? Convert.ToInt32(awayTeamWholeSeasonStats.GoalsConceded) : 0,
                        GoalsScoredRecently = awayTeamLastMatchesStats.GoalsScored != null ? Convert.ToInt32(awayTeamLastMatchesStats.GoalsScored) : 0,
                        GoalsConcededRecently = awayTeamLastMatchesStats.GoalsConceded != null ? Convert.ToInt32(awayTeamLastMatchesStats.GoalsConceded) : 0,
                        OpponentId = match.HomeTeamId,
                        PlaysAtHome = false,
                    });
                }

                if (matchdayScores.Count >= 20)
                {
                    break;
                }
            }

            var matchdayScoresList = matchdayScores.Values.ToList();

            foreach (MatchdayScore matchdayScore in matchdayScoresList)
            {
                matchdayScore.ScoringScore = matchdayScore.ScoringScore * (100 / scoringScores.Max());
                matchdayScore.ConcedingScore = matchdayScore.ConcedingScore * (100 / scoringScores.Max());
            }

            return new GetTeamsStatsResponse
            {
                MatchdayScores = matchdayScoresList,
                Matchday = nextMatchday.ToString(),
                Season = seasonYear,
            };
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