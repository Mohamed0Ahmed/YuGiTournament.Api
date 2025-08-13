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
    public record MultiMatchResultDto(
        int? WinnerId = null,        // For Classic system
        double? Score1 = null,       // For Points system
        double? Score2 = null        // For Points system
    );
    public record PlayerReplaceDto(int ReplacedPlayerId, int NewPlayerId);

    // Player DTOs
    public record AddPlayerDto(string FullName);
}


