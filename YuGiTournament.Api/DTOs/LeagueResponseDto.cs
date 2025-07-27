using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.DTOs
{
    public class LeagueResponseDto
    {
        public int LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public string LeagueDescription { get; set; } = string.Empty;
        public LeagueType LeagueType { get; set; }
        public SystemOfLeague SystemOfLeague { get; set; }
        public bool IsFinished { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<PlayerRankDto>? Players { get; set; }
        public List<GroupDto>? Groups { get; set; }
        public List<MatchDto>? Matches { get; set; }
        public List<MatchDto>? KnockoutMatches { get; set; }
    }

    public class PlayerRankDto
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double Points { get; set; }
        public int MatchesPlayed { get; set; }
        public int Rank { get; set; }
        public double WinRate { get; set; }
    }

    public class GroupDto
    {
        public int GroupNumber { get; set; }
        public List<PlayerRankDto> Players { get; set; } = [];
        public List<MatchDto> Matches { get; set; } = [];
    }

    public class MatchDto
    {
        public int MatchId { get; set; }
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public bool IsCompleted { get; set; }
        public string? Player1Name { get; set; }
        public string? Player2Name { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public TournamentStage TournamentStage { get; set; }
        public int? WinnerId { get; set; }
    }
}