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

            return new ApiResponse(true, "Tournament created successfully");
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

                    // Find the team with highest points (champion team)
                    var championTeam = tournament.Teams
                        .OrderByDescending(t => t.TotalPoints)
                        .ThenByDescending(t => t.Wins)
                        .ThenBy(t => t.Losses)
                        .First();

                    // Set champion team
                    tournament.ChampionTeamId = championTeam.MultiTeamId;

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

            // Check for duplicate team name in current tournament
            var duplicateName = tournament.Teams
                .Any(t => t.TeamName.ToLower() == teamName.ToLower());

            if (duplicateName)
                return new ApiResponse(false, "Team name already exists");

            // Check for duplicate players in same tournament
            var existingPlayerIds = tournament.Teams
                .SelectMany(t => SafeDeserializePlayerIds(t.PlayerIds))
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

            return new ApiResponse(true, "Team created successfully");
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
                    .SelectMany(t => SafeDeserializePlayerIds(t.PlayerIds))
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

            var currentPlayerIds = SafeDeserializePlayerIds(team.PlayerIds);

            if (!currentPlayerIds.Contains(replacedPlayerId))
                return new ApiResponse(false, "Player not found in team");

            // Check if new player is already in tournament
            var allTeams = await _unitOfWork.GetRepository<MultiTeam>()
                .GetAll()
                .Where(t => t.MultiTournamentId == team.MultiTournamentId)
                .ToListAsync();

            var allPlayerIds = allTeams
                .SelectMany(t => SafeDeserializePlayerIds(t.PlayerIds))
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

        public async Task<ApiResponse> RecordMatchResultAsync(int matchId, int? winnerId, double? score1, double? score2)
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
                // Classic system: Only winnerId is required
                if (!winnerId.HasValue)
                    return new ApiResponse(false, "Winner ID is required for Classic system");

                // Validate winnerId is one of the match players
                if (winnerId != match.Player1Id && winnerId != match.Player2Id)
                    return new ApiResponse(false, "Winner must be one of the match players");

                // Set scores: Winner gets 3, Loser gets 0
                if (winnerId == match.Player1Id)
                {
                    match.Score1 = 3;
                    match.Score2 = 0;
                }
                else
                {
                    match.Score1 = 0;
                    match.Score2 = 3;
                }

                match.WinnerId = winnerId;
            }
            else // Points system
            {
                // Points system: Both scores are required
                if (!score1.HasValue || !score2.HasValue)
                    return new ApiResponse(false, "Both scores are required for Points system");

                if (score1.Value < 0 || score2.Value < 0)
                    return new ApiResponse(false, "Scores cannot be negative");

                // Get max rounds per match from game rules (database or default)
                var maxRoundsPerMatch = await _gameRulesService.GetMaxRoundsPerMatchAsync();

                // Validate that total scores equal max rounds per match
                if (score1.Value + score2.Value != maxRoundsPerMatch)
                    return new ApiResponse(false, $"Total scores ({score1.Value + score2.Value}) must equal max rounds per match ({maxRoundsPerMatch})");

                match.Score1 = score1;
                match.Score2 = score2;

                // Determine winner automatically
                if (score1 > score2)
                    match.WinnerId = match.Player1Id;
                else if (score2 > score1)
                    match.WinnerId = match.Player2Id;
                else
                    match.WinnerId = null; // Draw
            }

            match.IsCompleted = true;
            match.CompletedOn = DateTime.UtcNow;

            // Update team statistics
            await UpdateTeamStatisticsAsync(match);

            _unitOfWork.GetRepository<MultiMatch>().Update(match);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Match result recorded successfully");
        }

        public async Task<ApiResponse> UndoMatchResultAsync(int matchId)
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
                return new ApiResponse(false, "Cannot undo results for non-started tournament");

            if (!match.IsCompleted)
                return new ApiResponse(false, "Match is not completed yet");

            // Save old scores for reverting team statistics
            var oldScore1 = match.Score1 ?? 0;
            var oldScore2 = match.Score2 ?? 0;
            var oldWinnerId = match.WinnerId;

            // Reset match to initial state
            match.Score1 = 0;
            match.Score2 = 0;
            match.WinnerId = null;
            match.IsCompleted = false;
            match.CompletedOn = null;

            // Revert team statistics
            await RevertTeamStatisticsAsync(match, oldScore1, oldScore2, oldWinnerId);

            _unitOfWork.GetRepository<MultiMatch>().Update(match);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "Match result undone successfully");
        }

        public async Task<ApiResponse> GetPlayerMatchesAsync(int playerId)
        {
            // First check if player exists
            var player = await _unitOfWork.GetRepository<FriendlyPlayer>()
                .GetAll()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player == null)
                return new ApiResponse(false, "Player not found");

            // Get active tournament
            var activeTournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .FirstOrDefaultAsync(t => t.IsActive);

            if (activeTournament == null)
                return new ApiResponse(false, "No active tournament found");

            // Get all matches for this player in the active tournament
            var matches = await _unitOfWork.GetRepository<MultiMatch>()
                .GetAll()
                .Where(m => m.MultiTournamentId == activeTournament.MultiTournamentId &&
                           (m.Player1Id == playerId || m.Player2Id == playerId))
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Winner)
                .OrderBy(m => m.Team1Id)
                .ThenBy(m => m.Team2Id)
                .Select(m => new
                {
                    m.MultiMatchId,
                    m.Player1Id,
                    Player1Name = m.Player1!.FullName,
                    m.Player2Id,
                    Player2Name = m.Player2!.FullName,
                    m.Team1Id,
                    Team1Name = m.Team1!.TeamName,
                    m.Team2Id,
                    Team2Name = m.Team2!.TeamName,
                    m.Score1,
                    m.Score2,
                    m.WinnerId,
                    WinnerName = m.Winner != null ? m.Winner.FullName : null,
                    m.IsCompleted,
                    m.CompletedOn
                })
                .ToListAsync();

            var playerMatchData = new
            {
                PlayerId = playerId,
                PlayerName = player.FullName,
                TournamentId = activeTournament.MultiTournamentId,
                TournamentName = activeTournament.Name,
                TotalMatches = matches.Count,
                CompletedMatches = matches.Count(m => m.IsCompleted),
                PendingMatches = matches.Count(m => !m.IsCompleted),
                Matches = matches
            };

            return new ApiResponse<object>(true, "Player matches retrieved successfully", playerMatchData);
        }

        #endregion

        #region Data Retrieval

        public async Task<ApiResponse> GetActiveTournamentAsync()
        {
            var tournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .Include(t => t.Teams)
                .Include(t => t.ChampionTeam)
                .FirstOrDefaultAsync(t => t.IsActive);

            if (tournament == null)
                return new ApiResponse(false, "No active tournament");

            var tournamentData = await BuildTournamentDetailsAsync(tournament);
            return new ApiResponse<object>(true, "Active tournament found", tournamentData);
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

            var tournamentData = await BuildTournamentDetailsAsync(tournament);
            return new ApiResponse<object>(true, "Tournament found", tournamentData);
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

            return new ApiResponse<List<object>>(true, "Tournaments retrieved successfully", result);
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
                .Include(m => m.Winner)
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
                        m.WinnerId,
                        WinnerName = m.Winner?.FullName,
                        m.IsCompleted,
                        m.CompletedOn
                    }).ToList()
                })
                .ToList();

            return new ApiResponse<object>(true, "Tournament matches retrieved successfully", fixtureGroups);
        }

        public async Task<ApiResponse> GetActiveTournamentMatchesAsync()
        {
            var activeTournament = await _unitOfWork.GetRepository<MultiTournament>()
                .GetAll()
                .FirstOrDefaultAsync(t => t.IsActive);

            if (activeTournament == null)
                return new ApiResponse(false, "No active tournament found");

            // Use the existing method to get matches for the active tournament
            return await GetTournamentMatchesAsync(activeTournament.MultiTournamentId);
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

            var standingsData = new
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                Status = tournament.Status.ToString(),
                ChampionTeamId = tournament.ChampionTeamId,
                Standings = standings
            };

            return new ApiResponse<object>(true, "Tournament standings retrieved successfully", standingsData);
        }

        #endregion

        #region Player Management

        public async Task<ApiResponse> GetAllPlayersAsync()
        {
            var players = await _unitOfWork.GetRepository<FriendlyPlayer>()
                .GetAll()
                .Where(p => p.IsActive)
                .OrderBy(p => p.FullName)
                .Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.IsActive,
                    p.CreatedOn,
                    p.MultiParticipations,
                    p.MultiTitlesWon
                })
                .ToListAsync();

            return new ApiResponse<object>(true, "Players retrieved successfully", players);
        }

        public async Task<ApiResponse> GetPlayerByIdAsync(int playerId)
        {
            var player = await _unitOfWork.GetRepository<FriendlyPlayer>()
                .GetAll()
                .Where(p => p.PlayerId == playerId)
                .Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.IsActive,
                    p.CreatedOn,
                    p.MultiParticipations,
                    p.MultiTitlesWon
                })
                .FirstOrDefaultAsync();

            if (player == null)
                return new ApiResponse(false, "Player not found");

            return new ApiResponse<object>(true, "Player found", player);
        }

        public async Task<ApiResponse> AddPlayerAsync(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new ApiResponse(false, "Player name is required");

            // Check if player with same name already exists
            var existingPlayer = await _unitOfWork.GetRepository<FriendlyPlayer>()
                .GetAll()
                .FirstOrDefaultAsync(p => p.FullName.ToLower() == fullName.ToLower());

            if (existingPlayer != null)
                return new ApiResponse(false, "Player with this name already exists");

            var player = new FriendlyPlayer
            {
                FullName = fullName,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                MultiParticipations = 0,
                MultiTitlesWon = 0
            };

            await _unitOfWork.GetRepository<FriendlyPlayer>().AddAsync(player);
            await _unitOfWork.SaveChangesAsync();

            var playerData = new
            {
                player.PlayerId,
                player.FullName,
                player.IsActive,
                player.CreatedOn,
                player.MultiParticipations,
                player.MultiTitlesWon
            };

            return new ApiResponse<object>(true, "Player added successfully", playerData);
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
                var playerIds = SafeDeserializePlayerIds(team.PlayerIds);
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

                    var team1Players = SafeDeserializePlayerIds(team1.PlayerIds);
                    var team2Players = SafeDeserializePlayerIds(team2.PlayerIds);

                    // Every player from team1 plays every player from team2
                    // Note: In Points system, total score must equal maxRoundsPerMatch from GameRules
                    // This is different from the old system where total score = team1Players.Count × team2Players.Count
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

            var score1 = match.Score1!.Value;
            var score2 = match.Score2!.Value;

            team1.TotalPoints += score1;
            team2.TotalPoints += score2;

            // Update wins/draws/losses for both systems
            if (match.WinnerId == null) // Draw
            {
                team1.Draws++;
                team2.Draws++;
            }
            else if (match.WinnerId == match.Player1Id) // Player1 won
            {
                team1.Wins++;
                team2.Losses++;
            }
            else // Player2 won
            {
                team2.Wins++;
                team1.Losses++;
            }

            _unitOfWork.GetRepository<MultiTeam>().Update(team1);
            _unitOfWork.GetRepository<MultiTeam>().Update(team2);

            return Task.CompletedTask;
        }

        private Task RevertTeamStatisticsAsync(MultiMatch match, double oldScore1, double oldScore2, int? oldWinnerId)
        {
            var team1 = match.Team1!;
            var team2 = match.Team2!;

            // Revert total points
            team1.TotalPoints -= oldScore1;
            team2.TotalPoints -= oldScore2;

            // Revert wins/draws/losses
            if (oldWinnerId == null) // Was draw
            {
                team1.Draws--;
                team2.Draws--;
            }
            else if (oldWinnerId == match.Player1Id) // Player1 had won
            {
                team1.Wins--;
                team2.Losses--;
            }
            else // Player2 had won
            {
                team2.Wins--;
                team1.Losses--;
            }

            _unitOfWork.GetRepository<MultiTeam>().Update(team1);
            _unitOfWork.GetRepository<MultiTeam>().Update(team2);

            return Task.CompletedTask;
        }

        private async Task UpdatePlayerStatisticsAsync(MultiTournament tournament)
        {
            // Update FriendlyPlayer statistics
            var allPlayerIds = tournament.Teams
                .SelectMany(t => SafeDeserializePlayerIds(t.PlayerIds))
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
                var championPlayerIds = SafeDeserializePlayerIds(championTeam.PlayerIds);

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
                var playerIds = SafeDeserializePlayerIds(team.PlayerIds);
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

        private static List<int> SafeDeserializePlayerIds(string playerIds)
        {
            if (string.IsNullOrWhiteSpace(playerIds))
                return new List<int>();

            try
            {
                return JsonSerializer.Deserialize<List<int>>(playerIds) ?? new List<int>();
            }
            catch (JsonException)
            {
                return new List<int>();
            }
        }

        #endregion
    }
}
