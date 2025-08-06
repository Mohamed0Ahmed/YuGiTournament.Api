using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services
{
    public class FriendlyMatchService : IFriendlyMatchService
    {
        private readonly ApplicationDbContext _context;

        public FriendlyMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Player Management

        public async Task<IEnumerable<FriendlyPlayerDto>> GetAllFriendlyPlayersAsync()
        {
            var players = await _context.FriendlyPlayers
                .Where(p => p.IsActive)
                .Select(p => new FriendlyPlayerDto
                {
                    PlayerId = p.PlayerId,
                    FullName = p.FullName,
                    CreatedOn = p.CreatedOn,
                    IsActive = p.IsActive,
                    TotalMatches = p.Player1Matches.Count(m => !m.IsDeleted) + p.Player2Matches.Count(m => !m.IsDeleted),
                    TotalWins = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score > m.Player2Score) +
                               p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score > m.Player1Score),
                    TotalLosses = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score < m.Player2Score) +
                                 p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score < m.Player1Score),
                    TotalDraws = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score == m.Player2Score) +
                                p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score == m.Player1Score),
                    WinRate = 0 // Will be calculated below
                })
                .ToListAsync();

            // Calculate win rates
            foreach (var player in players)
            {
                player.WinRate = player.TotalMatches > 0 ? (double)player.TotalWins / player.TotalMatches * 100 : 0;
            }

            return players;
        }

        public async Task<FriendlyPlayerDto?> GetFriendlyPlayerByIdAsync(int playerId)
        {
            var player = await _context.FriendlyPlayers
                .Where(p => p.PlayerId == playerId && p.IsActive)
                .Select(p => new FriendlyPlayerDto
                {
                    PlayerId = p.PlayerId,
                    FullName = p.FullName,
                    CreatedOn = p.CreatedOn,
                    IsActive = p.IsActive,
                    TotalMatches = p.Player1Matches.Count(m => !m.IsDeleted) + p.Player2Matches.Count(m => !m.IsDeleted),
                    TotalWins = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score > m.Player2Score) +
                               p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score > m.Player1Score),
                    TotalLosses = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score < m.Player2Score) +
                                 p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score < m.Player1Score),
                    TotalDraws = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score == m.Player2Score) +
                                p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score == m.Player1Score),
                    WinRate = 0
                })
                .FirstOrDefaultAsync();

            if (player != null)
            {
                player.WinRate = player.TotalMatches > 0 ? (double)player.TotalWins / player.TotalMatches * 100 : 0;
            }

            return player;
        }

        public async Task<ApiResponse> AddFriendlyPlayerAsync(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return new ApiResponse(false, "اسم اللاعب مطلوب");
            }

            var existingPlayer = await _context.FriendlyPlayers
                .FirstOrDefaultAsync(p => p.FullName.ToLower() == fullName.ToLower() && p.IsActive);

            if (existingPlayer != null)
            {
                return new ApiResponse(false, "اللاعب موجود بالفعل");
            }

            var player = new FriendlyPlayer
            {
                FullName = fullName.Trim(),
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            _context.FriendlyPlayers.Add(player);
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم إضافة اللاعب بنجاح");
        }

        public async Task<ApiResponse> DeleteFriendlyPlayerAsync(int playerId)
        {
            var player = await _context.FriendlyPlayers.FindAsync(playerId);
            if (player == null)
            {
                return new ApiResponse(false, "اللاعب غير موجود");
            }

            // Check if player has any matches
            var hasMatches = await _context.FriendlyMatches
                .AnyAsync(m => (m.Player1Id == playerId || m.Player2Id == playerId) && !m.IsDeleted);

            if (hasMatches)
            {
                return new ApiResponse(false, "لا يمكن حذف اللاعب لوجود مباريات له");
            }

            _context.FriendlyPlayers.Remove(player);
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم حذف اللاعب بنجاح");
        }

        public async Task<ApiResponse> DeactivateFriendlyPlayerAsync(int playerId)
        {
            var player = await _context.FriendlyPlayers.FindAsync(playerId);
            if (player == null)
            {
                return new ApiResponse(false, "اللاعب غير موجود");
            }

            player.IsActive = false;
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم إلغاء تفعيل اللاعب بنجاح");
        }

        #endregion

        #region Match Management

        public async Task<IEnumerable<FriendlyMatchHistoryDto>> GetAllFriendlyMatchesAsync()
        {
            return await _context.FriendlyMatches
                .Where(m => !m.IsDeleted)
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .OrderByDescending(m => m.PlayedOn)
                .Select(m => new FriendlyMatchHistoryDto
                {
                    MatchId = m.MatchId,
                    PlayedOn = m.PlayedOn,
                    Player1Score = m.Player1Score,
                    Player2Score = m.Player2Score,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Winner = m.Player1Score > m.Player2Score ? m.Player1.FullName :
                            m.Player2Score > m.Player1Score ? m.Player2.FullName : "تعادل"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<FriendlyMatchHistoryDto>> GetFriendlyMatchesBetweenPlayersAsync(int player1Id, int player2Id)
        {
            return await _context.FriendlyMatches
                .Where(m => !m.IsDeleted &&
                           ((m.Player1Id == player1Id && m.Player2Id == player2Id) ||
                            (m.Player1Id == player2Id && m.Player2Id == player1Id)))
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .OrderByDescending(m => m.PlayedOn)
                .Select(m => new FriendlyMatchHistoryDto
                {
                    MatchId = m.MatchId,
                    PlayedOn = m.PlayedOn,
                    Player1Score = m.Player1Score,
                    Player2Score = m.Player2Score,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Winner = m.Player1Score > m.Player2Score ? m.Player1.FullName :
                            m.Player2Score > m.Player1Score ? m.Player2.FullName : "تعادل"
                })
                .ToListAsync();
        }

        public async Task<ApiResponse> RecordFriendlyMatchAsync(RecordFriendlyMatchDto matchDto)
        {
            // Validate players exist
            var player1 = await _context.FriendlyPlayers.FindAsync(matchDto.Player1Id);
            var player2 = await _context.FriendlyPlayers.FindAsync(matchDto.Player2Id);

            if (player1 == null || player2 == null)
            {
                return new ApiResponse(false, "أحد اللاعبين غير موجود");
            }

            if (!player1.IsActive || !player2.IsActive)
            {
                return new ApiResponse(false, "أحد اللاعبين غير مفعل");
            }

            if (matchDto.Player1Id == matchDto.Player2Id)
            {
                return new ApiResponse(false, "لا يمكن للاعب أن يلعب ضد نفسه");
            }

            if (matchDto.Player1Score < 0 || matchDto.Player2Score < 0)
            {
                return new ApiResponse(false, "النتيجة يجب أن تكون صفر أو أكثر");
            }

            var match = new FriendlyMatch
            {
                Player1Id = matchDto.Player1Id,
                Player2Id = matchDto.Player2Id,
                Player1Score = matchDto.Player1Score,
                Player2Score = matchDto.Player2Score,
                PlayedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.FriendlyMatches.Add(match);
            await _context.SaveChangesAsync();

            // Check if this is a shutout result (5-0)
            await CheckAndRecordShutoutResult(match);

            return new ApiResponse(true, "تم تسجيل المباراة بنجاح");
        }

        public async Task<ApiResponse> DeleteFriendlyMatchAsync(int matchId)
        {
            var match = await _context.FriendlyMatches.FindAsync(matchId);
            if (match == null)
            {
                return new ApiResponse(false, "المباراة غير موجودة");
            }

            match.IsDeleted = true;
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم حذف المباراة بنجاح");
        }

        public async Task<ApiResponse> UpdateFriendlyMatchAsync(int matchId, RecordFriendlyMatchDto matchDto)
        {
            var match = await _context.FriendlyMatches.FindAsync(matchId);
            if (match == null)
            {
                return new ApiResponse(false, "المباراة غير موجودة");
            }

            // Validate players exist
            var player1 = await _context.FriendlyPlayers.FindAsync(matchDto.Player1Id);
            var player2 = await _context.FriendlyPlayers.FindAsync(matchDto.Player2Id);

            if (player1 == null || player2 == null)
            {
                return new ApiResponse(false, "أحد اللاعبين غير موجود");
            }

            if (matchDto.Player1Score < 0 || matchDto.Player2Score < 0)
            {
                return new ApiResponse(false, "النتيجة يجب أن تكون صفر أو أكثر");
            }

            match.Player1Id = matchDto.Player1Id;
            match.Player2Id = matchDto.Player2Id;
            match.Player1Score = matchDto.Player1Score;
            match.Player2Score = matchDto.Player2Score;

            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم تحديث المباراة بنجاح");
        }

        #endregion

        #region Statistics and Analysis

        public async Task<OverallScoreDto?> GetOverallScoreBetweenPlayersAsync(int player1Id, int player2Id)
        {
            var player1 = await _context.FriendlyPlayers.FindAsync(player1Id);
            var player2 = await _context.FriendlyPlayers.FindAsync(player2Id);

            if (player1 == null || player2 == null)
            {
                return null;
            }

            var matches = await _context.FriendlyMatches
                .Where(m => !m.IsDeleted &&
                           ((m.Player1Id == player1Id && m.Player2Id == player2Id) ||
                            (m.Player1Id == player2Id && m.Player2Id == player1Id)))
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .OrderByDescending(m => m.PlayedOn)
                .ToListAsync();

            if (!matches.Any())
            {
                return new OverallScoreDto
                {
                    Player1Name = player1.FullName,
                    Player2Name = player2.FullName,
                    Player1TotalScore = 0,
                    Player2TotalScore = 0,
                    TotalMatches = 0,
                    LeadingPlayer = "لا توجد مباريات",
                    ScoreDifference = 0,
                    Player1Wins = 0,
                    Player1Losses = 0,
                    Player1Draws = 0,
                    Player2Wins = 0,
                    Player2Losses = 0,
                    Player2Draws = 0,
                    Player1WinRate = 0,
                    Player2WinRate = 0,
                    MatchHistory = []
                };
            }

            int player1TotalScore = 0;
            int player2TotalScore = 0;
            int player1Wins = 0;
            int player1Losses = 0;
            int player1Draws = 0;

            foreach (var match in matches)
            {
                int player1MatchScore;
                int player2MatchScore;

                if (match.Player1Id == player1Id)
                {
                    player1MatchScore = match.Player1Score;
                    player2MatchScore = match.Player2Score;
                }
                else
                {
                    player1MatchScore = match.Player2Score;
                    player2MatchScore = match.Player1Score;
                }

                // Calculate total scores
                player1TotalScore += player1MatchScore;
                player2TotalScore += player2MatchScore;

                // Calculate wins/losses/draws
                if (player1MatchScore > player2MatchScore)
                {
                    player1Wins++;
                }
                else if (player1MatchScore < player2MatchScore)
                {
                    player1Losses++;
                }
                else
                {
                    player1Draws++;
                }
            }

            // Player2 stats are the inverse of Player1
            int player2Wins = player1Losses;
            int player2Losses = player1Wins;
            int player2Draws = player1Draws;

            // Calculate win rates
            double player1WinRate = matches.Count > 0 ? (double)player1Wins / matches.Count * 100 : 0;
            double player2WinRate = matches.Count > 0 ? (double)player2Wins / matches.Count * 100 : 0;

            var leadingPlayer = player1TotalScore > player2TotalScore ? player1.FullName :
                              player2TotalScore > player1TotalScore ? player2.FullName : "متساويان";

            // Create match history
            var matchHistory = matches.Select(m => new FriendlyMatchHistoryDto
            {
                MatchId = m.MatchId,
                PlayedOn = m.PlayedOn,
                Player1Score = m.Player1Score,
                Player2Score = m.Player2Score,
                Player1Name = m.Player1.FullName,
                Player2Name = m.Player2.FullName,
                Winner = m.Player1Score > m.Player2Score ? m.Player1.FullName :
                        m.Player2Score > m.Player1Score ? m.Player2.FullName : "تعادل"
            }).ToList();

            return new OverallScoreDto
            {
                Player1Name = player1.FullName,
                Player2Name = player2.FullName,
                Player1TotalScore = player1TotalScore,
                Player2TotalScore = player2TotalScore,
                TotalMatches = matches.Count,
                LeadingPlayer = leadingPlayer,
                ScoreDifference = Math.Abs(player1TotalScore - player2TotalScore),
                Player1Wins = player1Wins,
                Player1Losses = player1Losses,
                Player1Draws = player1Draws,
                Player2Wins = player2Wins,
                Player2Losses = player2Losses,
                Player2Draws = player2Draws,
                Player1WinRate = player1WinRate,
                Player2WinRate = player2WinRate,
                MatchHistory = matchHistory
            };
        }

        public async Task<FriendlyPlayerDto?> GetFriendlyPlayerStatsAsync(int playerId)
        {
            return await GetFriendlyPlayerByIdAsync(playerId);
        }

        public async Task<IEnumerable<FriendlyPlayerDto>> GetFriendlyPlayersRankingAsync()
        {
            var players = await GetAllFriendlyPlayersAsync();
            return players.OrderByDescending(p => p.TotalWins)
                         .ThenByDescending(p => p.WinRate)
                         .ThenByDescending(p => p.TotalMatches);
        }

        public async Task<IEnumerable<PlayerStatisticsDto>> GetPlayersStatisticsAsync()
        {
            var players = await _context.FriendlyPlayers
                .Where(p => p.IsActive)
                .Select(p => new PlayerStatisticsDto
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.FullName,
                    TotalMatches = p.Player1Matches.Count(m => !m.IsDeleted) + p.Player2Matches.Count(m => !m.IsDeleted),
                    Wins = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score > m.Player2Score) +
                           p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score > m.Player1Score),
                    Draws = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score == m.Player2Score) +
                            p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score == m.Player1Score),
                    Losses = p.Player1Matches.Count(m => !m.IsDeleted && m.Player1Score < m.Player2Score) +
                             p.Player2Matches.Count(m => !m.IsDeleted && m.Player2Score < m.Player1Score),
                    GoalsScored = p.Player1Matches.Where(m => !m.IsDeleted).Sum(m => m.Player1Score) +
                                 p.Player2Matches.Where(m => !m.IsDeleted).Sum(m => m.Player2Score),
                    GoalsConceded = p.Player1Matches.Where(m => !m.IsDeleted).Sum(m => m.Player2Score) +
                                   p.Player2Matches.Where(m => !m.IsDeleted).Sum(m => m.Player1Score),
                    WinRate = 0 // Will be calculated below
                })
                .ToListAsync();

            // Calculate win rates and order by wins
            foreach (var player in players)
            {
                player.WinRate = player.TotalMatches > 0 ? (double)player.Wins / player.TotalMatches * 100 : 0;
            }

            return players.OrderByDescending(p => p.Wins)
                         .ThenByDescending(p => p.WinRate)
                         .ThenByDescending(p => p.TotalMatches);
        }

        #endregion

        #region Shutout Results (5-0 matches)

        public async Task<IEnumerable<ShutoutResultDto>> GetAllShutoutResultsAsync()
        {
            return await _context.ShutoutResults
                .Where(s => !s.IsDeleted)
                .Include(s => s.Winner)
                .Include(s => s.Loser)
                .Include(s => s.Match)
                .OrderByDescending(s => s.AchievedOn)
                .Select(s => new ShutoutResultDto
                {
                    ShutoutId = s.ShutoutId,
                    MatchId = s.MatchId,
                    WinnerName = s.Winner.FullName,
                    LoserName = s.Loser.FullName,
                    AchievedOn = s.AchievedOn,
                    WinnerScore = 5,
                    LoserScore = 0,
                    MatchDetails = $"{s.Winner.FullName} 5-0 {s.Loser.FullName}"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ShutoutResultDto>> GetShutoutResultsByPlayerAsync(int playerId)
        {
            return await _context.ShutoutResults
                .Where(s => !s.IsDeleted && (s.WinnerId == playerId || s.LoserId == playerId))
                .Include(s => s.Winner)
                .Include(s => s.Loser)
                .Include(s => s.Match)
                .OrderByDescending(s => s.AchievedOn)
                .Select(s => new ShutoutResultDto
                {
                    ShutoutId = s.ShutoutId,
                    MatchId = s.MatchId,
                    WinnerName = s.Winner.FullName,
                    LoserName = s.Loser.FullName,
                    AchievedOn = s.AchievedOn,
                    WinnerScore = 5,
                    LoserScore = 0,
                    MatchDetails = $"{s.Winner.FullName} 5-0 {s.Loser.FullName}"
                })
                .ToListAsync();
        }

        public async Task<ShutoutResultDto?> GetShutoutResultByMatchAsync(int matchId)
        {
            return await _context.ShutoutResults
                .Where(s => !s.IsDeleted && s.MatchId == matchId)
                .Include(s => s.Winner)
                .Include(s => s.Loser)
                .Include(s => s.Match)
                .Select(s => new ShutoutResultDto
                {
                    ShutoutId = s.ShutoutId,
                    MatchId = s.MatchId,
                    WinnerName = s.Winner.FullName,
                    LoserName = s.Loser.FullName,
                    AchievedOn = s.AchievedOn,
                    WinnerScore = 5,
                    LoserScore = 0,
                    MatchDetails = $"{s.Winner.FullName} 5-0 {s.Loser.FullName}"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ApiResponse> DeleteShutoutResultAsync(int shutoutId)
        {
            var shutout = await _context.ShutoutResults.FindAsync(shutoutId);
            if (shutout == null)
            {
                return new ApiResponse(false, "النتيجة العريضة غير موجودة");
            }

            shutout.IsDeleted = true;
            await _context.SaveChangesAsync();

            return new ApiResponse(true, "تم حذف النتيجة العريضة بنجاح");
        }

        private async Task CheckAndRecordShutoutResult(FriendlyMatch match)
        {
            // Check if the match is a shutout (5-0)
            if ((match.Player1Score == 5 && match.Player2Score == 0) ||
                (match.Player1Score == 0 && match.Player2Score == 5))
            {
                int winnerId = match.Player1Score == 5 ? match.Player1Id : match.Player2Id;
                int loserId = match.Player1Score == 5 ? match.Player2Id : match.Player1Id;

                var shutoutResult = new ShutoutResult
                {
                    MatchId = match.MatchId,
                    WinnerId = winnerId,
                    LoserId = loserId,
                    AchievedOn = match.PlayedOn,
                    IsDeleted = false
                };

                _context.ShutoutResults.Add(shutoutResult);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Advanced Match Filtering and Pagination

        public async Task<PaginatedMatchResultDto> GetFilteredMatchesAsync(MatchFilterDto filter)
        {
            var query = _context.FriendlyMatches
                .Where(m => !m.IsDeleted)
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .AsQueryable();

            // Apply date filter
            query = ApplyDateFilter(query, filter.DateFilter);

            // Apply player filter if specified
            if (filter.PlayerId.HasValue)
            {
                query = query.Where(m => m.Player1Id == filter.PlayerId.Value || m.Player2Id == filter.PlayerId.Value);
            }

            // Apply opponent filter if specified
            if (filter.OpponentIds != null && filter.OpponentIds.Any())
            {
                if (filter.PlayerId.HasValue)
                {
                    // Player vs specific opponents
                    query = query.Where(m =>
                        (m.Player1Id == filter.PlayerId.Value && filter.OpponentIds.Contains(m.Player2Id)) ||
                        (m.Player2Id == filter.PlayerId.Value && filter.OpponentIds.Contains(m.Player1Id)));
                }
                else
                {
                    // Matches involving any of the specified players
                    query = query.Where(m =>
                        filter.OpponentIds.Contains(m.Player1Id) || filter.OpponentIds.Contains(m.Player2Id));
                }
            }

            return await BuildPaginatedResult(query, filter);
        }

        public async Task<PaginatedMatchResultDto> GetPlayerMatchesAsync(int playerId, MatchFilterDto filter)
        {
            var player = await _context.FriendlyPlayers.FindAsync(playerId);
            if (player == null)
            {
                return new PaginatedMatchResultDto
                {
                    Matches = [],
                    TotalMatches = 0,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false,
                    FilterSummary = "اللاعب غير موجود"
                };
            }

            filter.PlayerId = playerId;
            var result = await GetFilteredMatchesAsync(filter);
            result.FilterSummary = $"مباريات {player.FullName} - {GetDateFilterText(filter.DateFilter)}";

            return result;
        }

        public async Task<PaginatedMatchResultDto> GetPlayerVsOpponentsMatchesAsync(int playerId, List<int> opponentIds, MatchFilterDto filter)
        {
            var player = await _context.FriendlyPlayers.FindAsync(playerId);
            if (player == null)
            {
                return new PaginatedMatchResultDto
                {
                    Matches = [],
                    TotalMatches = 0,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false,
                    FilterSummary = "اللاعب غير موجود"
                };
            }

            var opponents = await _context.FriendlyPlayers
                .Where(p => opponentIds.Contains(p.PlayerId))
                .Select(p => p.FullName)
                .ToListAsync();

            filter.PlayerId = playerId;
            filter.OpponentIds = opponentIds;
            var result = await GetFilteredMatchesAsync(filter);
            result.FilterSummary = $"مباريات {player.FullName} ضد {string.Join(", ", opponents)} - {GetDateFilterText(filter.DateFilter)}";

            return result;
        }

        #endregion

        #region Helper Methods

        private IQueryable<FriendlyMatch> ApplyDateFilter(IQueryable<FriendlyMatch> query, DateFilter dateFilter)
        {
            var now = DateTime.UtcNow;
            var filterDate = dateFilter switch
            {
                DateFilter.Today => now.Date,
                DateFilter.Last3Days => now.AddDays(-3),
                DateFilter.LastWeek => now.AddDays(-7),
                DateFilter.LastMonth => now.AddMonths(-1),
                DateFilter.AllTime => DateTime.MinValue,
                _ => DateTime.MinValue
            };

            if (dateFilter != DateFilter.AllTime)
            {
                query = query.Where(m => m.PlayedOn >= filterDate);
            }

            return query;
        }

        private async Task<PaginatedMatchResultDto> BuildPaginatedResult(IQueryable<FriendlyMatch> query, MatchFilterDto filter)
        {
            var totalMatches = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalMatches / filter.PageSize);

            var matches = await query
                .OrderByDescending(m => m.PlayedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(m => new FriendlyMatchHistoryDto
                {
                    MatchId = m.MatchId,
                    PlayedOn = m.PlayedOn,
                    Player1Score = m.Player1Score,
                    Player2Score = m.Player2Score,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Winner = m.Player1Score > m.Player2Score ? m.Player1.FullName :
                            m.Player2Score > m.Player1Score ? m.Player2.FullName : "تعادل"
                })
                .ToListAsync();

            return new PaginatedMatchResultDto
            {
                Matches = matches,
                TotalMatches = totalMatches,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filter.Page > 1,
                HasNextPage = filter.Page < totalPages,
                FilterSummary = BuildFilterSummary(filter, totalMatches)
            };
        }

        private string GetDateFilterText(DateFilter dateFilter) => dateFilter switch
        {
            DateFilter.Today => "اليوم",
            DateFilter.Last3Days => "آخر 3 أيام",
            DateFilter.LastWeek => "آخر أسبوع",
            DateFilter.LastMonth => "آخر شهر",
            DateFilter.AllTime => "كل الأوقات",
            _ => "كل الأوقات"
        };

        private string BuildFilterSummary(MatchFilterDto filter, int totalMatches)
        {
            var dateText = GetDateFilterText(filter.DateFilter);
            return $"إجمالي {totalMatches} مباراة - {dateText}";
        }

        #endregion

        #region Advanced Shutout Filtering and Pagination

        public async Task<PaginatedShutoutResultDto> GetFilteredShutoutsAsync(ShutoutFilterDto filter)
        {
            var query = _context.ShutoutResults
                .Where(s => !s.IsDeleted)
                .Include(s => s.Winner)
                .Include(s => s.Loser)
                .Include(s => s.Match)
                .AsQueryable();

            // Apply date filter
            query = ApplyDateFilterForShutouts(query, filter.DateFilter);

            // Apply player filter if specified
            if (filter.PlayerId.HasValue)
            {
                query = ApplyPlayerFilterForShutouts(query, filter.PlayerId.Value, filter.PlayerRole);
            }

            // Apply multiple players filter if specified
            if (filter.PlayerIds != null && filter.PlayerIds.Any())
            {
                if (filter.PlayerRole == ShutoutPlayerRole.Winner)
                {
                    query = query.Where(s => filter.PlayerIds.Contains(s.WinnerId));
                }
                else if (filter.PlayerRole == ShutoutPlayerRole.Loser)
                {
                    query = query.Where(s => filter.PlayerIds.Contains(s.LoserId));
                }
                else // Any or null
                {
                    query = query.Where(s => filter.PlayerIds.Contains(s.WinnerId) || filter.PlayerIds.Contains(s.LoserId));
                }
            }

            return await BuildPaginatedShutoutResult(query, filter);
        }

        public async Task<PaginatedShutoutResultDto> GetPlayerShutoutsAsync(int playerId, ShutoutFilterDto filter)
        {
            var player = await _context.FriendlyPlayers.FindAsync(playerId);
            if (player == null)
            {
                return new PaginatedShutoutResultDto
                {
                    Shutouts = [],
                    TotalShutouts = 0,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false,
                    FilterSummary = "اللاعب غير موجود"
                };
            }

            filter.PlayerId = playerId;
            var result = await GetFilteredShutoutsAsync(filter);
            result.FilterSummary = $"نتائج صايمة {player.FullName} - {GetShutoutRoleText(filter.PlayerRole)} - {GetDateFilterText(filter.DateFilter)}";

            return result;
        }

        public async Task<PaginatedShutoutResultDto> GetPlayersShutoutsAsync(List<int> playerIds, ShutoutFilterDto filter)
        {
            var players = await _context.FriendlyPlayers
                .Where(p => playerIds.Contains(p.PlayerId))
                .Select(p => p.FullName)
                .ToListAsync();

            if (!players.Any())
            {
                return new PaginatedShutoutResultDto
                {
                    Shutouts = [],
                    TotalShutouts = 0,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false,
                    FilterSummary = "اللاعبين غير موجودين"
                };
            }

            filter.PlayerIds = playerIds;
            var result = await GetFilteredShutoutsAsync(filter);
            result.FilterSummary = $"نتائج صايمة {string.Join(", ", players)} - {GetShutoutRoleText(filter.PlayerRole)} - {GetDateFilterText(filter.DateFilter)}";

            return result;
        }

        #endregion

        #region Shutout Helper Methods

        private IQueryable<ShutoutResult> ApplyDateFilterForShutouts(IQueryable<ShutoutResult> query, DateFilter dateFilter)
        {
            var now = DateTime.UtcNow;
            var filterDate = dateFilter switch
            {
                DateFilter.Today => now.Date,
                DateFilter.Last3Days => now.AddDays(-3),
                DateFilter.LastWeek => now.AddDays(-7),
                DateFilter.LastMonth => now.AddMonths(-1),
                DateFilter.AllTime => DateTime.MinValue,
                _ => DateTime.MinValue
            };

            if (dateFilter != DateFilter.AllTime)
            {
                query = query.Where(s => s.AchievedOn >= filterDate);
            }

            return query;
        }

        private IQueryable<ShutoutResult> ApplyPlayerFilterForShutouts(IQueryable<ShutoutResult> query, int playerId, ShutoutPlayerRole? role)
        {
            return role switch
            {
                ShutoutPlayerRole.Winner => query.Where(s => s.WinnerId == playerId),
                ShutoutPlayerRole.Loser => query.Where(s => s.LoserId == playerId),
                ShutoutPlayerRole.Any or null => query.Where(s => s.WinnerId == playerId || s.LoserId == playerId),
                _ => query.Where(s => s.WinnerId == playerId || s.LoserId == playerId)
            };
        }

        private async Task<PaginatedShutoutResultDto> BuildPaginatedShutoutResult(IQueryable<ShutoutResult> query, ShutoutFilterDto filter)
        {
            var totalShutouts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalShutouts / filter.PageSize);

            var shutouts = await query
                .OrderByDescending(s => s.AchievedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new ShutoutResultDto
                {
                    ShutoutId = s.ShutoutId,
                    MatchId = s.MatchId,
                    WinnerName = s.Winner.FullName,
                    LoserName = s.Loser.FullName,
                    AchievedOn = s.AchievedOn,
                    WinnerScore = 5,
                    LoserScore = 0,
                    MatchDetails = $"{s.Winner.FullName} 5-0 {s.Loser.FullName}"
                })
                .ToListAsync();

            return new PaginatedShutoutResultDto
            {
                Shutouts = shutouts,
                TotalShutouts = totalShutouts,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filter.Page > 1,
                HasNextPage = filter.Page < totalPages,
                FilterSummary = BuildShutoutFilterSummary(filter, totalShutouts)
            };
        }

        private string GetShutoutRoleText(ShutoutPlayerRole? role) => role switch
        {
            ShutoutPlayerRole.Winner => "الانتصارات",
            ShutoutPlayerRole.Loser => "الهزائم",
            ShutoutPlayerRole.Any or null => "جميع النتائج",
            _ => "جميع النتائج"
        };

        private string BuildShutoutFilterSummary(ShutoutFilterDto filter, int totalShutouts)
        {
            var dateText = GetDateFilterText(filter.DateFilter);
            var roleText = GetShutoutRoleText(filter.PlayerRole);
            return $"إجمالي {totalShutouts} نتيجة صايمة - {roleText} - {dateText}";
        }

        #endregion
    }
}