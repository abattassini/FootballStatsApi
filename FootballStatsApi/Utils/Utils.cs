using FootballStatsApi.Domain;

namespace FootballStatsApi.Utils
{
    public static class Utils
    {
        public static Season GetSeasonByYear(string year) => year switch
        {
            "2018" => new Season { StartDate = new DateTime(2018, 04, 13), FinalDate = new DateTime(2018, 12, 3) },
            "2019" => new Season { StartDate = new DateTime(2019, 04, 26), FinalDate = new DateTime(2019, 12, 8) },
            "2020" => new Season { StartDate = new DateTime(2020, 08, 07), FinalDate = new DateTime(2021, 02, 26) },
            _ => throw new Exception(string.Format("Season for year {0} is not available.", year))
        };
    }
}
