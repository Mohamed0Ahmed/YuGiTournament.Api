using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Models.ViewModels;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class ClassicMatchService : IClassicMatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClassicMatchService> _logger;
        private readonly IPlayerRankingService _playerRankingService;

        public ClassicMatchService(IUnitOfWork unitOfWork, ILogger<ClassicMatchService> logger, IPlayerRankingService playerRankingService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _playerRankingService = playerRankingService;
        }

        public async Task<IEnumerable<MatchViewModel>> GetAllMatchesAsync()
        {
            var league = await _unitOfWork.GetRepository<Models.LeagueId>()
                .Find(x => x.IsFinished == false && x.SystemOfLeague == Models.SystemOfLeague.Classic)
                .FirstOrDefaultAsync();
            if (league == null)
                return [];

            return await GetMatchesByLeagueAsync(league.Id);
        }

        public async Task<MatchViewModel?> GetMatchByIdAsync(int matchId)
        {
            return await _unitOfWork.GetRepository<Models.Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.MatchId == matchId)
                .Select(m => new MatchViewModel
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ApiResponse> ResetMatchByIdAsync(int matchId)
        {
            try
            {
                using var transaction = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
                var match = await _unitOfWork.GetRepository<Models.Match>()
                    .GetAll()
                    .Include(m => m.Player1)
                    .Include(m => m.Player2)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                    return new ApiResponse(false, "المباراة غير موجودة");

                if (!match.IsCompleted)
                    return new ApiResponse(false, "المباراة لم تُلعب بعد");

                var player1 = match.Player1;
                var player2 = match.Player2;

                if (player1 == null || player2 == null)
                    return new ApiResponse(false, "اللاعبين غير موجودين");

                // عكس احتساب النقاط بناءً على النتيجة السابقة
                if (match.Score1 == 1 && match.Score2 == 1)
                {
                    // كانت تعادل
                    player1.Draws -= 1;
                    player2.Draws -= 1;
                    player1.Points -= 1;
                    player2.Points -= 1;
                }
                else if (match.Score1 == 3 && match.Score2 == 0)
                {
                    // فاز اللاعب 1
                    player1.Wins -= 1;
                    player1.Points -= 3;
                    player2.Losses -= 1;
                }
                else if (match.Score2 == 3 && match.Score1 == 0)
                {
                    // فاز اللاعب 2
                    player2.Wins -= 1;
                    player2.Points -= 3;
                    player1.Losses -= 1;
                }
                else
                {
                    return new ApiResponse(false, "نتيجة المباراة غير معروفة أو لم يتم احتسابها بنظام الكلاسيك");
                }

                player1.MatchesPlayed -= 1;
                player2.MatchesPlayed -= 1;
                player1.WinRate = player1.MatchesPlayed > 0 ? (double)player1.Wins / player1.MatchesPlayed * 100 : 0;
                player2.WinRate = player2.MatchesPlayed > 0 ? (double)player2.Wins / player2.MatchesPlayed * 100 : 0;

                match.Score1 = 0;
                match.Score2 = 0;
                match.IsCompleted = false;

                _unitOfWork.GetRepository<Models.Player>().Update(player1);
                _unitOfWork.GetRepository<Models.Player>().Update(player2);
                _unitOfWork.GetRepository<Models.Match>().Update(match);

                await _unitOfWork.SaveChangesAsync();

                // تحديث ترتيب اللاعبين
                var leaguePlayers = await _unitOfWork.GetRepository<Models.Player>()
                    .GetAll().Where(p => p.LeagueNumber == match.LeagueNumber).ToListAsync();
                var leagueMatches = await _unitOfWork.GetRepository<Models.Match>()
                    .GetAll().Where(m => m.LeagueNumber == match.LeagueNumber).ToListAsync();
                var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                foreach (var p in rankedPlayers)
                {
                    var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId);
                    dbPlayer.Rank = p.Rank;
                }
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse(true, "تم إعادة تعيين نتيجة المباراة بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while resetting classic match {MatchId}", matchId);
                return new ApiResponse(false, $"حصل خطأ أثناء إعادة تعيين نتيجة المباراة: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            if (resultDto == null)
                return new ApiResponse(false, "بيانات نتيجة المباراة غير صالحة");

            try
            {
                using var transaction = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
                var match = await _unitOfWork.GetRepository<Models.Match>()
                    .GetAll()
                    .Include(m => m.Player1)
                    .Include(m => m.Player2)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                    return new ApiResponse(false, "المباراة غير موجودة");

                if (match.IsCompleted)
                    return new ApiResponse(false, "تم احتساب نتيجة هذه المباراة بالفعل");

                var player1 = match.Player1;
                var player2 = match.Player2;

                if (player1 == null || player2 == null)
                    return new ApiResponse(false, "اللاعبين غير موجودين");

                // تحقق مما إذا كانت المباراة في دور إقصائي
                var isKnockoutStage = match.Stage == TournamentStage.QuarterFinals ||
                                     match.Stage == TournamentStage.SemiFinals ||
                                     match.Stage == TournamentStage.Final;

                if (isKnockoutStage)
                {
                    // في الأدوار الإقصائية، لا نضيف نقاط للاعبين، فقط نسجل الفائز
                    if (resultDto.WinnerId == null)
                    {
                        return new ApiResponse(false, "لا يمكن أن تكون هناك تعادل في الأدوار الإقصائية");
                    }
                    else if (resultDto.WinnerId == player1.PlayerId || resultDto.WinnerId == player2.PlayerId)
                    {
                        var winner = resultDto.WinnerId == player1.PlayerId ? player1 : player2;
                        var loser = resultDto.WinnerId == player1.PlayerId ? player2 : player1;

                        // تسجيل الفائز في المباراة
                        match.WinnerId = winner!.PlayerId;
                        match.IsCompleted = true;

                        // تعيين النتيجة (3 للفائز، 0 للخاسر) - نظام الكلاسيك
                        if (winner.PlayerId == match.Player1Id)
                        {
                            match.Score1 = 3;
                            match.Score2 = 0;
                        }
                        else
                        {
                            match.Score1 = 0;
                            match.Score2 = 3;
                        }

                        _unitOfWork.GetRepository<Models.Match>().Update(match);
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        await CheckAndAdvanceStageAsync(match.LeagueNumber, match.Stage);

                        return new ApiResponse(true, $"تم تسجيل فوز {winner.FullName} في {match.Stage}");
                    }
                    else
                    {
                        return new ApiResponse(false, "معرّف الفائز غير صحيح. يجب أن يكون أحد اللاعبين في المباراة");
                    }
                }
                else
                {
                    // في مرحلة المجموعات، نستخدم النظام العادي
                    string responseMessage;

                    if (resultDto.WinnerId == null)
                    {
                        // تعادل
                        player1.Draws += 1;
                        player2.Draws += 1;
                        player1.Points += 1;
                        player2.Points += 1;
                        match.Score1 = 1;
                        match.Score2 = 1;
                        responseMessage = "تم احتساب تعادل: نقطة لكل لاعب";
                    }
                    else if (resultDto.WinnerId == player1.PlayerId || resultDto.WinnerId == player2.PlayerId)
                    {
                        var winner = resultDto.WinnerId == player1.PlayerId ? player1 : player2;
                        var loser = resultDto.WinnerId == player1.PlayerId ? player2 : player1;

                        winner.Wins += 1;
                        winner.Points += 3;
                        loser.Losses += 1;
                        match.Score1 = winner == player1 ? 3 : 0;
                        match.Score2 = winner == player2 ? 3 : 0;
                        responseMessage = $"تم احتساب فوز: {winner.FullName} حصل على 3 نقاط";
                    }
                    else
                    {
                        return new ApiResponse(false, "معرّف الفائز غير صحيح. يجب أن يكون أحد اللاعبين في المباراة");
                    }

                    player1.MatchesPlayed += 1;
                    player2.MatchesPlayed += 1;
                    player1.WinRate = player1.MatchesPlayed > 0 ? (double)player1.Wins / player1.MatchesPlayed * 100 : 0;
                    player2.WinRate = player2.MatchesPlayed > 0 ? (double)player2.Wins / player2.MatchesPlayed * 100 : 0;

                    match.IsCompleted = true;

                    _unitOfWork.GetRepository<Models.Player>().Update(player1);
                    _unitOfWork.GetRepository<Models.Player>().Update(player2);
                    _unitOfWork.GetRepository<Models.Match>().Update(match);

                    await _unitOfWork.SaveChangesAsync();

                    // تحديث ترتيب اللاعبين
                    var leaguePlayers = await _unitOfWork.GetRepository<Models.Player>()
                        .GetAll().Where(p => p.LeagueNumber == match.LeagueNumber).ToListAsync();
                    var leagueMatches = await _unitOfWork.GetRepository<Models.Match>()
                        .GetAll().Where(m => m.LeagueNumber == match.LeagueNumber).ToListAsync();
                    var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                    foreach (var p in rankedPlayers)
                    {
                        var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId);
                        dbPlayer.Rank = p.Rank;
                    }
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    await CheckAndAdvanceStageAsync(match.LeagueNumber, match.Stage);

                    return new ApiResponse(true, responseMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating classic match {MatchId}", matchId);
                return new ApiResponse(false, $"حصل خطأ أثناء تحديث نتيجة المباراة: {ex.Message}");
            }
        }

        public async Task<IEnumerable<LeagueWithMatchesViewModel>> GetAllLeaguesWithMatchesAsync()
        {
            var leagues = await _unitOfWork.GetRepository<Models.LeagueId>()
                .GetAll()
                .Where(l => !l.IsDeleted && l.SystemOfLeague == Models.SystemOfLeague.Classic)
                .ToListAsync();

            if (leagues == null || leagues.Count == 0)
                return new List<LeagueWithMatchesViewModel>();

            var result = new List<LeagueWithMatchesViewModel>();
            foreach (var league in leagues)
            {
                var matches = await GetMatchesByLeagueAsync(league.Id);
                result.Add(new LeagueWithMatchesViewModel
                {
                    LeagueId = league.Id,
                    LeagueName = league.Name,
                    LeagueDescription = league.Description,
                    LeagueType = league.TypeOfLeague.ToString(),
                    SystemOfLeague = league.SystemOfLeague.ToString(), // Added
                    IsFinished = league.IsFinished,
                    CreatedOn = league.CreatedOn,
                    Matches = matches.ToList()
                });
            }

            return result;
        }

        // Helper method
        private async Task<List<MatchViewModel>> GetMatchesByLeagueAsync(int leagueId)
        {
            return await _unitOfWork.GetRepository<Models.Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.LeagueNumber == leagueId)
                .Select(m => new MatchViewModel
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id,
                    TournamentStage = m.Stage.ToString()
                })
                .ToListAsync();
        }

        private async Task CheckAndAdvanceStageAsync(int leagueId, TournamentStage currentStage)
        {
            // تحقق من نوع البطولة أولاً
            var league = await _unitOfWork.GetRepository<Models.LeagueId>()
                .Find(l => l.Id == leagueId)
                .FirstOrDefaultAsync();

            if (league == null) return;

            // لا تنشئ مباريات جديدة إلا إذا كانت البطولة من نوع Groups
            if (league.TypeOfLeague != Models.LeagueType.Groups) return;

            if (currentStage == TournamentStage.Final || currentStage == TournamentStage.GroupStage) return;

            var stageMatches = await _unitOfWork.GetRepository<Models.Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == currentStage && !m.IsDeleted)
                .ToListAsync();

            if (stageMatches.Any(m => !m.IsCompleted)) return;

            // في الأدوار الإقصائية، نستخدم WinnerId لتحديد الفائزين
            var winners = stageMatches.Select(m => m.WinnerId ?? (m.Score1 > m.Score2 ? m.Player1Id : m.Player2Id)).ToList();
            var nextStage = currentStage switch
            {
                TournamentStage.QuarterFinals => TournamentStage.SemiFinals,
                TournamentStage.SemiFinals => TournamentStage.Final,
                _ => TournamentStage.Final // Should not happen
            };

            var newMatches = new List<Models.Match>();
            for (int i = 0; i < winners.Count; i += 2)
            {
                newMatches.Add(new Models.Match
                {
                    Player1Id = winners[i],
                    Player2Id = winners[i + 1],
                    LeagueNumber = leagueId,
                    Stage = nextStage
                });
            }

            if (newMatches.Any())
            {
                await _unitOfWork.GetRepository<Models.Match>().AddRangeAsync(newMatches);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}