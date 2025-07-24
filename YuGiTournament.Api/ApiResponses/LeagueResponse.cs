using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.ApiResponses
{
    public class LeagueResponse
    {
        public ApiResponse Response { get; set; } = default!;
        public LeagueId? League
        {
            get; set;
        }
        public SystemOfLeague? SystemOfLeague => League?.SystemOfLeague;
    }
}
