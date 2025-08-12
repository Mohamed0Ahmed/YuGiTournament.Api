using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class MultiTournamentService : IMultiTournamentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameRulesService _gameRulesService;

        public MultiTournamentService(IUnitOfWork unitOfWork, IGameRulesService gameRulesService)
        {
            _unitOfWork = unitOfWork;
            _gameRulesService = gameRulesService;
        }

        #region Tournament Management

        public async Task<ApiResponse> CreateTournamentAsync(string name, SystemOfLeague systemOfScoring, int teamCount, int playersPerTeam)
        {
            if (teamCount < 2 || playersPerTeam < 1)
                return new ApiResponse(false, "Invalid teamCount or playersPerTeam");

            // Check for active tournament
            var hasActive = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll().AnyAsync(t => t.IsActive);
            if (hasActive)
                return new ApiResponse(false, "Another tournament is already active");

            var tournament = new MultiTournament
            {
                Name = name,
                SystemOfScoring = systemOfScoring,
                TeamCount = teamCount,
                PlayersPerTeam = playersPerTeam,
                Status = TournamentStatus.Created,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<MultiTournament>().AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Tournament created successfully", tournament);
        }

        public async Task<ApiResponse> UpdateTournamentStatusAsync(int tournamentId, TournamentStatus status)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            // Validation based on status transition
            switch (status)
            {
                case TournamentStatus.Started:
                    if (tournament.Status != TournamentStatus.Created)
                        return new ApiResponse(false, "Can only start a created tournament");

                    // Validate teams and generate matches
                    var validateResult = await ValidateAndGenerateMatchesAsync(tournament);
                    if (!validateResult.Success)
                        return validateResult;

                    tournament.StartedOn = DateTime.UtcNow;
                    break;

                case TournamentStatus.Finished:
                    if (tournament.Status != TournamentStatus.Started)
                        return new ApiResponse(false, "Can only finish a started tournament");

                    // Update player statistics
                    await UpdatePlayerStatisticsAsync(tournament);
                    tournament.FinishedOn = DateTime.UtcNow;
                    tournament.IsActive = false;
                    break;
            }

            tournament.Status = status;
            _unitOfWork.GetRepository<MultiTournament>().Update(tournament);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Tournament status updated successfully");
        }

        public async Task<ApiResponse> DeleteTournamentAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            // Delete all related entities
            if (tournament.Matches.Any())
            {
                _unitOfWork.GetRepository<MultiMatch>().DeleteRange(tournament.Matches);
            }

            if (tournament.Teams.Any())
            {
                _unitOfWork.GetRepository<MultiTeam>().DeleteRange(tournament.Teams);
            }

            _unitOfWork.GetRepository<MultiTournament>().Delete(tournament);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Tournament deleted successfully");
        }

        #endregion

        #region Team Management

        public async Task<ApiResponse> CreateTeamAsync(int tournamentId, string teamName, List<int> playerIds)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            if (tournament.Status != TournamentStatus.Created)
                return new ApiResponse(false, "Cannot add teams after tournament is started");

            if (tournament.Teams.Count >= tournament.TeamCount)
                return new ApiResponse(false, "Team limit reached");

            if (playerIds.Count != tournament.PlayersPerTeam)
                return new ApiResponse(false, $"Team must have exactly {tournament.PlayersPerTeam} players");

            // Check for duplicate team name
            if (tournament.Teams.Any(t => t.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase)))
                return new ApiResponse(false, "Team name already exists");

            // Check for duplicate players in same tournament
            var existingPlayerIds = tournament.Teams
                .SelectMany(t => JsonSerializer.Deserialize<List<int>>(t.PlayerIds) ?? new List<int>())
                .ToHashSet();

            if (playerIds.Any(p => existingPlayerIds.Contains(p)))
                return new ApiResponse(false, "One or more players are already in another team");

            var team = new MultiTeam
            {
                MultiTournamentId = tournamentId,
                TeamName = teamName,
                PlayerIds = JsonSerializer.Serialize(playerIds),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<MultiTeam>().AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Team created successfully", new
            {
                team.MultiTeamId,
                team.TeamName,
                PlayerIds = playerIds,
                team.CreatedOn
            });
        }

        public async Task<ApiResponse> UpdateTeamAsync(int teamId, string? teamName, List<int>? playerIds)
        {
            var team = await _unitOfWork.GetRepository<MultiTeam>()
                .GetAll()
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.MultiTeamId == teamId);

            if (team == null)
                return new ApiResponse(false, "Team not found");

            if (team.Tournament!.Status != TournamentStatus.Created)
                return new ApiResponse(false, "Cannot update team after tournament is started");

            if (!string.IsNullOrEmpty(teamName))
            {
                // Check for duplicate team name
                var duplicateName = await _unitOfWork.GetRepository<MultiTeam>()
                    .GetAll()
                    .AnyAsync(t => t.MultiTournamentId == team.MultiTournamentId &&
                                   t.MultiTeamId != teamId &&
                                   t.TeamName.ToLower() == teamName.ToLower());

                if (duplicateName)
                    return new ApiResponse(false, "Team name already exists");

                team.TeamName = teamName;
            }

            if (playerIds != null)
            {
                if (playerIds.Count != team.Tournament.PlayersPerTeam)
                    return new ApiResponse(false, $"Team must have exactly {team.Tournament.PlayersPerTeam} players");

                // Check for duplicate players in same tournament (excluding current team)
                var otherTeams = await _unitOfWork.GetRepository<MultiTeam>()
                    .GetAll()
                    .Where(t => t.MultiTournamentId == team.MultiTournamentId && t.MultiTeamId != teamId)
                    .ToListAsync();

                var existingPlayerIds = otherTeams
                    .SelectMany(t => JsonSerializer.Deserialize<List<int>>(t.PlayerIds) ?? new List<int>())
                    .ToHashSet();

                if (playerIds.Any(p => existingPlayerIds.Contains(p)))
                    return new ApiResponse(false, "One or more players are already in another team");

                team.PlayerIds = JsonSerializer.Serialize(playerIds);
            }

            _unitOfWork.GetRepository<MultiTeam>().Update(team);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Team updated successfully");
        }

        public async Task<ApiResponse> ReplacePlayerAsync(int teamId, int replacedPlayerId, int newPlayerId)
        {
            var team = await _unitOfWork.GetRepository<MultiTeam>()
                .GetAll()
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.MultiTeamId == teamId);

            if (team == null)
                return new ApiResponse(false, "Team not found");

            if (team.Tournament!.Status != TournamentStatus.Started)
                return new ApiResponse(false, "Player replacement only allowed after tournament start");

            var currentPlayerIds = JsonSerializer.Deserialize<List<int>>(team.PlayerIds) ?? new List<int>();

            if (!currentPlayerIds.Contains(replacedPlayerId))
                return new ApiResponse(false, "Player not found in team");

            // Check if new player is already in tournament
            var allTeams = await _unitOfWork.GetRepository<MultiTeam>()
                .GetAll()
                .Where(t => t.MultiTournamentId == team.MultiTournamentId)
                .ToListAsync();

            var allPlayerIds = allTeams
                .SelectMany(t => JsonSerializer.Deserialize<List<int>>(t.PlayerIds) ?? new List<int>())
                .ToHashSet();

            if (allPlayerIds.Contains(newPlayerId))
                return new ApiResponse(false, "New player is already in tournament");

            // Replace player in team
            var index = currentPlayerIds.IndexOf(replacedPlayerId);
            currentPlayerIds[index] = newPlayerId;
            team.PlayerIds = JsonSerializer.Serialize(currentPlayerIds);

            // Update uncompleted matches
            var matches = await _unitOfWork.GetRepository<MultiMatch>()
                .GetAll()
                .Where(m => m.MultiTournamentId == team.MultiTournamentId &&
                           !m.IsCompleted &&
                           (m.Player1Id == replacedPlayerId || m.Player2Id == replacedPlayerId))
                .ToListAsync();

            foreach (var match in matches)
            {
                if (match.Player1Id == replacedPlayerId) match.Player1Id = newPlayerId;
                if (match.Player2Id == replacedPlayerId) match.Player2Id = newPlayerId;
            }

            _unitOfWork.GetRepository<MultiTeam>().Update(team);
            _unitOfWork.GetRepository<MultiMatch>().UpdateRange(matches);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Player replaced successfully");
        }

        #endregion

        #region Match Management

        public async Task<ApiResponse> RecordMatchResultAsync(int matchId, int? score1, int? score2, double? totalPoints1, double? totalPoints2)
        {
            var match = await _unitOfWork.GetRepository<MultiMatch>()
                .GetAll()
                .Include(m => m.Tournament)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.MultiMatchId == matchId);

            if (match == null)
                return new ApiResponse(false, "Match not found");

            if (match.Tournament!.Status != TournamentStatus.Started)
                return new ApiResponse(false, "Cannot record results for non-started tournament");

            if (match.IsCompleted)
                return new ApiResponse(false, "Match already completed");

            // Validate and set scores based on scoring system
            if (match.Tournament.SystemOfScoring == SystemOfLeague.Classic)
            {
                if (!score1.HasValue || !score2.HasValue)
                    return new ApiResponse(false, "Scores required for Classic system");

                if (!IsValidClassicScore(score1.Value, score2.Value))
                    return new ApiResponse(false, "Invalid classic score (must be 3-0, 0-3, or 1-1)");

                match.Score1 = score1;
                match.Score2 = score2;
                match.TotalPoints1 = null;
                match.TotalPoints2 = null;
            }
            else // Points system
            {
                if (!totalPoints1.HasValue || !totalPoints2.HasValue)
                    return new ApiResponse(false, "Total points required for Points system");

                var maxRounds = await _gameRulesService.GetMaxRoundsPerMatchAsync();
                if (totalPoints1 < 0 || totalPoints2 < 0 || totalPoints1 + totalPoints2 > maxRounds)
                    return new ApiResponse(false, "Invalid points total");

                match.TotalPoints1 = totalPoints1;
                match.TotalPoints2 = totalPoints2;
                match.Score1 = null;
                match.Score2 = null;
            }

            match.IsCompleted = true;
            match.CompletedOn = DateTime.UtcNow;

            // Update team statistics
            await UpdateTeamStatisticsAsync(match);

            _unitOfWork.GetRepository<MultiMatch>().Update(match);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Match result recorded successfully");
        }

        #endregion

        #region Data Retrieval

        public async Task<ApiResponse> GetActiveTournamentAsync()
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.IsActive);

            if (tournament == null)
                return new ApiResponse(false, "No active tournament");

            return new ApiResponse(true, "Active tournament found", await BuildTournamentDetailsAsync(tournament));
        }

        public async Task<ApiResponse> GetTournamentByIdAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .Include(t => t.ChampionTeam)
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            return new ApiResponse(true, "Tournament found", await BuildTournamentDetailsAsync(tournament));
        }

        public async Task<ApiResponse> GetAllTournamentsAsync()
        {
            var tournaments = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .Include(t => t.ChampionTeam)
                .OrderByDescending(t => t.CreatedOn)
                .ToListAsync();

            var result = new List<object>();
            foreach (var tournament in tournaments)
            {
                result.Add(await BuildTournamentDetailsAsync(tournament));
            }

            return new ApiResponse(true, "Tournaments retrieved successfully", result);
        }

        public async Task<ApiResponse> GetTournamentMatchesAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            var matches = await _unitOfWork.GetRepository<MultiMatch>()
                .GetAll()
                .Where(m => m.MultiTournamentId == tournamentId)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .OrderBy(m => m.Team1Id)
                .ThenBy(m => m.Team2Id)
                .ToListAsync();

            // Group matches by team fixture for easier frontend consumption
            var fixtureGroups = matches
                .GroupBy(m => new { m.Team1Id, m.Team2Id })
                .Select(g => new
                {
                    Team1Id = g.Key.Team1Id,
                    Team1Name = g.First().Team1?.TeamName ?? "",
                    Team2Id = g.Key.Team2Id,
                    Team2Name = g.First().Team2?.TeamName ?? "",
                    Matches = g.Select(m => new
                    {
                        m.MultiMatchId,
                        m.Player1Id,
                        Player1Name = m.Player1?.FullName ?? $"Player {m.Player1Id}",
                        m.Player2Id,
                        Player2Name = m.Player2?.FullName ?? $"Player {m.Player2Id}",
                        m.Score1,
                        m.Score2,
                        m.TotalPoints1,
                        m.TotalPoints2,
                        m.IsCompleted,
                        m.CompletedOn
                    }).ToList()
                })
                .ToList();

            return new ApiResponse(true, "Tournament matches retrieved successfully", fixtureGroups);
        }

        public async Task<ApiResponse> GetTournamentStandingsAsync(int tournamentId)
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.MultiTournamentId == tournamentId);

            if (tournament == null)
                return new ApiResponse(false, "Tournament not found");

            var standings = tournament.Teams
                .OrderByDescending(t => t.TotalPoints)
                .ThenByDescending(t => t.Wins)
                .ThenBy(t => t.Losses)
                .Select((team, index) => new
                {
                    Position = index + 1,
                    team.MultiTeamId,
                    team.TeamName,
                    team.TotalPoints,
                    team.Wins,
                    team.Draws,
                    team.Losses,
                    MatchesPlayed = team.Wins + team.Draws + team.Losses
                })
                .ToList();

            return new ApiResponse(true, "Tournament standings retrieved successfully", new
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                Status = tournament.Status.ToString(),
                ChampionTeamId = tournament.ChampionTeamId,
                Standings = standings
            });
        }

        #endregion

        #region Helper Methods

        private async Task<ApiResponse> ValidateAndGenerateMatchesAsync(MultiTournament tournament)
        {
            // Validate teams count
            if (tournament.Teams.Count != tournament.TeamCount)
                return new ApiResponse(false, $"Expected {tournament.TeamCount} teams, but found {tournament.Teams.Count}");

            // Validate players per team
            foreach (var team in tournament.Teams)
            {
                var playerIds = JsonSerializer.Deserialize<List<int>>(team.PlayerIds) ?? new List<int>();
                if (playerIds.Count != tournament.PlayersPerTeam)
                    return new ApiResponse(false, $"Team '{team.TeamName}' has {playerIds.Count} players, expected {tournament.PlayersPerTeam}");
            }

            // Generate Round-Robin matches
            var teams = tournament.Teams.ToList();
            var matches = new List<MultiMatch>();

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    var team1 = teams[i];
                    var team2 = teams[j];

                    var team1Players = JsonSerializer.Deserialize<List<int>>(team1.PlayerIds) ?? new List<int>();
                    var team2Players = JsonSerializer.Deserialize<List<int>>(team2.PlayerIds) ?? new List<int>();

                    // Every player from team1 plays every player from team2
                    foreach (var player1Id in team1Players)
                    {
                        foreach (var player2Id in team2Players)
                        {
                            matches.Add(new MultiMatch
                            {
                                MultiTournamentId = tournament.MultiTournamentId,
                                Team1Id = team1.MultiTeamId,
                                Team2Id = team2.MultiTeamId,
                                Player1Id = player1Id,
                                Player2Id = player2Id,
                                IsCompleted = false
                            });
                        }
                    }
                }
            }

            await _unitOfWork.GetRepository<MultiMatch>().AddRangeAsync(matches);
            return new ApiResponse(true, "Matches generated successfully");
        }

        private Task UpdateTeamStatisticsAsync(MultiMatch match)
        {
            var team1 = match.Team1!;
            var team2 = match.Team2!;

            if (match.Tournament!.SystemOfScoring == SystemOfLeague.Classic)
            {
                var score1 = match.Score1!.Value;
                var score2 = match.Score2!.Value;

                team1.TotalPoints += score1;
                team2.TotalPoints += score2;

                if (score1 == 3) { team1.Wins++; team2.Losses++; }
                else if (score2 == 3) { team2.Wins++; team1.Losses++; }
                else { team1.Draws++; team2.Draws++; }
            }
            else // Points system
            {
                var points1 = match.TotalPoints1!.Value;
                var points2 = match.TotalPoints2!.Value;

                team1.TotalPoints += points1;
                team2.TotalPoints += points2;

                // For Points system, we don't track wins/draws/losses at team level
                // since it's about accumulated points
            }

            _unitOfWork.GetRepository<MultiTeam>().Update(team1);
            _unitOfWork.GetRepository<MultiTeam>().Update(team2);

            return Task.CompletedTask;
        }

        private async Task UpdatePlayerStatisticsAsync(MultiTournament tournament)
        {
            // Update FriendlyPlayer statistics
            var allPlayerIds = tournament.Teams
                .SelectMany(t => JsonSerializer.Deserialize<List<int>>(t.PlayerIds) ?? new List<int>())
                .Distinct()
                .ToList();

            var players = await _unitOfWork.GetRepository<FriendlyPlayer>()
                .GetAll()
                .Where(p => allPlayerIds.Contains(p.PlayerId))
                .ToListAsync();

            foreach (var player in players)
            {
                player.MultiParticipations++;
            }

            // Update champion team's players
            if (tournament.ChampionTeamId.HasValue)
            {
                var championTeam = tournament.Teams.First(t => t.MultiTeamId == tournament.ChampionTeamId);
                var championPlayerIds = JsonSerializer.Deserialize<List<int>>(championTeam.PlayerIds) ?? new List<int>();

                var championPlayers = players.Where(p => championPlayerIds.Contains(p.PlayerId));
                foreach (var player in championPlayers)
                {
                    player.MultiTitlesWon++;
                }
            }

            _unitOfWork.GetRepository<FriendlyPlayer>().UpdateRange(players);
        }

        private async Task<object> BuildTournamentDetailsAsync(MultiTournament tournament)
        {
            var teamsWithPlayers = new List<object>();

            foreach (var team in tournament.Teams)
            {
                var playerIds = JsonSerializer.Deserialize<List<int>>(team.PlayerIds) ?? new List<int>();
                var players = await _unitOfWork.GetRepository<FriendlyPlayer>()
                    .GetAll()
                    .Where(p => playerIds.Contains(p.PlayerId))
                    .ToListAsync();

                var teamPlayers = playerIds.Select(id =>
                {
                    var player = players.FirstOrDefault(p => p.PlayerId == id);
                    return new
                    {
                        PlayerId = id,
                        FullName = player?.FullName ?? $"Player {id}",
                        IsActive = player?.IsActive ?? false
                    };
                }).ToList();

                teamsWithPlayers.Add(new
                {
                    team.MultiTeamId,
                    team.TeamName,
                    team.TotalPoints,
                    team.Wins,
                    team.Draws,
                    team.Losses,
                    team.CreatedOn,
                    Players = teamPlayers
                });
            }

            return new
            {
                tournament.MultiTournamentId,
                tournament.Name,
                tournament.SystemOfScoring,
                tournament.TeamCount,
                tournament.PlayersPerTeam,
                Status = tournament.Status.ToString(),
                tournament.IsActive,
                tournament.CreatedOn,
                tournament.StartedOn,
                tournament.FinishedOn,
                tournament.ChampionTeamId,
                ChampionTeamName = tournament.ChampionTeam?.TeamName,
                Teams = teamsWithPlayers
            };
        }

        private static bool IsValidClassicScore(int score1, int score2)
        {
            return (score1, score2) is (3, 0) or (0, 3) or (1, 1);
        }

        #endregion
    }
}
