using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.DTOs
{
    public class StartLeagueDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LeagueType TypeOfLeague { get; set; }
    }
}
