using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.DTOs
{
    // Tournament DTOs
    public record CreateTournamentDto(string Name, SystemOfLeague SystemOfScoring, int TeamCount, int PlayersPerTeam);
    public record UpdateTournamentStatusDto(TournamentStatus Status);

    // Team DTOs
    public record TeamCreateDto(string TeamName, List<int> PlayerIds);
    public record TeamUpdateDto(string? TeamName, List<int>? PlayerIds);

    // Match DTOs
    public record MultiMatchResultDto(int? Score1, int? Score2, double? TotalPoints1, double? TotalPoints2);
    public record PlayerReplaceDto(int ReplacedPlayerId, int NewPlayerId);
}


